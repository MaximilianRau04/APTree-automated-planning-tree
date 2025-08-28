using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlanningDataStructures;
using AIPlanning;
using BehaviorTreeMainProject.Services.AIPlanning;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
  
    public class SubtreeInjectionService : BTServiceBase
    {
        private readonly Dictionary<string, SubtreeConfiguration> subtreeConfigurations;
        private readonly Dictionary<string, BTFlowNode_Dynamic> cachedSubtrees;
        
        // Default planner mapping for HL actions
        private readonly Dictionary<string, string> defaultPlannerMapping;
        
        // Action to be processed in the next tick
        private GenericBTAction pendingAction;
        
        // Parameter instances from PDDL file
        private readonly Dictionary<string, string> parameterInstances;
        
        // Track generated problem files for debugging
        private readonly List<string> generatedProblemFiles;

        // Logging system
        private static readonly string LogFilePath = "SubtreeInjectionService_Debug.log";
        private static readonly object LogLock = new object();

        public SubtreeInjectionService(IBehaviorTree owningTree, GenericBTAction action) : base(owningTree)
        {
            subtreeConfigurations = new Dictionary<string, SubtreeConfiguration>();
            cachedSubtrees = new Dictionary<string, BTFlowNode_Dynamic>();
            defaultPlannerMapping = new Dictionary<string, string>();
            pendingAction = action;
            parameterInstances = new Dictionary<string, string>();
            generatedProblemFiles = new List<string>();
            
            InitializeDefaultConfigurations();
            InitializeDefaultPlannerMapping();
            LoadParameterInstances();
        }

        /// <summary>
        /// Alternative constructor that allows setting the tree later
        /// </summary>
        public SubtreeInjectionService(GenericBTAction action) : base(null)
        {
            subtreeConfigurations = new Dictionary<string, SubtreeConfiguration>();
            cachedSubtrees = new Dictionary<string, BTFlowNode_Dynamic>();
            defaultPlannerMapping = new Dictionary<string, string>();
            pendingAction = action;
            parameterInstances = new Dictionary<string, string>();
            generatedProblemFiles = new List<string>();
            
            InitializeDefaultConfigurations();
            InitializeDefaultPlannerMapping();
            LoadParameterInstances();
        }

        /// <summary>
        /// Log message to both console and file
        /// </summary>
        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] {message}";
            
            // Write to console
            Console.WriteLine(logMessage);
            
            // Write to file
            lock (LogLock)
            {
                try
                {
                    File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{timestamp}] ❌ Failed to write to log file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Clear the log file
        /// </summary>
        public static void ClearLogFile()
        {
            lock (LogLock)
            {
                try
                {
                    File.WriteAllText(LogFilePath, $"=== SubtreeInjectionService Debug Log - Started at {DateTime.Now} ==={Environment.NewLine}");
                    Console.WriteLine($"✅ Log file cleared: {LogFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to clear log file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Load parameter instances from PDDL file
        /// </summary>
        private void LoadParameterInstances()
        {
            try
            {
                string filePath = "src/InputInstances/ParameterInstances_PDDL.txt";
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(" - ");
                        if (parts.Length == 2)
                        {
                            parameterInstances[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                    LogMessage($"✅ SubtreeInjectionService: Loaded {parameterInstances.Count} parameter instances");
                }
                else
                {
                    LogMessage($"⚠️ SubtreeInjectionService: Parameter instances file not found at {filePath}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ SubtreeInjectionService: Error loading parameter instances: {ex.Message}");
            }
        }

        

        /// <summary>
        /// Service tick method - implements the required logic:
        /// 1. Check if action is HL by checking the name
        /// 2. If not HL, return true
        /// 3. If HL, inject the subtree
        /// 4. Return true if injection successful, false otherwise
        /// </summary>
        public override bool Tick(float InDeltaTime)
        {
            LogMessage($"🔍 SubtreeInjectionService: Tick called for service attached to tree: {OwningTree?.GetType().Name}");
            
            // First, check if we have a pending action to process
            if (pendingAction != null)
            {
                var actionType = pendingAction.actionType.ToString();
                LogMessage($"🔍 SubtreeInjectionService: Processing queued action: {actionType}");
                LogMessage($"🔍 SubtreeInjectionService: Action type ends with 'HL': {actionType.EndsWith("HL")}");
                
                // 1. Check if the action is HL by checking the name of the action
                if (!actionType.EndsWith("HL"))
                {
                    LogMessage($"🔍 SubtreeInjectionService: Action {actionType} is not a high-level action (no 'HL' suffix)");
                    // 2. If it is not HL return true
                    return true;
                }
                
                // 3. If it is HL, then we Inject the subtree
                LogMessage($"🔍 SubtreeInjectionService: Detected high-level action: {actionType}");
                try
                {
                    ProcessSubtreeInjection( null); // customParameters would be passed here if needed
                    LogMessage($"✅ SubtreeInjectionService: Successfully injected subtree for {actionType}");
                    // 4. If the injection was successful return true
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"❌ SubtreeInjectionService: Failed to inject subtree for {actionType}: {ex.Message}");
                    // 4. else, return false
                    return false; 
                }
            }
            else
            {
                LogMessage($"🔍 SubtreeInjectionService: No pending action to process (pendingAction is null)");
            }
            
            return true; // No action to process
            
               
            }
            
           

        

        /// <summary>
        /// Process subtree injection for a specific action
        /// </summary>
        private void ProcessSubtreeInjection( Dictionary<string, object> customParameters = null)
        {
            try
            {
                var actionType = pendingAction.actionType.ToString();
                LogMessage($"🔧 SubtreeInjectionService: Processing injection for {actionType}");
                
                // Get the default planner for this action type
                string configName = GetDefaultPlannerForAction(actionType);
                
                // Create instance name from action
                string instanceName = pendingAction.InstanceName.ToString();
                
                // Generate dynamic PDDL problem file
                string problemFileName = GenerateDynamicPDDLProblem(instanceName);
                
                // Merge custom parameters with the generated problem file
                var mergedParameters = customParameters ?? new Dictionary<string, object>();
                mergedParameters["problemFile"] = problemFileName;
                
                LogMessage($"🔧 SubtreeInjectionService: Using dynamic problem file: {problemFileName}");
                LogMessage($"🔧 SubtreeInjectionService: Merged parameters count: {mergedParameters.Count}");
                foreach (var param in mergedParameters)
                {
                    LogMessage($"   Parameter: {param.Key} = {param.Value}");
                }
                
                // Inject the subtree
                InjectSubtreeIntoAction(pendingAction, configName, instanceName, mergedParameters);
                
                LogMessage($"✅ SubtreeInjectionService: Successfully processed injection for {actionType}");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ SubtreeInjectionService: Error processing injection: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate a dynamic PDDL problem file for the given action
        /// </summary>
        public string GenerateDynamicPDDLProblem( string instanceName)
        {
            try
            {
                LogMessage($"🔧 SubtreeInjectionService: Starting GenerateDynamicPDDLProblem for instance: {instanceName}");
                
                // Check if pendingAction is null
                if (pendingAction == null)
                {
                    LogMessage($"❌ SubtreeInjectionService: pendingAction is null!");
                    throw new InvalidOperationException("pendingAction is null");
                }
                // setting the address
                var actionType = pendingAction.actionType.ToString();
                var actionFullName = pendingAction.GetType().Name; // Get the full class name
                string problemFileName = $"problem{instanceName}.pddl";
                string problemFilePath = $"python_service/Plannerinputs/{problemFileName}";
                string relativeProblemPath = $"Plannerinputs/{problemFileName}";
                
                LogMessage($"🔧 SubtreeInjectionService: Generating PDDL problem file: {problemFileName}");
                LogMessage($"🔧 SubtreeInjectionService: Action type: {actionType}, Action full name: {actionFullName}");
                
                // Check if LinkedBlackboard is null
                if (LinkedBlackboard == null)
                {
                    LogMessage($"❌ SubtreeInjectionService: LinkedBlackboard is null!");
                    throw new InvalidOperationException("LinkedBlackboard is null");
                }
                
                // 1. Retrieve predicates from blackboard
                LogMessage($"🔧 SubtreeInjectionService: About to call LinkedBlackboard.GetAllPredicates()");
                var initialstatepredicates = LinkedBlackboard.GetTruePredicates();
                LogMessage($"🔧 SubtreeInjectionService: Retrieved {initialstatepredicates?.Count ?? 0} initial state predicates");
                
                if (initialstatepredicates == null)
                {
                    LogMessage($"❌ SubtreeInjectionService: initialstatepredicates is null!");
                    throw new InvalidOperationException("initialstatepredicates is null");
                }
                
                LogMessage($"🔧 SubtreeInjectionService: About to call Parser.ConvertMultiplePredicatesToPDDL()");
                string initialstatepredicatesPDDL = Parser.ConvertMultiplePredicatesToPDDL(initialstatepredicates);
                LogMessage($"📋 SubtreeInjectionService: Initial state PDDL: {initialstatepredicatesPDDL}");
                
                // 2. Get action effects for goals
                LogMessage($"🔧 SubtreeInjectionService: About to call pendingAction.GetActionEffects()");
                var goalstatePredicates = pendingAction.GetActionEffects();
                LogMessage($"🔧 SubtreeInjectionService: Retrieved {goalstatePredicates?.Count ?? 0} goal predicates from action effects");
                
                if (goalstatePredicates == null)
                {
                    LogMessage($"❌ SubtreeInjectionService: goalstatePredicates is null!");
                    throw new InvalidOperationException("goalstatePredicates is null");
                }
                
                foreach (var predicate in goalstatePredicates)
                {
                    LogMessage($"   Goal predicate: {predicate?.PredicateName}");
                }
                
                LogMessage($"🔧 SubtreeInjectionService: About to call Parser.ConvertMultiplePredicatesToPDDL() for goals");
                string goalstatepredicatesPDDL = Parser.ConvertMultiplePredicatesToPDDL(goalstatePredicates);
                LogMessage($"🎯 SubtreeInjectionService: Goal state PDDL: {goalstatepredicatesPDDL}");
                
                // 3. Generate PDDL problem content
                LogMessage($"🔧 SubtreeInjectionService: About to call GeneratePDDLProblemContent()");
                string pddlContent = GeneratePDDLProblemContent(actionFullName, initialstatepredicatesPDDL, goalstatepredicatesPDDL);
                LogMessage($"🔧 SubtreeInjectionService: Generated PDDL content length: {pddlContent?.Length ?? 0}");
                
                // 4. Write to file
                LogMessage($"🔧 SubtreeInjectionService: About to write file to: {problemFilePath}");
                File.WriteAllText(problemFilePath, pddlContent);
                LogMessage($"🔧 SubtreeInjectionService: File written successfully");
                
                // 5. Verify file was created and contains content
                if (File.Exists(problemFilePath))
                {
                    var fileContent = File.ReadAllText(problemFilePath);
                    LogMessage($"✅ SubtreeInjectionService: Generated PDDL problem file: {problemFilePath}");
                    LogMessage($"📄 SubtreeInjectionService: File size: {fileContent.Length} characters");
                    LogMessage($"📄 SubtreeInjectionService: Problem file content preview:");
                    LogMessage(pddlContent);
                    
                    // Verify that goals are present
                    if (fileContent.Contains("(:goal"))
                    {
                        LogMessage($"✅ SubtreeInjectionService: Problem file contains goal section");
                    }
                    else
                    {
                        LogMessage($"⚠️ SubtreeInjectionService: Problem file does NOT contain goal section!");
                    }
                }
                else
                {
                    LogMessage($"❌ SubtreeInjectionService: Failed to create problem file: {problemFilePath}");
                }
                
                // Track the generated problem file
                generatedProblemFiles.Add(problemFilePath);
                
                LogMessage($"✅ SubtreeInjectionService: Successfully completed GenerateDynamicPDDLProblem");
                return relativeProblemPath;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ SubtreeInjectionService: Error generating PDDL problem: {ex.Message}");
                LogMessage($"❌ SubtreeInjectionService: Exception type: {ex.GetType().Name}");
                LogMessage($"❌ SubtreeInjectionService: Stack trace: {ex.StackTrace}");
                // Fallback to default problem file
                return "Plannerinputs/bigproblem.pddl";
            }
        }

   

        /// <summary>
        /// Generate PDDL problem content
        /// </summary>
        private string GeneratePDDLProblemContent(string actionType, string initialPredicates, string goalPredicates)
        {
            // Get relevant objects based on action type
            var objects = GetRelevantObjects(actionType);
            
            
            return $@"(define (problem {actionType.ToLower()})
  (:domain fit)
  (:objects 
    {objects}
  )
  (:init  
    {initialPredicates}
  )
  (:goal 
    (and
      {goalPredicates}
    ) 
  )
)";
        }

        /// <summary>
        /// Get relevant objects from ParameterInstances_PDDL.txt file
        /// </summary>
        /// <param name="actionType">Action type (ignored - returns all objects)</param>
        /// <returns>String containing all parameter instances from the PDDL file</returns>
        private string GetRelevantObjects(string actionType)
        {
            try
            {
                string filePath = "src/InputInstances/ParameterInstances_PDDL.txt";
                
                if (!File.Exists(filePath))
                {
                    LogMessage($"❌ SubtreeInjectionService: ParameterInstances_PDDL.txt file not found at {filePath}");
                    return string.Empty;
                }
                
                string content = File.ReadAllText(filePath);
                LogMessage($"✅ SubtreeInjectionService: Successfully read {content.Length} characters from ParameterInstances_PDDL.txt");
                
                return content;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ SubtreeInjectionService: Error reading ParameterInstances_PDDL.txt: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Initialize default planner mapping for HL actions
        /// </summary>
        private void InitializeDefaultPlannerMapping()
        {
            
            // Generic mapping for any HL action not specifically mapped
            defaultPlannerMapping["*HL"] = "FF_Default";
            
            LogMessage("✅ SubtreeInjectionService: Initialized default planner mapping");
        }

        /// <summary>
        /// Get the default planner configuration for a given action type
        /// </summary>
        private string GetDefaultPlannerForAction(string actionType)
        {
            // First try exact match
            if (defaultPlannerMapping.TryGetValue(actionType, out string configName))
            {
                return configName;
            }
            else
            // Fallback to FF_Default
            LogMessage($"⚠️ SubtreeInjectionService: No mapping found for {actionType}, using FF_Default");
            return "FF_Default";
        }

        /// <summary>
        /// Set custom planner mapping for an action type
        /// </summary>
        public void SetPlannerMapping(string actionType, string configName)
        {
            defaultPlannerMapping[actionType] = configName;
            LogMessage($"✅ SubtreeInjectionService: Set planner mapping {actionType} -> {configName}");
        }

        /// <summary>
        /// Get current planner mappings
        /// </summary>
        public Dictionary<string, string> GetPlannerMappings()
        {
            return new Dictionary<string, string>(defaultPlannerMapping);
        }

        /// <summary>
        /// Configuration for subtree creation
        /// </summary>
        public class SubtreeConfiguration
        {
            public string Name { get; set; }
            public PlannerType PlannerType { get; set; }
            public SuccessCriteria SuccessCriteria { get; set; }
            public Dictionary<string, object> PlannerParameters { get; set; }
            public bool UseCaching { get; set; } = true;

            public SubtreeConfiguration(string name, PlannerType plannerType, SuccessCriteria successCriteria = SuccessCriteria.ALL)
            {
                Name = name;
                PlannerType = plannerType;
                SuccessCriteria = successCriteria;
                PlannerParameters = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Supported planner types
        /// </summary>
        public enum PlannerType
        {
            FF,
            ENHSP,
            GOAP,
            StateChart
        }

        /// <summary>
        /// Initialize default subtree configurations
        /// </summary>
        private void InitializeDefaultConfigurations()
        {
            // FF Planner Configuration
            var ffConfig = new SubtreeConfiguration("FF_Default", PlannerType.FF, SuccessCriteria.ALL);
                            ffConfig.PlannerParameters["domainFile"] = "Plannerinputs/DomainML.pddl";
                          ffConfig.PlannerParameters["problemFile"] = "Plannerinputs/bigproblem.pddl";
            ffConfig.PlannerParameters["plannerPath"] = "ff";  // FF planner command for Docker
            ffConfig.PlannerParameters["timeoutSeconds"] = 30;
            ffConfig.PlannerParameters["maxPlanLength"] = 10;
            ffConfig.PlannerParameters["executionMode"] = CallPDDLPlanner.ParallelExecutionMode.Sequential;
            subtreeConfigurations["FF_Default"] = ffConfig;

            // ENHSP Planner Configuration
            var enhspConfig = new SubtreeConfiguration("ENHSP_Default", PlannerType.ENHSP, SuccessCriteria.ALL);
                            enhspConfig.PlannerParameters["domainFile"] = "Plannerinputs/domain.pddl";
                enhspConfig.PlannerParameters["problemFile"] = "Plannerinputs/problemC1.pddl";
            enhspConfig.PlannerParameters["plannerPath"] = "/home/shermin/ENHSP-Public/enhsp.jar";
            enhspConfig.PlannerParameters["timeoutSeconds"] = 30;
            enhspConfig.PlannerParameters["maxPlanLength"] = 10;
            enhspConfig.PlannerParameters["executionMode"] = CallPDDLPlanner.ParallelExecutionMode.Sequential;
            subtreeConfigurations["ENHSP_Default"] = enhspConfig;

            // GOAP Planner Configuration
            var goapConfig = new SubtreeConfiguration("GOAP_Default", PlannerType.GOAP, SuccessCriteria.ALL);
            goapConfig.PlannerParameters["timeoutSeconds"] = 30;
            goapConfig.PlannerParameters["maxPlanLength"] = 10;
            goapConfig.PlannerParameters["domain"] = "Construction";
            goapConfig.PlannerParameters["enableDebugLogging"] = true;
            goapConfig.PlannerParameters["heuristicWeight"] = 1.0f;
            goapConfig.PlannerParameters["maxSearchDepth"] = 50;
            subtreeConfigurations["GOAP_Default"] = goapConfig;

            // StateChart Planner Configuration
            var stateChartConfig = new SubtreeConfiguration("StateChart_Default", PlannerType.StateChart, SuccessCriteria.ALL);
            stateChartConfig.PlannerParameters["timeoutSeconds"] = 30;
            stateChartConfig.PlannerParameters["maxPlanLength"] = 10;
            stateChartConfig.PlannerParameters["currentState"] = "initial";
            stateChartConfig.PlannerParameters["targetState"] = "final";
            stateChartConfig.PlannerParameters["availableTransitions"] = new List<string> { "start", "process", "complete" };
            subtreeConfigurations["StateChart_Default"] = stateChartConfig;

            LogMessage("✅ SubtreeInjectionService: Initialized default configurations");
        }

        /// <summary>
        /// Register a custom subtree configuration
        /// </summary>
        public void RegisterConfiguration(string configName, SubtreeConfiguration configuration)
        {
            subtreeConfigurations[configName] = configuration;
            LogMessage($"✅ SubtreeInjectionService: Registered configuration '{configName}'");
        }

        /// <summary>
        /// Get a registered configuration
        /// </summary>
        public SubtreeConfiguration GetConfiguration(string configName)
        {
            if (subtreeConfigurations.TryGetValue(configName, out var config))
            {
                return config;
            }
            throw new ArgumentException($"Configuration '{configName}' not found");
        }

        /// <summary>
        /// Create a subtree using a registered configuration
        /// </summary>
        public BTFlowNode_Dynamic CreateSubtree(string configName, string instanceName, Dictionary<string, object> customParameters = null)
        {
            var config = GetConfiguration(configName);
            return CreateSubtree(config, instanceName, customParameters);
        }

        /// <summary>
        /// Create a subtree using a configuration
        /// </summary>
        public BTFlowNode_Dynamic CreateSubtree(SubtreeConfiguration config, string instanceName, Dictionary<string, object> customParameters = null)
        {
            try
            {
                LogMessage($"🔧 SubtreeInjectionService: Creating subtree '{config.Name}' for instance '{instanceName}'");

                // Check cache first
                string cacheKey = $"{config.Name}_{instanceName}";
                if (config.UseCaching && cachedSubtrees.TryGetValue(cacheKey, out var cachedSubtree))
                {
                    LogMessage($"✅ SubtreeInjectionService: Using cached subtree for '{cacheKey}'");
                    return cachedSubtree;
                }

                // Create subtree based on planner type
                BTFlowNode_Dynamic subtree = config.PlannerType switch
                {
                    PlannerType.FF => CreateFFSubtree(config, instanceName, customParameters),
                    PlannerType.ENHSP => CreateENHSPSubtree(config, instanceName, customParameters),
                    PlannerType.GOAP => CreateGOAPSubtree(config, instanceName, customParameters),
                    PlannerType.StateChart => CreateStateChartSubtree(config, instanceName, customParameters),
                    _ => throw new ArgumentException($"Unsupported planner type: {config.PlannerType}")
                };

                // Cache the subtree if caching is enabled
                if (config.UseCaching)
                {
                    cachedSubtrees[cacheKey] = subtree;
                    LogMessage($"💾 SubtreeInjectionService: Cached subtree for '{cacheKey}'");
                }

                LogMessage($"✅ SubtreeInjectionService: Created subtree successfully");
                return subtree;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ SubtreeInjectionService: Error creating subtree: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inject a subtree into an action
        /// </summary>
        public void InjectSubtreeIntoAction(GenericBTAction action, string configName, string instanceName, Dictionary<string, object> customParameters = null)
        {
            var subtree = CreateSubtree(configName, instanceName, customParameters);
            action.SetAsHighLevelAction(subtree, subtree.PlanningService);
            LogMessage($"✅ SubtreeInjectionService: Injected subtree '{configName}' into action '{action.InstanceName.ToString()}'");
        }

        

        /// <summary>
        /// Remove subtree from an action
        /// </summary>
        public void RemoveSubtreeFromAction(GenericBTAction action)
        {
            action.RemoveSubtree();
            LogMessage($"✅ SubtreeInjectionService: Removed subtree from action '{action.InstanceName.ToString()}'");
        }

        /// <summary>
        /// Clear the subtree cache
        /// </summary>
        public void ClearCache()
        {
            cachedSubtrees.Clear();
            LogMessage("🧹 SubtreeInjectionService: Cleared subtree cache");
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public (int cachedSubtrees, int configurations, int plannerMappings) GetStatistics()
        {
            return (cachedSubtrees.Count, subtreeConfigurations.Count, defaultPlannerMapping.Count);
        }
        
        /// <summary>
        /// Get list of generated problem files for debugging
        /// </summary>
        public List<string> GetGeneratedProblemFiles()
        {
            return new List<string>(generatedProblemFiles);
        }

        /// <summary>
        /// Find the action this service is attached to
        /// </summary>
        private GenericBTAction FindAttachedAction()
        {
            // The service is attached to a specific action, so we need to find that action
            // We can do this by searching through the tree and finding the action that has this service
            if (OwningTree?.RootNode == null)
            {
                LogMessage($"🔍 SubtreeInjectionService: No root node found");
                return null;
            }
            
            LogMessage($"🔍 SubtreeInjectionService: Searching for attached action in tree with root: {OwningTree.RootNode.GetType().Name}");
            var foundAction = FindActionWithService(OwningTree.RootNode);
            
            if (foundAction != null)
            {
                LogMessage($"🔍 SubtreeInjectionService: Found attached action: {foundAction.actionType}");
            }
            else
            {
                LogMessage($"🔍 SubtreeInjectionService: No attached action found");
            }
            
            return foundAction;
        }
        
        /// <summary>
        /// Recursively find the action that has this service attached to it
        /// </summary>
        private GenericBTAction FindActionWithService(IBTNode node)
        {
            // Check if this node is a GenericBTAction and has this service
            if (node is GenericBTAction action)
            {
                // Check if this action has this service in its services list
                if (HasServiceAttached(action))
                {
                    return action;
                }
            }
            
            // Check if this node has children (composite nodes, flow nodes, etc.)
            var children = GetNodeChildren(node);
            foreach (var child in children)
            {
                var foundAction = FindActionWithService(child);
                if (foundAction != null)
                {
                    return foundAction;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if an action has this service attached to it
        /// </summary>
        private bool HasServiceAttached(GenericBTAction action)
        {
            // We need to check if this action has this service in its services list
            // Since we can't directly access the services list, we'll use a different approach
            // We can check if the action has a SubtreeInjectionService by trying to get it
            try
            {
                var subtreeService = action.GetSubtreeInjectionService();
                var isAttached = subtreeService == this;
                LogMessage($"🔍 SubtreeInjectionService: Checking action {action.actionType} - Service match: {isAttached}");
                return isAttached;
            }
            catch (Exception ex)
            {
                // If we can't get the service, assume it's not this one
                LogMessage($"🔍 SubtreeInjectionService: Error checking service for action {action.actionType}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get children of a node using available methods
        /// </summary>
        private List<IBTNode> GetNodeChildren(IBTNode node)
        {
            var children = new List<IBTNode>();
            
            // Try different approaches to get children
            if (node is BTFlowNode_Composite compositeNode)
            {
                // Use the GetChildren method if available
                var compositeChildren = compositeNode.GetChildren();
                children.AddRange(compositeChildren);
            }
            else if (node is BTFlowNode_Dynamic dynamicNode)
            {
                // For dynamic nodes, check if they have an action graph
                var actionGraph = dynamicNode.GetActionGraph();
                if (actionGraph != null)
                {
                    var actionNodes = actionGraph.GetAllActionNodes();
                    children.AddRange(actionNodes);
                }
            }
            
            return children;
        }

        /// <summary>
        /// Create FF subtree
        /// </summary>
        private BTFlowNode_Dynamic CreateFFSubtree(SubtreeConfiguration config, string instanceName, Dictionary<string, object> customParameters)
        {
            var subtreeTree = new BTInstance();
            subtreeTree.Initialise(LinkedBlackboard, $"{config.Name}_Subtree_{instanceName}");

            var dynamicFlowNode = new BTFlowNode_Dynamic(
                new FastName($"{config.Name}_DynamicFlow_{instanceName}"),
                subtreeTree,
                config.SuccessCriteria
            );

            // Merge default and custom parameters
            var parameters = MergeParameters(config.PlannerParameters, customParameters);
            
            LogMessage($"🔧 SubtreeInjectionService: Creating FF subtree with parameters:");
            LogMessage($"   Domain File: {parameters["domainFile"]}");
            LogMessage($"   Problem File: {parameters["problemFile"]}");
            LogMessage($"   Planner Path: {parameters["plannerPath"]}");
            LogMessage($"   Timeout: {parameters["timeoutSeconds"]} seconds");
            LogMessage($"   Max Plan Length: {parameters["maxPlanLength"]}");

            var pddlRequest = new PDDLPlanningRequest(
                parameters["domainFile"].ToString(),
                parameters["problemFile"].ToString(),
                parameters["plannerPath"].ToString(),
                "FF",
                Convert.ToInt32(parameters["timeoutSeconds"]),
                Convert.ToInt32(parameters["maxPlanLength"])
            );

            var ffPlanner = new CallPDDLPlanner(subtreeTree, pddlRequest);
            ffPlanner.ExecutionMode = (CallPDDLPlanner.ParallelExecutionMode)parameters["executionMode"];

            dynamicFlowNode.SetPlanningService(ffPlanner);
            subtreeTree.RootNode = dynamicFlowNode;

            return dynamicFlowNode;
        }

        /// <summary>
        /// Create ENHSP subtree
        /// </summary>
        private BTFlowNode_Dynamic CreateENHSPSubtree(SubtreeConfiguration config, string instanceName, Dictionary<string, object> customParameters)
        {
            var subtreeTree = new BTInstance();
            subtreeTree.Initialise(LinkedBlackboard, $"{config.Name}_Subtree_{instanceName}");

            var dynamicFlowNode = new BTFlowNode_Dynamic(
                new FastName($"{config.Name}_DynamicFlow_{instanceName}"),
                subtreeTree,
                config.SuccessCriteria
            );

            var parameters = MergeParameters(config.PlannerParameters, customParameters);
            
            LogMessage($"🔧 SubtreeInjectionService: Creating ENHSP subtree with parameters:");
            LogMessage($"   Domain File: {parameters["domainFile"]}");
            LogMessage($"   Problem File: {parameters["problemFile"]}");
            LogMessage($"   Planner Path: {parameters["plannerPath"]}");
            LogMessage($"   Timeout: {parameters["timeoutSeconds"]} seconds");
            LogMessage($"   Max Plan Length: {parameters["maxPlanLength"]}");

            var pddlRequest = new PDDLPlanningRequest(
                parameters["domainFile"].ToString(),
                parameters["problemFile"].ToString(),
                parameters["plannerPath"].ToString(),
                "ENHSP",
                Convert.ToInt32(parameters["timeoutSeconds"]),
                Convert.ToInt32(parameters["maxPlanLength"])
            );

            var enhspPlanner = new CallPDDLPlanner(subtreeTree, pddlRequest);
            enhspPlanner.ExecutionMode = (CallPDDLPlanner.ParallelExecutionMode)parameters["executionMode"];

            dynamicFlowNode.SetPlanningService(enhspPlanner);
            subtreeTree.RootNode = dynamicFlowNode;

            return dynamicFlowNode;
        }

        /// <summary>
        /// Create GOAP subtree
        /// </summary>
        private BTFlowNode_Dynamic CreateGOAPSubtree(SubtreeConfiguration config, string instanceName, Dictionary<string, object> customParameters)
        {
            var subtreeTree = new BTInstance();
            subtreeTree.Initialise(LinkedBlackboard, $"{config.Name}_Subtree_{instanceName}");

            var dynamicFlowNode = new BTFlowNode_Dynamic(
                new FastName($"{config.Name}_DynamicFlow_{instanceName}"),
                subtreeTree,
                config.SuccessCriteria
            );

            var parameters = MergeParameters(config.PlannerParameters, customParameters);

            var goapRequest = new GOAPPlanningRequest
            {
                TimeoutSeconds = Convert.ToInt32(parameters["timeoutSeconds"]),
                MaxPlanLength = Convert.ToInt32(parameters["maxPlanLength"]),
                Domain = parameters["domain"].ToString(),
                EnableDebugLogging = Convert.ToBoolean(parameters["enableDebugLogging"]),
                HeuristicWeight = Convert.ToSingle(parameters["heuristicWeight"]),
                MaxSearchDepth = Convert.ToInt32(parameters["maxSearchDepth"])
            };

            // Set default GOAP state and goals if not provided
            if (!customParameters?.ContainsKey("initialState") == true)
            {
                goapRequest.InitialState = new Dictionary<string, object>
                {
                    ["robot_empty"] = true,
                    ["object_at_location"] = true,
                    ["location_free"] = true,
                    ["object_clear"] = true,
                    ["object_not_stacked"] = true
                };
            }

            if (!customParameters?.ContainsKey("goals") == true)
            {
                goapRequest.Goals = new Dictionary<string, object>
                {
                    ["robot_holding_object"] = true,
                    ["object_not_at_location"] = true,
                    ["robot_not_empty"] = true
                };
            }

            if (!customParameters?.ContainsKey("availableActions") == true)
            {
                goapRequest.AvailableActions = new List<string>
                {
                    "TravelML", "EquipeML", "PickUpML", "DeequipML"
                };
            }

            var goapPlanner = new CallGOAPPlanner(subtreeTree, goapRequest);

            dynamicFlowNode.SetPlanningService(goapPlanner);
            subtreeTree.RootNode = dynamicFlowNode;

            return dynamicFlowNode;
        }

     

        

        /// <summary>
        /// Merge default and custom parameters
        /// </summary>
        private Dictionary<string, object> MergeParameters(Dictionary<string, object> defaultParams, Dictionary<string, object> customParams)
        {
            var merged = new Dictionary<string, object>(defaultParams);
            
            LogMessage($"🔧 SubtreeInjectionService: Merging parameters - Default params: {defaultParams.Count}, Custom params: {customParams?.Count ?? 0}");
            
            if (customParams != null)
            {
                foreach (var kvp in customParams)
                {
                    var oldValue = defaultParams.ContainsKey(kvp.Key) ? defaultParams[kvp.Key].ToString() : "not set";
                    LogMessage($"🔧 SubtreeInjectionService: Overriding parameter {kvp.Key}: {oldValue} -> {kvp.Value}");
                    merged[kvp.Key] = kvp.Value;
                }
            }
            
            LogMessage($"🔧 SubtreeInjectionService: Final merged parameters count: {merged.Count}");
            return merged;
        }

        

        /// <summary>
        /// Create StateChart subtree
        /// </summary>
        private BTFlowNode_Dynamic CreateStateChartSubtree(SubtreeConfiguration config, string instanceName, Dictionary<string, object> customParameters)
        {
            var subtreeTree = new BTInstance();
            subtreeTree.Initialise(LinkedBlackboard, $"{config.Name}_Subtree_{instanceName}");

            var dynamicFlowNode = new BTFlowNode_Dynamic(
                new FastName($"{config.Name}_DynamicFlow_{instanceName}"),
                subtreeTree,
                config.SuccessCriteria
            );

            var parameters = MergeParameters(config.PlannerParameters, customParameters);

            var stateChartRequest = new StateChartPlanningRequest
            {
                TimeoutSeconds = Convert.ToInt32(parameters["timeoutSeconds"]),
                MaxPlanLength = Convert.ToInt32(parameters["maxPlanLength"]),
                CurrentState = parameters["currentState"]?.ToString() ?? "initial",
                TargetState = parameters["targetState"]?.ToString() ?? "final",
                AvailableTransitions = parameters["availableTransitions"] as List<string> ?? new List<string>()
            };

            var stateChartPlanner = new CallSCPlanner(subtreeTree, stateChartRequest);

            dynamicFlowNode.SetPlanningService(stateChartPlanner);
            subtreeTree.RootNode = dynamicFlowNode;

            return dynamicFlowNode;
        }
    }
}
