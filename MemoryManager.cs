using System.Collections.Generic;
using UnityEngine;


namespace KitchenItemColorChanger
{
    public static class MemoryManager
    {
        private static Dictionary<MemoryManagerHandle, HashSet<Object>> ObjectReferences = new Dictionary<MemoryManagerHandle, HashSet<Object>>();

        private static Stack<HashSet<Object>> HashSetPool = new Stack<HashSet<Object>>();

        public static MemoryManagerHandle Handle(Object obj)
        {
            return obj;
        }

        private static HashSet<Object> GetObjects(MemoryManagerHandle handle, bool do_not_create = false)
        {
            if (ObjectReferences.TryGetValue(handle, out var value))
            {
                return value;
            }
            if (do_not_create)
            {
                return null;
            }
            value = ((HashSetPool.Count <= 0) ? new HashSet<Object>() : HashSetPool.Pop());
            ObjectReferences[handle] = value;
            return value;
        }

        private static void ClearHandle(MemoryManagerHandle handle)
        {
            if (ObjectReferences.TryGetValue(handle, out var value))
            {
                value.Clear();
                HashSetPool.Push(value);
                ObjectReferences.Remove(handle);
            }
        }

        public static bool Register(MemoryManagerHandle handle, Object obj)
        {
            return GetObjects(handle).Add(obj);
        }

        public static void Dispose(MemoryManagerHandle handle)
        {
            HashSet<Object> objects = GetObjects(handle, do_not_create: true);
            if (objects == null)
            {
                return;
            }
            foreach (Object item in objects)
            {
                Object.Destroy(item);
            }
            ClearHandle(handle);
        }
    }

}
