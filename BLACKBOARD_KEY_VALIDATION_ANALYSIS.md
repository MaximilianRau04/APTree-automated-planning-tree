# Blackboard Key Does Not Exist Issue - Design-Time Validation Analysis

## Problem Statement

In **behaviorTree.cpp** (and similar BT frameworks), a common runtime error is:
```
Blackboard key does not exist: beam1
```

This happens when an action node references a parameter (e.g., `beam="{beam1}"`) that hasn't been initialized on the blackboard. The error is only caught **at runtime** when the action tries to retrieve the value.

## Current State of Your DSL

### 1. Property Definition Model (ConcreteBT Grammar)
Your DSL currently defines objects in the **ConcreteBT.mc4** grammar:

```monticore
symbol Beam extends Element = "Beam" name:Name "(" lenght:DOUBLE_VALUE color:Name ")";
symbol Robot extends Agent = "Robot" name:Name "(" id:NUMBER ")";
```

✅ **This is excellent!** At property definition time, you declare:
- What types exist (Element, Agent, Location, etc.)
- What concrete instances are valid (Beam, Robot, FirstPosition, etc.)

### 2. BehaviorTree Model (BehaviorTree.mc4)
Your BehaviorTree grammar is quite generic:

```monticore
BehaviorTree = "BehaviorTree" Name "{" "root" root:FlowNode "}";
PickUp extends ActionNode = "Action" name:Name@BTNode ("{" (Decorator | Service)* "}")?;
```

⚠️ **Problem**: Action nodes reference parameters by generic `Name`, with no type checking:
- `PickUp name="pickup-beam1"` → name resolves to a BTNode
- But nowhere do you validate that `beam1`, `fp1`, etc. actually exist in your property definitions
- The parameters (beam, location, etc.) are not explicitly defined in the grammar

## How behaviorTree.cpp Handles This (and Why It Fails)

In **btcpp.xml** (C++ behavior tree):

```xml
<PickUp name="pickup-beam1" beam="{beam1}" location="fp1"/>
```

At runtime, the C++ framework:
1. Parses the XML
2. Finds the `PickUp` action node
3. Looks up `beam1` on the blackboard → **If not found, crashes!**
4. The framework has NO design-time knowledge of what keys should exist

## How Your DSL Can Solve This with Design-Time Validation

### Current State: You Have The Building Blocks

1. ✅ **Property Definition Model** - Declares all valid objects:
   ```monticore
   PropertyTypeDefinition = "define" name:Name "as" superType:Name "{" (Property)* "}";
   Property = name:Name ":" type:Name;
   ```
   This allows you to know: `beam1 : Beam`, `fp1 : FirstPosition`, etc.

2. ✅ **Action Type Definitions** - Declare what parameters actions need:
   ```monticore
   ActionTypeDefinition = 
       "Action" typeName:Name "{"
           "properties" "{" Property* "}"
           ...
       "}";
   ```

3. ⚠️ **Missing Piece** - BehaviorTree grammar doesn't reference these definitions

### Solution: Add Design-Time Validation with Context Rules (CoCoS)

You need to add **Context Conditions (CoCos)** to enforce rules at model check time.

## Proposed Architecture for Blackboard Key Validation

### Step 1: Enhance the BehaviorTree Grammar (Optional, for explicit properties)

You could optionally make action parameters explicit in the grammar. For example:

```monticore
// Option A: Minimal change - just add a comment showing expected types
ActionInvocation extends ActionNode = 
    "Action" actionType:Name@ActionTypeDefinition 
    name:Name 
    (parameters:ParameterBinding)*;

ParameterBinding = paramName:Name "=" "\"" paramValue:Name "\"";

// Option B: More explicit - use references
ParameterBinding = 
    paramName:Name "=" value:Name@Property;
```

### Step 2: Create a CoCoChecker for Blackboard Key Validation (RECOMMENDED)

Create a **CoCo rule** that validates:

