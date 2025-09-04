using System.Reflection.PortableExecutable;
using BehaviorTreeMainProject.Services;
using BehaviorTreeMainProject.Log.Services;

public abstract class BTNodeBase : IBTNode
{
    //public  string DebugDisplayName { get; protected set; } = "Unnamed Node";
    //who is responsible for doing this action

    public Agent? self { get; protected set; }
    // which tree does this node belong to
    public IBehaviorTree? OwningTree { get; protected set; }

    // NEW: Reference to parent node for bidirectional access
    public IBTNode? ParentNode { get; set; }
    
    // NEW: Public method to set parent reference (for external use)
    public void SetParentNode(IBTNode parent)
    {
        ParentNode = parent;
        LoggingService.LogInfo($"🔧 BTNodeBase: {DebugDisplayName} - Parent reference set to: {parent?.DebugDisplayName ?? "null"}");
    }

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

    public abstract string DebugDisplayName { get; protected set; }

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
        
        // NEW: Set the parent reference for bidirectional access
        childNode.SetParentNode(this);
        LoggingService.LogInfo($"🔧 BTNodeBase: Set ParentNode for child {childNode.DebugDisplayName}");
        
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
            LinkedBlackboard.SetActionInstance(action.InstanceName, action);
            LoggingService.LogInfo($"🔧 BTNodeBase: Set tree for SubtreeInjectionService of {childNode.DebugDisplayName}");
            
        }
        else if (childNode is BTFlowNodeBase flowNode)
        {
            LinkedBlackboard.SetFlowNodeInstance(flowNode.InstanceName, flowNode);
            LoggingService.LogInfo($"🔧 BTNodeBase: Set tree for all services of {childNode.DebugDisplayName}");
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
        // Log node tick start
        ExecutionFlowLogger.LogNodeTick(DebugDisplayName, GetType().Name, "START", LastStatus.ToString());
        
        //first time running, reset the node which will chnage the node to --> ready to tick
        if (LastStatus == EBTNodeResult.Uninitialized)
        {
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} is Uninitialized, calling Reset()");
            Reset();
        }

        //then the ticks goes through the services. If any of the services fail, then node result will be failed  
        LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Starting AlwaysOnServices phase");
        CurrentTickPhase = EBTNodeTickPhase.AlwaysOnServices;
        
        if (!OnTick_AlwaysOnServices(InDeltaTime))
        {
            //checks if the decorators can change the result and if yes, we will change the result and also the action upon exit will be executed
            LastStatus = EBTNodeResult.failed;
            LoggingService.LogWarning($"❌ BTNodeBase: AlwaysOnServices failed for {DebugDisplayName}, setting status to failed");
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - EXITING at AlwaysOnServices failure");
            return OnTickReturn(LastStatus);
        }
        LoggingService.LogInfo($"✅ BTNodeBase: {DebugDisplayName} - AlwaysOnServices completed successfully");

        // then the ticks goes through the general services (like planning services)
        LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Starting GeneralServices phase");
        CurrentTickPhase = EBTNodeTickPhase.GeneralServices; 
        ExecutionFlowLogger.LogPhaseTransition(DebugDisplayName, "AlwaysOnServices", "GeneralServices");
        if(!OnTick_GeneralServices(InDeltaTime))           
        {
            LoggingService.LogWarning($"❌ BTNodeBase: GeneralServices failed for {DebugDisplayName}");
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - EXITING at GeneralServices failure");
            return OnTickReturn(EBTNodeResult.failed);
        }
        LoggingService.LogInfo($"✅ BTNodeBase: {DebugDisplayName} - GeneralServices completed successfully");

        // then the ticks goes through the decorators, if any of the decorators return false, then 
        LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Starting Decorators phase");
        CurrentTickPhase = EBTNodeTickPhase.Decorators;
        ExecutionFlowLogger.LogPhaseTransition(DebugDisplayName, "GeneralServices", "Decorators");
        // if decorators return false, then we return failed
        if (!OnTick_Decorators(InDeltaTime))
        {
            // Decorator blocked execution - return failed so node can be re-evaluated on next tick
            LastStatus = EBTNodeResult.failed;
            //node has previously run and now is not permitted to?
            if (bDecoratorsAllowRunning && bCanSendExitNotification)
            {OnExit();}                
            bDecoratorsAllowRunning = false;
            LoggingService.LogInfo($"⏳ BTNodeBase: Decorators blocked execution for {DebugDisplayName}, returning failed for re-evaluation on next tick");
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - EXITING at Decorators blocking");
            return OnTickReturn(LastStatus);
        }
        LoggingService.LogInfo($"✅ BTNodeBase: {DebugDisplayName} - Decorators evaluation completed successfully");
        
        // Only reset if decorators were previously blocking but now allow execution
        if (!bDecoratorsAllowRunning)
        {
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Decorators changed from blocking to allowing, calling Reset()");
            Reset();
            bDecoratorsAllowRunning = true;
        }
        else
        {
            // Decorators were already allowing execution, ensure flag is set to true
            bDecoratorsAllowRunning = true;
        }
        

        // have we already finished? if yes, then we return the result
        LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Checking HasFinished: {HasFinished} (LastStatus: {LastStatus})");
        if (HasFinished)
        {
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Node has finished, returning {LastStatus} without further processing");
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - EXITING at HasFinished check");
            return OnTickReturn(LastStatus);
        }
        
        //node has never been ticked? if yes, then we enter the node
        LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Checking if node needs OnEnter (LastStatus: {LastStatus})");
        if(LastStatus == EBTNodeResult.readyToTick )
        {
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Calling OnEnter()");
            OnEnter();
            if (HasFinished)
            {
                LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - OnEnter caused node to finish, returning {LastStatus}");
                LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - EXITING at OnEnter completion");
                return OnTickReturn(LastStatus);
            }
        }
        
        //here we tick the node logic itself
        LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Starting NodeLogic phase");
        CurrentTickPhase = EBTNodeTickPhase.NodeLogic;
        ExecutionFlowLogger.LogPhaseTransition(DebugDisplayName, "Decorators", "NodeLogic");
        if (!OnTick_NodeLogic(InDeltaTime))
        {
            LoggingService.LogWarning($"❌ BTNodeBase: NodeLogic failed for {DebugDisplayName}");
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - EXITING at NodeLogic failure");
            return OnTickReturn(EBTNodeResult.failed);
        }
        LoggingService.LogInfo($"✅ BTNodeBase: {DebugDisplayName} - NodeLogic completed successfully");

        // if it has children, we tick them too 
        if(HasChildren)
        {
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Starting Children phase (HasChildren: {HasChildren})");
            CurrentTickPhase = EBTNodeTickPhase.Children;
            ExecutionFlowLogger.LogPhaseTransition(DebugDisplayName, "NodeLogic", "Children");
            if (!OnTick_Children(InDeltaTime))
            {
                LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - OnTick_Children returned false, returning {LastStatus}");
                LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - EXITING at OnTick_Children");
                return OnTickReturn(LastStatus);
            }
            LoggingService.LogInfo($"✅ BTNodeBase: {DebugDisplayName} - OnTick_Children completed successfully");
        }
        else
        {
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - No children to tick (HasChildren: {HasChildren})");
        }

        LoggingService.LogInfo($"✅ BTNodeBase: {DebugDisplayName} - Tick method completed successfully, returning {LastStatus}");
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
        if(Decorators != null)
        {
            foreach(var Decorator in Decorators)
            {
                if (Decorator.CanPostProcessTickResult)
                    FinalResult = Decorator.PostProcessTickResult(FinalResult);

            }
        }
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
                
                // Log service tick start
                ExecutionFlowLogger.LogServiceTick(service.GetType().Name, "GeneralService", DebugDisplayName, "START");
                
                bool serviceResult = service.Tick(InDeltaTime);
                
                // Log service tick result
                ExecutionFlowLogger.LogServiceTick(service.GetType().Name, "GeneralService", DebugDisplayName, serviceResult ? "SUCCESS" : "FAILED");
                
                if (!serviceResult)
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
        LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - OnTick_Decorators called, decorator count: {Decorators?.Count ?? 0}");
        
        if(Decorators != null)
        {
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Starting decorator evaluation for {Decorators.Count} decorators");
            
            foreach(var decorator in Decorators)
            {
                LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Evaluating decorator: {decorator.GetType().Name}");
                
                // Log decorator tick start
                ExecutionFlowLogger.LogDecoratorTick(decorator.GetType().Name, "Decorator", DebugDisplayName, "START");
                
                bool decoratorResult = decorator.Tick(InDeltaTime);
                
                // Log decorator tick result
                ExecutionFlowLogger.LogDecoratorTick(decorator.GetType().Name, "Decorator", DebugDisplayName, decoratorResult ? "ALLOW" : "BLOCK");
                
                LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Decorator {decorator.GetType().Name} result: {(decoratorResult ? "ALLOW" : "BLOCK")}");
                
                if (!decoratorResult)
                {
                    LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - Decorator {decorator.GetType().Name} BLOCKED execution, returning false");
                    return false;
                }
            }
            
            LoggingService.LogInfo($"✅ BTNodeBase: {DebugDisplayName} - All {Decorators.Count} decorators evaluated successfully, returning true");
        }
        else
        {
            LoggingService.LogInfo($"🔄 BTNodeBase: {DebugDisplayName} - No decorators to evaluate, returning true");
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

