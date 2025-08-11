using System.Reflection.PortableExecutable;

public abstract class BTNodeBase : IBTNode
{
    //public  string DebugDisplayName { get; protected set; } = "Unnamed Node";
    //who is responsible for doing this action

    public Agent? self { get; protected set; }
    // which tree does this node belong to
    public IBehaviorTree? OwningTree { get; protected set; }

    public Blackboard<FastName> LinkedBlackboard => OwningTree.LinkedBlackboard;
    // to keep track of the last status of the node
    public EBTNodeResult LastStatus { get; protected set; } = EBTNodeResult.Uninitialized;
    // to keep track of the tick phase of each node
    protected EBTNodeTickPhase CurrentTickPhase { get; set; } = EBTNodeTickPhase.WaitingForNextTick;
    // to store the list of services of this node
    protected List<BTServiceBase>? AlwaysOnServices;
    protected List<BTServiceBase>? GenrealServices; 
    // to store the list of decorators of this node
    protected List<IBTDecorator>? Decorators;
    
// to know if a know has finished or not. (succeeded or failed)
public bool HasFinished => (LastStatus == EBTNodeResult.Succeeded || LastStatus ==EBTNodeResult.failed);
// to store if all the decorators allow for running this node

    protected bool bDecoratorsAllowRunning = false;

// to diffrentiate between flow nodes and action nodes
    public abstract bool HasChildren { get; }

    public virtual string DebugDisplayName { get; protected set; } = "Unnamed Node";

    protected bool bCanSendExitNotification = false;
/// <summary>
/// Adds the decorator nodes to a node
/// </summary>
/// <param name="InDecorator"></param>
/// <returns></returns>
    public IBTNode AddDecorator(IBTDecorator InDecorator)
    {
        if (Decorators == null)
            Decorators = new();
        InDecorator.SetOwiningTree(OwningTree);
        Decorators.Add(InDecorator);
        return this;
    }
    /// <summary>
    /// Adds services to each node
    /// </summary>
    /// <param name="InService"></param>
    /// <param name="InIsAlwaysOn"></param>
    /// <returns></returns>

    public IBTNode AddService(BTServiceBase InService, bool InIsAlwaysOn = false)
    {
        InService.SetOwiningTree(OwningTree);
        if (InIsAlwaysOn)
        {
            if (AlwaysOnServices == null)
                AlwaysOnServices = new();
            AlwaysOnServices.Add(InService);
        }
        else 
        {
            if (GenrealServices == null)
                GenrealServices = new();
                GenrealServices.Add(InService);
    ;
            }
        return this;

    }
/// <summary>
/// 
/// </summary>
/// <param name="InDeltaTime"></param>
/// <returns></returns>
    public bool DoDecoratorsNowPermitRunning(float InDeltaTime)
    {
        // if the decorators already allow running then no need to check
        if (bDecoratorsAllowRunning)
            return false;

        // update always on services on services
        if (!OnTick_AlwaysOnServices(InDeltaTime))
            return false;

        // check decorators 
        if (!OnTick_Decorators(InDeltaTime))
            return false;

        return true;

    }

    public virtual void Reset()
    {
        LastStatus = EBTNodeResult.readyToTick;
    }

