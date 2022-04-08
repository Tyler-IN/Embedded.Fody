using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

public sealed partial class ModuleWeaver {

  private unsafe FieldReference GetEmbeddedResourceAccessorType(string resourceName) {
    var tName = "$$" + resourceName;
    var td = ModuleDefinition.GetType(tName);
    if (td != null)
      return td.Fields.First();

    td = new(null,
      tName,
      TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
      _trObject);

    var fPointer = new FieldDefinition("Pointer",
      FieldAttributes.Assembly | FieldAttributes.Static | FieldAttributes.InitOnly,
      _trBytePtr);
    td.Fields.Add(fPointer);

    var mStaticCtor = new MethodDefinition(".cctor",
      MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
      _trVoid);
    {
      var il = mStaticCtor.Body.GetILProcessor();

      var getTypeFromHandle = ModuleDefinition.ImportReference
        (_trType.Resolve().GetMethods().First(m => m.Name == nameof(Type.GetTypeFromHandle)));
      var assemblyGetter = ModuleDefinition.ImportReference
        (_trType.Resolve().Properties.First(p => p.Name == nameof(Type.Assembly)).GetMethod);
      var tdAsm = _trAssembly.Resolve();
      var getManifestResourceStream = ModuleDefinition.ImportReference
        (tdAsm.GetMethods().First(m => m.Name == nameof(Assembly.GetManifestResourceStream)));
      var tdUms = _trUnmanagedMemoryStream.Resolve();
      var positionPointerGetter = tdUms == null // wtf
        ? ModuleDefinition.ImportReference(typeof(UnmanagedMemoryStream).GetProperty(nameof(UnmanagedMemoryStream.PositionPointer))?.GetMethod)
        : ModuleDefinition.ImportReference(tdUms.Properties.First(p => p.Name == nameof(UnmanagedMemoryStream.PositionPointer)).GetMethod);

      il.Emit(OpCodes.Ldtoken, td);
      il.Emit(OpCodes.Call, getTypeFromHandle);
      il.Emit(OpCodes.Callvirt, assemblyGetter);
      il.Emit(OpCodes.Ldstr, resourceName);
      il.Emit(OpCodes.Callvirt, getManifestResourceStream);
      il.Emit(OpCodes.Castclass, _trUnmanagedMemoryStream); // GetManifestResourceStream
      //il.Emit(OpCodes.Dup);
      il.Emit(OpCodes.Callvirt, positionPointerGetter);
      il.Emit(OpCodes.Stsfld, fPointer);
      //il.Emit(OpCodes.Callvirt, new MethodReference("Dispose", _trVoid, _trStream));
      il.Emit(OpCodes.Ret);
    }
    mStaticCtor.Body.Optimize();
    td.Methods.Add(mStaticCtor);

    ModuleDefinition.Types.Add(td);

    return fPointer;
  }

}