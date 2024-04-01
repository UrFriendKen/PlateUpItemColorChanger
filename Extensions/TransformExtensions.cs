using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KitchenItemColorChanger.Extensions
{
    internal static class TransformExtensions
    {
        public static string GetPath(this Transform transform, Transform stopAtParent = null, bool includeStopAt = false)
        {
            if (transform == null || transform == stopAtParent)
                return transform?.name;

            Transform currentParent = transform;
            Stack<string> pathStack = new Stack<string>();
            pathStack.Push(transform.name);

            HashSet<string> seenChildNames = new HashSet<string>();
            do
            {
                Transform targetChild = currentParent;
                string targetChildName = targetChild.name;

                currentParent = targetChild.parent;
                if (currentParent != null)
                {
                    int skipCount = 0;
                    seenChildNames.Clear();
                    for (int i = 0; i < currentParent.childCount; i++)
                    {
                        Transform child = currentParent.GetChild(i);
                        if (child == targetChild)
                            break;

                        if (child.name == targetChildName)
                            skipCount++;

                        seenChildNames.Add(child.name);
                    }

                    if (skipCount > 0)
                    {
                        string previousChildName = pathStack.Pop();
                        previousChildName += $".{skipCount:000}";
                        pathStack.Push(previousChildName);
                    }
                    pathStack.Push(currentParent.name);
                }
            }
            while (currentParent != null && currentParent != stopAtParent);

            if (stopAtParent != null &&
                currentParent == stopAtParent &&
                !includeStopAt)
            {
                pathStack.Pop();
            }

            string path = string.Join("/", pathStack);
            return path;
        }
    }
}