1. **Every action node in the BehaviorTree references actions defined in CRFTypeDef**
2. **All parameters used in action invocations match the action's property definitions**
3. **All referenced parameter values exist as instances in the property definitions**

#### Example CoCoChecker Structure (Java/MontiCore style):

```java
public class BlackboardKeyExistenceCoCoChecker {
    
    /**
     * CoCo Rule: Every action invocation must have all required parameters
     * that exist in the property definitions
     */
    public void check(ASTBehaviorTree btModel, ASTCRFTypeDef propertyModel) {
        // 1. Build a map of all available instances from propertyModel
        Map<String, ASTProperty> availableInstances = buildInstanceMap(propertyModel);
        
        // 2. Traverse all action nodes in the BehaviorTree
        List<ASTActionNode> actionNodes = btModel.getActionNodes();
        for (ASTActionNode action : actionNodes) {
            // 3. Get the action's type definition
            String actionTypeName = action.getActionType();
            ASTActionTypeDefinition actionDef = findActionDefinition(actionTypeName, propertyModel);
            
            if (actionDef == null) {
                addError("Action type not found: " + actionTypeName, action);
                continue;
            }
            
            // 4. For each property in the action definition
            for (ASTProperty expectedParam : actionDef.getProperties()) {
                String paramName = expectedParam.getName();
                String paramValue = action.getParameterValue(paramName);
                
                if (paramValue == null || paramValue.isEmpty()) {
                    addError("Missing parameter: " + paramName + " in action " + action.getName(), action);
                    continue;
                }
                
                // 5. Check if the parameter value exists in available instances
                if (!availableInstances.containsKey(paramValue)) {
                    addError("Blackboard key does not exist: " + paramValue + 
                            " (expected type: " + expectedParam.getType() + ")", action);
                }
                
                // 6. Check type compatibility
                ASTProperty actualInstance = availableInstances.get(paramValue);
                if (!isTypeCompatible(expectedParam.getType(), actualInstance.getType())) {
                    addError("Type mismatch: parameter '" + paramName + "' expects type " + 
                            expectedParam.getType() + " but got " + actualInstance.getType(), action);
                }
            }
        }
    }
    
    private Map<String, ASTProperty> buildInstanceMap(ASTCRFTypeDef propertyModel) {
        Map<String, ASTProperty> instances = new HashMap<>();
        
        for (ASTPropertyTypeDefinition propDef : propertyModel.getPropertyTypeDefinitions()) {
            for (ASTProperty prop : propDef.getProperties()) {
                // Key: instance name, Value: property (with type info)
                instances.put(prop.getName(), prop);
            }
        }
        
        return instances;
    }
    
    private ASTActionTypeDefinition findActionDefinition(String actionTypeName, ASTCRFTypeDef propertyModel) {
        for (ASTActionTypeDefinition action : propertyModel.getActionTypeDefinitions()) {
            if (action.getTypeName().equals(actionTypeName)) {
                return action;
            }
        }
        return null;
    }
    
    private boolean isTypeCompatible(String expectedType, String actualType) {
        // Check if actualType is a subtype of expectedType
        // This would need to traverse the type hierarchy
        return expectedType.equals(actualType);
    }
}
```

### Step 3: Integration into Your IDE (Optional)

Using MontiCore and your Java setup, you can:

