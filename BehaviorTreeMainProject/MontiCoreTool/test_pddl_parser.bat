@echo off
echo Testing PDDL Planner Parser
echo ==========================

echo.
echo Step 1: Compiling test class...
javac -cp "target/generated-sources/monticore/sourcecode/pddlplanner;target/generated-sources/monticore/sourcecode/behaviortree;target/generated-sources/monticore/sourcecode/crf;target/generated-sources/monticore/sourcecode/dynamicbtflownode;target/dependency/*" src/main/java/PDDLPlannerTest.java -d target/test-classes

if %errorlevel% neq 0 (
    echo Compilation failed!
    pause
    exit /b 1
)

echo.
echo Step 2: Running parser test...
java -cp "target/test-classes;target/generated-sources/monticore/sourcecode/pddlplanner;target/generated-sources/monticore/sourcecode/behaviortree;target/generated-sources/monticore/sourcecode/crf;target/generated-sources/monticore/sourcecode/dynamicbtflownode;target/dependency/*" PDDLPlannerTest

echo.
echo Test completed!
pause