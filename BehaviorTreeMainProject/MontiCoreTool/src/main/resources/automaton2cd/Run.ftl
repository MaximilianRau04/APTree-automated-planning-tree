<#-- (c) https://github.com/MontiCore/monticore -->
${tc.signature("constants")}
${cd4c.method("public boolean run(List<Stimuli> stimuli)")}

for(Stimuli stimulus: stimuli) {
  switch(stimulus) {
    <#list constants as constant>
      case ${constant} : {
        this.${constant}();
        break;
      }
    </#list>
      default:
        de.se_rwth.commons.logging.Log.error("0x25371 no defined stimulus for input " + stimulus);
  }
}
return this.state.isIsFinal();