1. **On-Save Validation**: When a user saves a `.dsl` file, automatically run the CoCoChecker
2. **Error Reporting**: Show underlines in the editor for unresolved references (like VS Code's diagnostic API)
3. **Quick Fixes**: Suggest available instances when a key is missing

## Current Workflow vs Proposed Workflow

### ❌ Current (Implicit, Runtime Errors):
```
User creates property definitions:
  → Beam beam1(...)
  
User creates BT model:
  → Action PickUp name=... beam="beam1"
  
User runs the system:
  → ERROR: Blackboard key does not exist: beam1
  
Debugging: User manually traces back to property definitions
```

### ✅ Proposed (Explicit, Design-Time Validation):
```
User creates property definitions:
  → Beam beam1(...)
  
User creates BT model:
  → Action PickUp name=... beam="beam1"
  
IDE/DSL immediately checks:
  ✓ PickUp action type exists?
  ✓ PickUp expects a "beam" parameter?
  ✓ "beam1" instance exists in property definitions?
  ✓ "beam1" is of correct type (Element/Beam)?
  
User sees error/warning BEFORE running → FIXED AT DESIGN TIME
```

## Implementation Roadmap

### Phase 1: Add Grammar Enhancements (MINIMAL)
If your BehaviorTree model doesn't currently track parameters explicitly, enhance it:

```monticore
// In BehaviorTree.mc4
ActionNode extends BTNode = 
    "Action" actionType:Name@ActionTypeDefinition
    name:Name 
    ("{" (Decorator | Service)* "}")?
    ;
    
// In CRFTypeDef.mc4
ActionTypeDefinition = 
    "Action" typeName:Name "{"
        "acttype:" actLevel:ActionLevel
        "properties" "{" 
            (parameterDefinitions:PropertyDefinition)* 
        "}"
        "preconditions" "{" preconditions:PredicateRef* "}"
        "effects" "{" effects:PredicateRef* "}"
    "}";
```

### Phase 2: Create CoCoCheckers (PRIMARY)
```
MontiCoreTool/
  src/main/java/cocos/
    ├── BlackboardKeyExistenceCoCoChecker.java
    ├── ActionParameterTypeChecker.java
    └── PredicateReferenceChecker.java
```

### Phase 3: Integrate into Your IDE/Tool
- Hook into MontiCore's infrastructure to run CoCoCheckers on model load/save
- Report errors through MontiCore's error reporting system

### Phase 4: Add Quick Fixes (Future)
- "Create missing instance in property definitions"
- "Show available instances of type X"

## Key Advantages of This Approach Over behaviorTree.cpp

| Aspect | behaviorTree.cpp | Your DSL (with CoCoS) |
|--------|------------------|----------------------|
| **Blackboard key validation** | Runtime only ❌ | Design-time ✅ |
| **Type checking** | None ❌ | Full type system ✅ |
| **Error Detection** | When action executes ❌ | When model is checked ✅ |
| **Development Speed** | Slow (test → debug → fix) ❌ | Fast (write → validate → run) ✅ |
| **IDE Support** | Limited ❌ | Excellent (with MontiCore integration) ✅ |

## Files You'll Need to Modify/Create

### To Implement:

1. **MontiCoreTool/src/main/java/cocos/BlackboardKeyExistenceCoCoChecker.java** (NEW)
   - Main CoCo rule for blackboard key validation

2. **BehaviorTree.mc4** (OPTIONAL ENHANCEMENT)
   - Add explicit action type references: `actionType:Name@ActionTypeDefinition`
   - Add explicit parameter tracking

3. **CRFTypeDef.mc4** (ENHANCEMENT)
   - Ensure ActionTypeDefinition clearly defines its parameters

4. **MontiCoreTool/src/main/java/ModelChecker.java** or similar (MODIFY)
   - Register and execute the new CoCoChecker

## Summary

**Can your DSL catch blackboard key issues at design time?**

✅ **YES!** You have everything you need:
- Property definitions (ConcreteBT.mc4) define what instances exist
- Action definitions (CRFTypeDef.mc4) define what parameters are needed
- You just need to **add CoCoCheckers** to validate that BT models respect these definitions

This is a **major advantage over behaviorTree.cpp**, which only detects these errors at runtime!

---

## Next Steps

Would you like me to:
1. Create the `BlackboardKeyExistenceCoCoChecker.java` implementation?
2. Enhance the grammars to explicitly support this validation?
3. Show how to integrate the CoCoChecker into your MontiCore tool setup?
