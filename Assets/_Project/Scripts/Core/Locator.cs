using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class Locator
    {
        static Locator instance;
        static object syncRoot = new();
        
        private static readonly Dictionary<Type, object> _managerMap = new();

        public void Register<T>(T instance, bool overwrite = false) where T : class
        {
            lock (syncRoot)
            {
                if (instance == null)
                {
                    Debug.LogError($"Locator.Register<{typeof(T).Name}>: instance can not be null.");
                    throw new ArgumentNullException(nameof(instance));
                }

                var type = typeof(T);
                if (_managerMap.TryGetValue(type, out var existing))
                {
                    if (!overwrite && !ReferenceEquals(existing, instance))
                    {
                        Debug.LogError($"Locator.Register<{typeof(T).Name}>: Service already registered.");
                    }
                }

                _managerMap[type] = instance;   
            }
        }

        public bool TryResolve<T>(out T service) where T : class
        {
            lock (syncRoot)
            {
                if (_managerMap.TryGetValue(typeof(T), out var obj))
                {
                    service = obj as T;
                    return service != null;
                }

                service = null;
                return false;   
            }
        }

        private static void Clear()
        {
            _managerMap.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void BootstrapOnLoad()
        {
            Clear();
        }
        
        
        public static Locator Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                lock (syncRoot)
                {
                    instance ??= new Locator();
                }

                return instance;
            }
        }
    }
}
