using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Mvvm
{
    /// <summary>
    /// Class that can be derived from to create a singleton. the only issue is that the type must have a non-public parameterless constructor.
    /// </summary>
    /// <typeparam name="T">Any type inherited from Singleton&lt;T&gt;></typeparam>
    public abstract class Singleton<T>
        where T : Singleton<T>
    {
        /// <summary>
        /// Lazily created instance variable.
        /// </summary>
        private readonly static Lazy<T> _instance;

        /// <summary>
        /// Lazily created instance property.
        /// </summary>
        public static T Current
        {
            get { return _instance.Value; }
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Singleton()
        {
            _instance = new Lazy<T>(InstanceFactory);
        }

        /// <summary>
        /// Calls a non-public empty constructor on derived type to create instance. If no such constructor exists a TypeAccessException is thrown.
        /// </summary>
        /// <returns></returns>
        private static T InstanceFactory()
        {
            var type = typeof(T);
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (constructors.Length == 1)
            {
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

                // Make sure we found our one and only private or protected constructor.
                if ((ctor != null) && (ctor.IsPrivate || ctor.IsFamily))
                {
                    var instance = ctor.Invoke(new object[] { }) as T;

                    if (instance == null)
                    {
                        throw new TypeInitializationException(type.FullName, new NullReferenceException());
                    }

                    return instance;
                }
                //, new()
                //else
                //{
                //    return new T();
                //}
            }

            throw new TypeInitializationException(type.FullName, new TypeAccessException("Type must contain a single (non-public) constructor if derived from Singleton<T>."));
        }
    }
}
