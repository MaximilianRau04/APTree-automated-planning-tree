@echo off
REM Test script for parsing CRFConcreteInstances.bt

echo === Building MontiCore Tool ===
call gradlew.bat build -x test

echo.
echo === Running ConcreteBT Instance Parser ===
call gradlew.bat run -PmainClass=ConcreteBTInstanceParser --args="src/test/resources/valid/CRFConcrete/CRFConcreteInstances.bt"

echo.
echo === Done ===
pause
