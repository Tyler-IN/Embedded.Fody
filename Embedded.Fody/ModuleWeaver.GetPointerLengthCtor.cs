using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

public sealed partial class ModuleWeaver {

  private MethodReference GetPointerLengthCtor(TypeReference fieldType, out int ctorParamCount) {
    var gitField = fieldType as GenericInstanceType;
    var tdField = gitField == null
      ? fieldType.Resolve()
      : gitField.Resolve();

    var ctorDef = tdField
      .GetConstructors()
      .Where(c => {
        var ps = c.Parameters;
        var pc = ps.Count;
        if (pc < 1 || pc > 2)
          return false;

        var firstParamType = ps[0].ParameterType;
        if (firstParamType is GenericParameter gp1)
          firstParamType = ((GenericInstanceType) fieldType).GenericArguments[gp1.Position];
        if (!(firstParamType.IsPointer
          || (firstParamType.Namespace == "System" && firstParamType.Name == "IntPtr")))
          return false;

        if (pc == 1)
          return true;

        var secondParamType = ps[1].ParameterType;
        if (secondParamType is GenericParameter gp2)
          secondParamType = ((GenericInstanceType) fieldType).GenericArguments[gp2.Position];

        return secondParamType.IsPrimitive && secondParamType.Namespace == "System"
          && (secondParamType.Name == "Int32" || secondParamType.Name == "Int64"
            || secondParamType.Name == "UInt32" || secondParamType.Name == "UInt64");
      })
      .OrderByDescending(c => c.Parameters.Count)
      .First();
    ctorParamCount = ctorDef.Parameters.Count;

    if (fieldType is GenericInstanceType fieldGit)
      return ModuleDefinition.ImportReference( MakeHostInstanceGeneric(ModuleDefinition.ImportReference( ctorDef ), fieldGit.GenericArguments.ToArray()), fieldGit );

    return ModuleDefinition.ImportReference( ctorDef );
  }

  public static MethodReference MakeHostInstanceGeneric(MethodReference methodRef, params TypeReference[] args) {
    var reference = new MethodReference(
      methodRef.Name,
      methodRef.ReturnType,
      methodRef.DeclaringType.MakeGenericInstanceType(args)) {
      HasThis = methodRef.HasThis,
      ExplicitThis = methodRef.ExplicitThis,
      CallingConvention = methodRef.CallingConvention
    };

    foreach (var parameter in methodRef.Parameters)
      reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

    foreach (var genericParam in methodRef.GenericParameters)
      reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));

    return reference;
  }

}