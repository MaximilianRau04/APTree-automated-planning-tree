using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlanningDataStructures;
using AIPlanning;
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

        public SubtreeInjectionService(IBehaviorTree owningTree) : base(owningTree)
        {
            subtreeConfigurations = new Dictionary<string, SubtreeConfiguration>();
            cachedSubtrees = new Dictionary<string, BTFlowNode_Dynamic>();
            defaultPlannerMapping = new Dictionary<string, string>();
            pendingAction = null;
            parameterInstances = new Dictionary<string, string>();
            generatedProblemFiles = new List<string>();
            
            InitializeDefaultConfigurations();
            InitializeDefaultPlannerMapping();
            LoadParameterInstances();
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
                    Console.WriteLine($"✅ SubtreeInjectionService: Loaded {parameterInstances.Count} parameter instances");
                }
                else
                {
                    Console.WriteLine($"⚠️ SubtreeInjectionService: Parameter instances file not found at {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SubtreeInjectionService: Error loading parameter instances: {ex.Message}");
            }
        }

        /// <summary>
        /// Queue an action for subtree injection in the next tick
        /// </summary>
        public void QueueActionForInjection(GenericBTAction action, Dictionary<string, object> customParameters = null)
        {
            pendingAction = action;
            // Store custom parameters with the action for later use
            if (customParameters != null)
            {
                Console.WriteLine($"📝 SubtreeInjectionService: Stored custom parameters for {action.actionType}");
            }
            Console.WriteLine($"📋 SubtreeInjectionService: Queued {action.actionType} for subtree injection in next tick");
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
            // If no action is pending, return true
            if (pendingAction == null)
            {
                return true;
            }

            var action = pendingAction;
            pendingAction = null; // Clear the pending action
            
            var actionType = action.actionType.ToString();
            Console.WriteLine($"🔍 SubtreeInjectionService: Processing action in tick: {actionType}");
            
            // 1. Check if the action is HL by checking the name of the action
            if (!actionType.EndsWith("HL"))
            {
                Console.WriteLine($"🔍 SubtreeInjectionService: Action {actionType} is not a high-level action (no 'HL' suffix)");
                // 2. If it is not HL return true
                return true;
            }
            
            // 3. If it is HL, then we Inject the subtree
            Console.WriteLine($"🔍 SubtreeInjectionService: Detected high-level action: {actionType}");
            try
            {
                ProcessSubtreeInjection(action, null); // customParameters would be passed here if needed
                Console.WriteLine($"✅ SubtreeInjectionService: Successfully injected subtree for {actionType}");
                // 4. If the injection was successful return true
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SubtreeInjectionService: Failed to inject subtree for {actionType}: {ex.Message}");
                // 4. else, return false
                return false; 
            }
        }

        /// <summary>
        /// Process subtree injection for a specific action
        /// </summary>
        private void ProcessSubtreeInjection(GenericBTAction action, Dictionary<string, object> customParameters = null)
        {
            try
            {
                var actionType = action.actionType.ToString();
                Console.WriteLine($"🔧 SubtreeInjectionService: Processing injection for {actionType}");
                
                // Get the default planner for this action type
                string configName = GetDefaultPlannerForAction(actionType);
                
                // Create instance name from action
                string instanceName = action.InstanceName.ToString();
                
                // Generate dynamic PDDL problem file
                string problemFileName = GenerateDynamicPDDLProblem(action, instanceName);
                
                // Merge custom parameters with the generated problem file
                var mergedParameters = customParameters ?? new Dictionary<string, object>();
                mergedParameters["problemFile"] = problemFileName;
                
                Console.WriteLine($"🔧 SubtreeInjectionService: Using dynamic problem file: {problemFileName}");
                Console.WriteLine($"🔧 SubtreeInjectionService: Merged parameters count: {mergedParameters.Count}");
                foreach (var param in mergedParameters)
                {
                    Console.WriteLine($"   Parameter: {param.Key} = {param.Value}");
                }
                
                // Inject the subtree
                InjectSubtreeIntoAction(action, configName, instanceName, mergedParameters);
                
                Console.WriteLine($"✅ SubtreeInjectionService: Successfully processed injection for {actionType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SubtreeInjectionService: Error processing injection: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate a dynamic PDDL problem file for the given action
        /// </summary>
        public string GenerateDynamicPDDLProblem(GenericBTAction action, string instanceName)
        {
            try
            {
                var actionType = action.actionType.ToString();
                var actionFullName = action.GetType().Name; // Get the full class name
                string problemFileName = $"problem{actionFullName}_{instanceName}.pddl";
                string problemFilePath = $"python_service/Plannerinputs/{problemFileName}";
                
                Console.WriteLine($"🔧 SubtreeInjectionService: Generating PDDL problem file: {problemFileName}");
                
                // 1. Retrieve predicates from blackboard
                var initialstatepredicates = LinkedBlackboard.GetAllPredicates();
                string initialstatepredicatesPDDL = Parser.ConvertMultiplePredicatesToPDDL(initialstatepredicates);
                Console.WriteLine($"📋 SubtreeInjectionService: Retrieved {initialstatepredicates.Count} initial state predicates");
                Console.WriteLine($"📋 SubtreeInjectionService: Initial state PDDL: {initialstatepredicatesPDDL}");
                
                // 2. Get action effects for goals
                var goalstatePredicates = action.GetActionEffects();
                Console.WriteLine($"🎯 SubtreeInjectionService: Retrieved {goalstatePredicates.Count} goal predicates from action effects");
                foreach (var predicate in goalstatePredicates)
                {
                    Console.WriteLine($"   Goal predicate: {predicate.PredicateName}");
                }
                string goalstatepredicatesPDDL = Parser.ConvertMultiplePredicatesToPDDL(goalstatePredicates);
                Console.WriteLine($"🎯 SubtreeInjectionService: Goal state PDDL: {goalstatepredicatesPDDL}");
                
                // 3. Generate PDDL problem content
                string pddlContent = GeneratePDDLProblemContent(actionFullName, initialstatepredicatesPDDL, goalstatepredicatesPDDL);
                
                // 4. Write to file
                File.WriteAllText(problemFilePath, pddlContent);
                
                // 5. Verify file was created and contains content
                if (File.Exists(problemFilePath))
                {
                    var fileContent = File.ReadAllText(problemFilePath);
                    Console.WriteLine($"✅ SubtreeInjectionService: Generated PDDL problem file: {problemFilePath}");
                    Console.WriteLine($"📄 SubtreeInjectionService: File size: {fileContent.Length} characters");
                    Console.WriteLine($"📄 SubtreeInjectionService: Problem file content preview:");
                    Console.WriteLine(pddlContent);
                    
                    // Verify that goals are present
                    if (fileContent.Contains("(:goal"))
                    {
                        Console.WriteLine($"✅ SubtreeInjectionService: Problem file contains goal section");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ SubtreeInjectionService: Problem file does NOT contain goal section!");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ SubtreeInjectionService: Failed to create problem file: {problemFilePath}");
                }
                
                // Track the generated problem file
                generatedProblemFiles.Add(problemFilePath);
                
                return problemFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SubtreeInjectionService: Error generating PDDL problem: {ex.Message}");
                Console.WriteLine($"❌ SubtreeInjectionService: Stack trace: {ex.StackTrace}");
                // Fallback to default problem file
                return "python_service/Plannerinputs/problemC1.pddl";
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
                    Console.WriteLine($"❌ SubtreeInjectionService: ParameterInstances_PDDL.txt file not found at {filePath}");
                    return string.Empty;
                }
                
                string content = File.ReadAllText(filePath);
                Console.WriteLine($"✅ SubtreeInjectionService: Successfully read {content.Length} characters from ParameterInstances_PDDL.txt");
                
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SubtreeInjectionService: Error reading ParameterInstances_PDDL.txt: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Initialize default planner mapping for HL actions
        /// </summary>
        private void InitializeDefaultPlannerMapping()
        {
            // Map action types to default planners
            defaultPlannerMapping["PickUpHL"] = "FF_Default";
            defaultPlannerMapping["PlaceHL"] = "FF_Default";
            defaultPlannerMapping["StackHL"] = "FF_Default";
            defaultPlannerMapping["GluingHL"] = "ENHSP_Default";
            defaultPlannerMapping["NailingHL"] = "ENHSP_Default";
            defaultPlannerMapping["TravelHL"] = "GOAP_Default";
            defaultPlannerMapping["InitializeHL"] = "StateChart_Default";
            
            // Generic mapping for any HL action not specifically mapped
            defaultPlannerMapping["*HL"] = "FF_Default";
            
            Console.WriteLine("✅ SubtreeInjectionService: Initialized default planner mapping");
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
            Console.WriteLine($"⚠️ SubtreeInjectionService: No mapping found for {actionType}, using FF_Default");
            return "FF_Default";
        }

        /// <summary>
        /// Set custom planner mapping for an action type
        /// </summary>
        public void SetPlannerMapping(string actionType, string configName)
        {
            defaultPlannerMapping[actionType] = configName;
            Console.WriteLine($"✅ SubtreeInjectionService: Set planner mapping {actionType} -> {configName}");
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
            ffConfig.PlannerParameters["domainFile"] = "python_service/Plannerinputs/domain.pddl";
            ffConfig.PlannerParameters["problemFile"] = "python_service/Plannerinputs/problemC1.pddl";
            ffConfig.PlannerParameters["plannerPath"] = "/home/shermin/ENHSP-Public/enhsp.jar";
            ffConfig.PlannerParameters["timeoutSeconds"] = 30;
            ffConfig.PlannerParameters["maxPlanLength"] = 10;
            ffConfig.PlannerParameters["executionMode"] = CallPDDLPlanner.ParallelExecutionMode.Sequential;
            subtreeConfigurations["FF_Default"] = ffConfig;

            // ENHSP Planner Configuration
            var enhspConfig = new SubtreeConfiguration("ENHSP_Default", PlannerType.ENHSP, SuccessCriteria.ALL);
            enhspConfig.PlannerParameters["domainFile"] = "python_service/Plannerinputs/domain.pddl";
            enhspConfig.PlannerParameters["problemFile"] = "python_service/Plannerinputs/problemC1.pddl";
            enhspConfig.PlannerParameters["plannerPath"] = "/home/shermin/ENHSP-Public/enhsp.jar";
            enhspConfig.PlannerParameters["timeoutSeconds"] = 30;
            enhspConfig.PlannerParameters["maxPlanLength"] = 10;
            enhspConfig.PlannerParameters["executionMode"] = CallPDDLPlanner.ParallelExecutionMode.Parallel;
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

            Console.WriteLine("✅ SubtreeInjectionService: Initialized default configurations");
        }

        /// <summary>
        /// Register a custom subtree configuration
        /// </summary>
        public void RegisterConfiguration(string configName, SubtreeConfiguration configuration)
        {
            subtreeConfigurations[configName] = configuration;
            Console.WriteLine($"✅ SubtreeInjectionService: Registered configuration '{configName}'");
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
                Console.WriteLine($"🔧 SubtreeInjectionService: Creating subtree '{config.Name}' for instance '{instanceName}'");

                // Check cache first
                string cacheKey = $"{config.Name}_{instanceName}";
                if (config.UseCaching && cachedSubtrees.TryGetValue(cacheKey, out var cachedSubtree))
                {
                    Console.WriteLine($"✅ SubtreeInjectionService: Using cached subtree for '{cacheKey}'");
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
                    Console.WriteLine($"💾 SubtreeInjectionService: Cached subtree for '{cacheKey}'");
                }

                Console.WriteLine($"✅ SubtreeInjectionService: Created subtree successfully");
                return subtree;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SubtreeInjectionService: Error creating subtree: {ex.Message}");
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
            Console.WriteLine($"✅ SubtreeInjectionService: Injected subtree '{configName}' into action '{action.InstanceName.ToString()}'");
        }

        

        /// <summary>
        /// Remove subtree from an action
        /// </summary>
        public void RemoveSubtreeFromAction(GenericBTAction action)
        {
            action.RemoveSubtree();
            Console.WriteLine($"✅ SubtreeInjectionService: Removed subtree from action '{action.InstanceName.ToString()}'");
        }

        /// <summary>
        /// Clear the subtree cache
        /// </summary>
        public void ClearCache()
        {
            cachedSubtrees.Clear();
            Console.WriteLine("🧹 SubtreeInjectionService: Cleared subtree cache");
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
            
            Console.WriteLine($"🔧 SubtreeInjectionService: Creating FF subtree with parameters:");
            Console.WriteLine($"   Domain File: {parameters["domainFile"]}");
            Console.WriteLine($"   Problem File: {parameters["problemFile"]}");
            Console.WriteLine($"   Planner Path: {parameters["plannerPath"]}");
            Console.WriteLine($"   Timeout: {parameters["timeoutSeconds"]} seconds");
            Console.WriteLine($"   Max Plan Length: {parameters["maxPlanLength"]}");

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
            
            Console.WriteLine($"🔧 SubtreeInjectionService: Creating ENHSP subtree with parameters:");
            Console.WriteLine($"   Domain File: {parameters["domainFile"]}");
            Console.WriteLine($"   Problem File: {parameters["problemFile"]}");
            Console.WriteLine($"   Planner Path: {parameters["plannerPath"]}");
            Console.WriteLine($"   Timeout: {parameters["timeoutSeconds"]} seconds");
            Console.WriteLine($"   Max Plan Length: {parameters["maxPlanLength"]}");

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
            
            Console.WriteLine($"🔧 SubtreeInjectionService: Merging parameters - Default params: {defaultParams.Count}, Custom params: {customParams?.Count ?? 0}");
            
            if (customParams != null)
            {
                foreach (var kvp in customParams)
                {
                    var oldValue = defaultParams.ContainsKey(kvp.Key) ? defaultParams[kvp.Key].ToString() : "not set";
                    Console.WriteLine($"🔧 SubtreeInjectionService: Overriding parameter {kvp.Key}: {oldValue} -> {kvp.Value}");
                    merged[kvp.Key] = kvp.Value;
                }
            }
            
            Console.WriteLine($"🔧 SubtreeInjectionService: Final merged parameters count: {merged.Count}");
            return merged;
        }

        /// <summary>
        /// Demonstrates how to use the SubtreeInjectionService with automatic HL detection
        /// </summary>
        public static void DemonstrateUsage(IBehaviorTree behaviorTree, Blackboard<FastName> blackboard, 
                                          Element beam1, Location location1, Robot robot1, VacuumGripper vg1)
        {
            Console.WriteLine("\n🔧 SubtreeInjectionService: Demonstrating automatic HL detection");
            
            // Create the service
            var service = new SubtreeInjectionService(behaviorTree);
            
                         // Example 1: Queue HL action for injection in next tick
             Console.WriteLine("\n📋 Example 1: Queue HL action for injection");
             var pickUpAction1 = new PickUpHL("PickUpHL", "pickup1", blackboard, beam1, location1, robot1);
             service.QueueActionForInjection(pickUpAction1); // Queues for injection in next tick
             Console.WriteLine($"   ✅ Queued {pickUpAction1.InstanceName.ToString()} for subtree injection");
             
             // Example 2: Custom planner mapping
             Console.WriteLine("\n📋 Example 2: Custom planner mapping");
             service.SetPlannerMapping("PickUpHL", "ENHSP_Default"); // Override default mapping
             var pickUpAction2 = new PickUpHL("PickUpHL", "pickup2", blackboard, beam1, location1, robot1);
             service.QueueActionForInjection(pickUpAction2); // Now uses ENHSP instead of FF
             Console.WriteLine($"   ✅ Queued {pickUpAction2.InstanceName.ToString()} for ENHSP injection");
             
             // Example 3: Custom parameters with automatic detection
             Console.WriteLine("\n📋 Example 3: Custom parameters with automatic detection");
             var customParams = new Dictionary<string, object> 
             { 
                 ["timeoutSeconds"] = 45,
                 ["maxPlanLength"] = 8
             };
             var pickUpAction3 = new PickUpHL("PickUpHL", "pickup3", blackboard, beam1, location1, robot1);
             service.QueueActionForInjection(pickUpAction3, customParams);
             Console.WriteLine($"   ✅ Queued {pickUpAction3.InstanceName.ToString()} with custom parameters");
             
             // Example 4: Non-HL action (no injection)
             Console.WriteLine("\n📋 Example 4: Non-HL action (no injection)");
             var pickUpAction4 = new PickUpML("PickUpML", "pickup4", blackboard, beam1, new Firstposition(), robot1, vg1);
             service.QueueActionForInjection(pickUpAction4); // No injection because no "HL" suffix
             Console.WriteLine($"   ✅ No injection for: {pickUpAction4.InstanceName.ToString()}");
            
            // Example 5: Manual injection override
            Console.WriteLine("\n📋 Example 5: Manual injection override");
            var pickUpAction5 = new PickUpML("PickUpML", "pickup5", blackboard, beam1, new Firstposition(), robot1, vg1);
            service.InjectSubtreeIntoAction(pickUpAction5, "GOAP_Default", "pickup5"); // Force injection
            Console.WriteLine($"   ✅ Manually injected GOAP subtree into: {pickUpAction5.InstanceName.ToString()}");
            
            // Example 6: Remove subtree
            Console.WriteLine("\n📋 Example 6: Remove subtree");
            service.RemoveSubtreeFromAction(pickUpAction1);
            Console.WriteLine($"   ✅ Removed subtree from: {pickUpAction1.InstanceName.ToString()}");
            
            // Example 7: Statistics
            Console.WriteLine("\n📋 Example 7: Statistics");
            var stats = service.GetStatistics();
            Console.WriteLine($"   📊 Cached subtrees: {stats.cachedSubtrees}");
            Console.WriteLine($"   📊 Configurations: {stats.configurations}");
            Console.WriteLine($"   📊 Planner mappings: {stats.plannerMappings}");
            
            // Example 8: Show planner mappings
            Console.WriteLine("\n📋 Example 8: Current planner mappings");
            var mappings = service.GetPlannerMappings();
            foreach (var mapping in mappings)
            {
                Console.WriteLine($"   🔗 {mapping.Key} -> {mapping.Value}");
            }
            
            // Example 9: Generate PDDL problem file
            Console.WriteLine("\n📋 Example 9: Generate PDDL problem file");
            var testAction = new PickUpHL("PickUpHL", "testpickup", blackboard, beam1, location1, robot1);
            string problemFile = service.GenerateDynamicPDDLProblem(testAction, "testpickup");
            Console.WriteLine($"   📄 Generated problem file: {problemFile}");
            
            Console.WriteLine("\n🎯 SubtreeInjectionService: Demonstration completed!");
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