    public void SetOwiningTree(IBehaviorTree InOwningtree)
    {
        this.OwningTree = InOwningtree;
    }
/// <summary>
/// main logic of the ticks. ticks decide which nodes are gonna be executed
/// </summary>
/// <param name="InDeltaTime"></param>
/// <returns></returns>
    public EBTNodeResult Tick(float InDeltaTime)
    {
        //first time running, reset the node which will chnage the node to --> ready to tick
        if (LastStatus == EBTNodeResult.Uninitialized)
            Reset();

        //then the ticks goes through the services. If any of the services fail, then node result will be failed  

        CurrentTickPhase = EBTNodeTickPhase.AlwaysOnServices;
        
        if (!OnTick_AlwaysOnServices(InDeltaTime))
        {
            //checks if the decorators can change the result and if yes, we will change the result and also the action upon exit will be executed
            LastStatus = EBTNodeResult.failed;
            Console.WriteLine("here1");
            return OnTickReturn(LastStatus);
        }

        // then the ticks goes through the decorators, if any of the decorators return false, then 
        
        CurrentTickPhase = EBTNodeTickPhase.Decorators;
        if (!OnTick_Decorators(InDeltaTime))
        {
            LastStatus = EBTNodeResult.failed;
            //node has previously run and now is not permitted to?
            if (bDecoratorsAllowRunning && bCanSendExitNotification)
                OnExit();
            bDecoratorsAllowRunning = false;
            Console.WriteLine(LastStatus.ToString());
            return OnTickReturn(LastStatus);
        }
        // if the decorators have changed to permit running then we reset the node
        if (!bDecoratorsAllowRunning)
        {
            Reset();
            bDecoratorsAllowRunning = true;
        }

        // have we already finished?
        if (HasFinished)
            return OnTickReturn(LastStatus);
        CurrentTickPhase = EBTNodeTickPhase.GeneralServices; 
        if(!OnTick_GenralServices(InDeltaTime))           
            return OnTickReturn(EBTNodeResult.failed);
        
        //node has never been ticked?
        if(LastStatus == EBTNodeResult.readyToTick )
        {
            OnEnter();
            if (HasFinished)
                return OnTickReturn(LastStatus);
        }
        //here we tick the node logic itself
        CurrentTickPhase = EBTNodeTickPhase.NodeLogic;
        if (!OnTick_NodeLogic(InDeltaTime))
            return OnTickReturn(EBTNodeResult.failed);
// if it has children, we tick them too 
            if(HasChildren)
            {
            CurrentTickPhase = EBTNodeTickPhase.Children;
            if (!OnTick_Children(InDeltaTime))
                return OnTickReturn(LastStatus);
            }
        

        return OnTickReturn(LastStatus);

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="InProvisionalResult"></param>
    /// <returns></returns>
    protected virtual EBTNodeResult OnTickReturn(EBTNodeResult InProvisionalResult)
    {
        EBTNodeResult FinalResult = InProvisionalResult;
        CurrentTickPhase = EBTNodeTickPhase.WaitingForNextTick;
        // if(Decorators != null)
        // {
        //     foreach(var Decorator in Decorators)
        //     {
        //         if (Decorator.CanPostProcessTickResult(FinalResult))
        //             FinalResult = Decorator.PostProcessTickResult(FinalResult);

        //     }
        // }
        if (bCanSendExitNotification && HasFinished)
            OnExit();

        return FinalResult;
    }
    /// <summary>
    /// goes through the services, and if any of the services's thick return's false, then the function returns false
    /// </summary>
    /// <param name="InDeltaTime"></param>
    /// <returns></returns>
    protected virtual bool OnTick_AlwaysOnServices(float InDeltaTime)
    {
        if(AlwaysOnServices != null)
        {
            foreach(var service in AlwaysOnServices)
            {
                if (!service.Tick(InDeltaTime))
                    return false;
            }
        }
        return true;
    }
    protected virtual bool OnTick_GenralServices(float InDeltaTime)
    {
        if(GenrealServices != null)
        {
            foreach(var service in GenrealServices)
            {
                if (!service.Tick(InDeltaTime))
                    return false;
            }
        }
        return true;
    }
    protected virtual bool OnTick_Decorators(float InDeltaTime)
    {
        if(Decorators != null)
        {
            foreach(var decorator in Decorators)
            {
                if (!decorator.Tick(InDeltaTime))
                    return false;
            }
        }
        return true;
    }
    // these ones are the ones that actually execute a node logic
    protected abstract bool OnTick_NodeLogic(float InDeltaTime);
    //this one is for the flow nodes 
    protected abstract bool OnTick_Children(float InDeltaTime);

    protected virtual void OnEnter()
    {
        bCanSendExitNotification = true;

    }
    protected virtual void OnExit()
    {
        bCanSendExitNotification = false;
    }
}

