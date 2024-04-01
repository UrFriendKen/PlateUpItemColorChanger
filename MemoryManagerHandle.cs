// Kitchen.MemoryManagerHandle
using UnityEngine;


namespace KitchenItemColorChanger
{
    public readonly struct MemoryManagerHandle
    {
        public readonly int Key;

        public readonly bool ExtendedLogging;

        public MemoryManagerHandle(int key, bool extended_logging = false)
        {
            Key = key;
            ExtendedLogging = extended_logging;
        }

        public T Register<T>(T obj) where T : Object
        {
            if (ExtendedLogging)
            {
                UnityEngine.Debug.LogError($"[MemoryManagementHandle] {Key}: Registering {obj}");
            }
            MemoryManager.Register(this, obj);
            return obj;
        }

        public T Register<T>(T obj, out bool made_assignment) where T : Object
        {
            if (ExtendedLogging)
            {
                UnityEngine.Debug.LogError($"[MemoryManagementHandle] {Key}: Registering {obj}");
            }
            made_assignment = MemoryManager.Register(this, obj);
            return obj;
        }

        public void Dispose()
        {
            if (ExtendedLogging)
            {
                UnityEngine.Debug.LogError($"[MemoryManagementHandle] {Key}: Disposing");
            }
            MemoryManager.Dispose(this);
        }

        public static implicit operator MemoryManagerHandle(Object obj)
        {
            return new MemoryManagerHandle(obj.GetInstanceID());
        }

        public override bool Equals(object obj)
        {
            if (obj is MemoryManagerHandle memoryManagerHandle)
            {
                return memoryManagerHandle.Key == Key;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Key;
        }
    }

}
