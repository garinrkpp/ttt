using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Quantum {
  public static class FrameDiffer {
    class ReflectType {
      public FieldInfo[] Fields;

      static Dictionary<Type, ReflectType> lookup = new Dictionary<Type, ReflectType>();
      static HashSet<Type> primitiveTypes = new HashSet<Type>(new[] {
        typeof(FP), typeof(FPVector2), typeof(FPVector3), typeof(FPQuaternion)
      });

      public static ReflectType Create(Type t) {
        if (t.IsPrimitive) {
          return null;
        }

        if (t.IsEnum) {
          return null;
        }

        if (primitiveTypes.Contains(t)) {
          return null;
        }

        ReflectType rt;

        if (lookup.TryGetValue(t, out rt) == false) {
          rt = new ReflectType();
          rt.Fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

          lookup.Add(t, rt);
        }

        return rt;
      }
    }

    static void Calculate(StringBuilder sb, String name, Object value, Int32 depth) {
      var rt = ReflectType.Create(value.GetType());

      if (rt == null) {
        sb.AppendLine(new String(' ', depth * 2) + name + ": " + value.ToString());
      } else {
        sb.AppendLine(new String(' ', depth * 2) + name);

        foreach (var field in rt.Fields) {
          Calculate(sb, field.Name, field.GetValue(value), depth + 1);
        }
      }
    }

    static public String Dump(String name, Object value) {
      var sb = new StringBuilder(1024 * 1024);

      Calculate(sb, name, value, 0);

      return sb.ToString();
    }
  }
}
