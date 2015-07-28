using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CsFormAnalyzer.Utils
{
    public static class ReflectionHelper
    {
        public static T CreateNewObjectFactory<T>(Type type)
        {
            var ci = type.GetConstructor(Type.EmptyTypes);
            if (ci == null)
            {
                throw new ArgumentException(type.Name + " has no Default Constructor");
            }
            var dm = new DynamicMethod("ViewFactory", type, null);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Newobj, ci);
            il.Emit(OpCodes.Ret);
            var ret = (Func<T>)dm.CreateDelegate(typeof(Func<T>));
            return ret();
        }

        #region ClearEventInvocations

        public static void ClearEventInvocations(object obj, string eventName)
        {
            var fi = GetEventField(obj.GetType(), eventName);
            if (fi == null) return;
            fi.SetValue(obj, null);
        }

        private static FieldInfo GetEventField(Type type, string eventName)
        {
            FieldInfo field = null;
            while (type != null)
            {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                    break;

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    break;
                type = type.BaseType;
            }
            return field;
        }

        #endregion

        public static IEnumerable<Type> GetTypesByAsmPath(string path)
        {   
            var asm = Assembly.UnsafeLoadFrom(path);            
            //var asm = Assembly.ReflectionOnlyLoadFrom(path);
            try
            {
                var types = asm.GetTypes();
                return types;
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
                return null;
            }
        }

    }
}
