using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace CityBuilderCore
{
    /// <summary>
    /// very rudementary dependency management used to decouple implementations<br/>
    /// dependencies get cleared whenever the scene changes<br/>
    /// in this implementaton all registered types are quasi singletons<br/>
    /// could be replaced with something more sophisticated like a tagged or scoped dependencies if necessary<br/>
    /// for the purpose of this framework it has been left as simple as possible for performance
    /// </summary>
    public static class Dependencies
    {
        private static Dictionary<Type, object> _dependencies = new Dictionary<Type, object>();

        static Dependencies()
        {
            SceneManager.sceneUnloaded += (s) => Clear();
        }

        public static void Register<T>(T instance)
        {
            if (_dependencies.ContainsKey(typeof(T)))
                throw new Exception($"Duplicate Dependency {typeof(T).Name}! ({instance})");

            _dependencies.Add(typeof(T), instance);
        }

        public static bool Contains<T>()
        {
            return _dependencies.ContainsKey(typeof(T));
        }

        /// <summary>
        /// returns the dependency of the type, throws if the dependency is not found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>()
        {
            if (!_dependencies.ContainsKey(typeof(T)))
                throw new Exception($"Missing Dependency {typeof(T).Name}!");

            return (T)_dependencies[typeof(T)];
        }

        public static T GetOptional<T>()
        {
            if (!_dependencies.ContainsKey(typeof(T)))
                return default;

            return (T)_dependencies[typeof(T)];
        }

        public static void Clear()
        {
            _dependencies.Clear();
        }
    }
}