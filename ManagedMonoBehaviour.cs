using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KitchenItemColorChanger
{
    public class ManagedMonoBehaviour : MonoBehaviour
    {
        private MemoryManagerHandle MemoryManagerHandle => this;

        protected T RegisterDisposable<T>(T d) where T : UnityEngine.Object
        {
            MemoryManagerHandle.Register(d, out var _);
            return d;
        }

        protected virtual void OnDestroy()
        {
            DisposeDisposables();
        }

        protected void DisposeDisposables()
        {
            MemoryManagerHandle.Dispose();
        }
    }
}
