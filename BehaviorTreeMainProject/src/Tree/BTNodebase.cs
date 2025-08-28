using System.Reflection.PortableExecutable;
using BehaviorTreeMainProject.Services;

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

    protected bool bDecoratorsAllowRunning = true;

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
        // Only set the tree if it's already available
        if (OwningTree != null)
        {
            InService.SetOwiningTree(OwningTree);
        }
        
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
    /// Set the tree for all services that don't have it set yet
    /// This should be called after SetOwiningTree is called on the node
    /// </summary>
    public void SetTreeForAllServices(IBehaviorTree InOwningtree)
    {
        if (AlwaysOnServices != null)
        {
            foreach (var service in AlwaysOnServices)
            {
                if (service.OwningTree == null)
                {
                    service.SetOwiningTree(InOwningtree);
                }
            }
        }
        
        if (GenrealServices != null)
        {
            foreach (var service in GenrealServices)
            {
                if (service.OwningTree == null)
                {
                    service.SetOwiningTree(InOwningtree);
                }
            }
        }
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
    /// Add a child node and set up proper parent-child relationship
    /// This method should be overridden by derived classes that can have children
    /// </summary>
    public virtual IBTNode AddChild(IBTNode childNode)
    {
        LoggingService.LogInfo($"🔧 BTNodeBase: AddChild called for {DebugDisplayName} - adding child: {childNode.DebugDisplayName}");
        
        // Set the owning tree for the child
        childNode.SetOwiningTree(OwningTree);
        LoggingService.LogInfo($"🔧 BTNodeBase: Set OwningTree for child {childNode.DebugDisplayName}");
        
        // Set the tree for all services that don't have it set yet
        childNode.SetTreeForAllServices(OwningTree);
        LoggingService.LogInfo($"🔧 BTNodeBase: Set tree for all services of child {childNode.DebugDisplayName}");
        
        // If this is a GenericBTAction, also set the tree for its SubtreeInjectionService
        if (childNode is GenericBTAction action)
        {
            action.SetTreeForSubtreeInjectionService(OwningTree);
            LoggingService.LogInfo($"🔧 BTNodeBase: Set tree for SubtreeInjectionService of {childNode.DebugDisplayName}");
        }
        
        LoggingService.LogInfo($"🔧 BTNodeBase: AddChild completed for {childNode.DebugDisplayName}");
        return childNode;
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
            LoggingService.LogWarning($"❌ BTNodeBase: AlwaysOnServices failed for {DebugDisplayName}, setting status to failed");
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
            LoggingService.LogWarning($"❌ BTNodeBase: Decorators failed for {DebugDisplayName}, setting status to {LastStatus}");
            return OnTickReturn(LastStatus);
        }
        // if the decorators have changed to permit running then we reset the node
        // if (!bDecoratorsAllowRunning)
        // {
        //     Reset();
        //     bDecoratorsAllowRunning = true;
        // }

        // have we already finished? if yes, then we return the result
        if (HasFinished)
            return OnTickReturn(LastStatus);
        CurrentTickPhase = EBTNodeTickPhase.GeneralServices; 
        if(!OnTick_GeneralServices(InDeltaTime))           
            return OnTickReturn(EBTNodeResult.failed);
        
        //node has never been ticked? if yes, then we enter the node
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
    protected virtual bool OnTick_GeneralServices(float InDeltaTime)
    {
        LoggingService.LogInfo($"🚨 DEBUG: BTNodeBase.OnTick_GeneralServices called for {DebugDisplayName}");
        LoggingService.LogInfo($"🔍 BTNodeBase: GeneralServices count: {GenrealServices?.Count ?? 0}");
        
        if(GenrealServices != null && GenrealServices.Count > 0)
        {
            LoggingService.LogInfo($"🔍 BTNodeBase: Executing {GenrealServices.Count} general services");
            foreach(var service in GenrealServices)
            {
                LoggingService.LogInfo($"   🔄 BTNodeBase: Calling service.Tick() for {service.GetType().Name}");
                if (!service.Tick(InDeltaTime))
                {
                    LoggingService.LogWarning($"   ❌ BTNodeBase: Service {service.GetType().Name} returned false");
                    return false;
                }
                LoggingService.LogInfo($"   ✅ BTNodeBase: Service {service.GetType().Name} returned true");
            }
            LoggingService.LogInfo($"   ✅ BTNodeBase: All general services completed successfully");
        }
        else
        {
            LoggingService.LogInfo($"🔍 BTNodeBase: No general services to execute");
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

