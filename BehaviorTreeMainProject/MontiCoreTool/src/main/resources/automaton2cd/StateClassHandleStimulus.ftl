<#-- (c) https://github.com/MontiCore/monticore -->
<#--
  Fills in a method with the state-action implementation

-->
${tc.signature("stimulus", "className", "targetStateAttrName")}
${cd4c.method("public void handle${stimulus?cap_first}( ${className} k)")}

k.setState(k.get${targetStateAttrName}());

