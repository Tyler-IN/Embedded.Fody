using System;
using Mono.Cecil.Cil;

public sealed partial class ModuleWeaver {

  private static Instruction CreateLoadConstInt(ILProcessor ilp, long value)
    => value is > int.MaxValue or < int.MinValue
      ? ilp.Create(OpCodes.Ldc_I8, value)
      : value is > sbyte.MaxValue or < sbyte.MinValue
        ? ilp.Create(OpCodes.Ldc_I4, (int) value)
        : value is < -1 or > 8
          ? ilp.Create(OpCodes.Ldc_I4_S, (sbyte) value)
          : ilp.Create(value switch {
            -1 => OpCodes.Ldc_I4_M1,
            0 => OpCodes.Ldc_I4_0,
            1 => OpCodes.Ldc_I4_1,
            2 => OpCodes.Ldc_I4_2,
            3 => OpCodes.Ldc_I4_3,
            4 => OpCodes.Ldc_I4_4,
            5 => OpCodes.Ldc_I4_5,
            6 => OpCodes.Ldc_I4_6,
            7 => OpCodes.Ldc_I4_7,
            8 => OpCodes.Ldc_I4_8,
            _ => throw new NotImplementedException()
          });

}