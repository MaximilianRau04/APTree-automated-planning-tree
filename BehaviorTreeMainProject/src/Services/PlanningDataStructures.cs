using System;
using System.Collections.Generic;

namespace PlanningDataStructures
{
    // Base interface for all planning requests
    public interface IPlanningRequest
    {
        string PlanningType { get; }
    }

    // PDDL-specific request
    public class PDDLPlanningRequest : IPlanningRequest
    {
        public string PlanningType => "PDDL";
        public string DomainFile { get; set; }
        public string ProblemFile { get; set; }
        public string PlannerPath { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxPlanLength { get; set; } = 20;
        
    }

    // GOAP-specific request
    public class GOAPPlanningRequest : IPlanningRequest
    {
        public string PlanningType => "GOAP";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxPlanLength { get; set; } = 20;
        
        // GOAP-specific state and goals (key-value pairs)
        public Dictionary<string, object> InitialState { get; set; }  // World state as key-value pairs
        public Dictionary<string, object> Goals { get; set; }         // Goal state as key-value pairs
        public List<string> AvailableActions { get; set; }            // GOAP needs available actions
    }

    // StateChart-specific request
    public class StateChartPlanningRequest : IPlanningRequest
    {
        public string PlanningType => "StateChart";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxPlanLength { get; set; } = 20;
        
        // StateChart-specific state and goals
        public string CurrentState { get; set; }   // Current state machine state
        public string TargetState { get; set; }    // Target state to reach
        public List<string> AvailableTransitions { get; set; }  // Available state transitions
    }

    // Reinforcement Learning request
    public class RLPlanningRequest : IPlanningRequest
    {
        public string PlanningType => "RL";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxSteps { get; set; } = 100;
        
        // RL-specific parameters
        public string EnvironmentState { get; set; }  // Current environment state
        public string Objective { get; set; }         // Learning objective
        public Dictionary<string, object> Parameters { get; set; }  // RL parameters (epsilon, learning rate, etc.)
    }

    
    // Response classes for receiving from external planner
    public class PlanningResult
    {
        public bool Success { get; set; }
        public string Plan { get; set; } // Plan as string (like NodeGraph format)
        public string Error { get; set; } // Error as string
        public double PlanningTimeSeconds { get; set; }
        public int PlanLength { get; set; }
        public string PlannerUsed { get; set; }
    }

    // Enum for planning types
    public enum PlanningType
    {
        PDDL,
        GOAP,
        StateChart,
        ReinforcementLearning
    }
}
