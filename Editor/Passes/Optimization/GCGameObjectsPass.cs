/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Components;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Logging;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Passes.Optimization
{
    internal class GCGameObjectsPass : BuildPass
    {
        public override BuildConstraint Constraint => InvokeAtStage(BuildStage.Optimization).Build();

        private readonly HashSet<GameObject> _usefulObjects;

        public GCGameObjectsPass()
        {
            _usefulObjects = new HashSet<GameObject>();
        }

        private void ScanGameObjectReferences(GameObject go)
        {
            var comps = go.GetComponents<Component>();

            foreach (var comp in comps)
            {
                if (comp == null)
                {
                    // missing script
                    continue;
                }

                if (comp is Transform ||
                    comp is DKBaseComponent)
                {
                    continue;
                }

                ScanComponentReferences(comp);
            }
        }

        private void TagRecursivelyUpwards(GameObject go)
        {
            var p = go.transform;
            while (p != null)
            {
                _usefulObjects.Add(p.gameObject);
                p = p.parent;
            }
        }

        private void ScanComponentReferences(Component comp)
        {
            var serializedObj = new SerializedObject(comp);
            var it = serializedObj.GetIterator();

            bool traverseDown = true;
            while (it.Next(traverseDown))
            {
                traverseDown = true;

                if (it.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (it.objectReferenceValue is Component c)
                    {
                        TagRecursivelyUpwards(c.gameObject);
                    }
                    else if (it.objectReferenceValue is GameObject o)
                    {
                        TagRecursivelyUpwards(o);
                    }
                }
                else if (it.propertyType == SerializedPropertyType.String)
                {
                    // disable traverse downwards
                    traverseDown = false;
                    continue;
                }
            }
        }

        private void TraverseGameObjects(GameObject RootGameObject, Func<GameObject, bool> visitFunc)
        {
            var queue = new Queue<GameObject>();
            queue.Enqueue(RootGameObject);

            while (queue.Count > 0)
            {
                var go = queue.Dequeue();

                if (go.CompareTag("EditorOnly"))
                {
                    // stop traversing down
                    TagRecursivelyUpwards(go);
                    continue;
                }

                if (visitFunc(go))
                {
                    for (var i = 0; i < go.transform.childCount; i++)
                    {
                        var child = go.transform.GetChild(i);
                        queue.Enqueue(child.gameObject);
                    }
                }
            }
        }

        private void ScanReferences(GameObject RootGameObject)
        {
            TraverseGameObjects(RootGameObject, go =>
            {
                // do not remove end bones
                if (go.name.ToLower().EndsWith("end"))
                {
                    TagRecursivelyUpwards(go);
                }

                ScanGameObjectReferences(go);
                return true;
            });
        }

        private void CleanGameObjects(Report report, GameObject RootGameObject)
        {
            TraverseGameObjects(RootGameObject, go =>
            {
                if (!_usefulObjects.Contains(go))
                {
                    // for debug purposes
                    report.LogInfo("GCGameObjectsPass", $"GC Destroyed: {AnimationUtils.GetRelativePath(go.transform, RootGameObject.transform)}");
                    Object.DestroyImmediate(go);
                    return false;
                }
                return true;
            });
        }

        public override bool Invoke(Context ctx)
        {
            ScanReferences(ctx.AvatarGameObject);
            CleanGameObjects(ctx.Report, ctx.AvatarGameObject);
            return true;
        }
    }
}
