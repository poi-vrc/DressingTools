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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Localization;
using UnityEngine;
using UnityEngine.Animations;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    [ComponentPassFor(typeof(DTObjectMapping))]
    internal class ObjectMappingPass : ComponentPass
    {
        private const string LogLabel = "ObjectMappingPass";
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                // need to let it group first, or the search will be wrong if we move something
                .AfterPass<GroupDynamicsPass>()
                .BeforePass<GroupDynamicsModifyAnimPass>()
                .BeforePass<IgnoreDynamicsPass>()
                .Build();

        private static void RemoveExistingPrefixSuffix(Transform trans)
        {
            // check if there is a prefix
            if (trans.name.StartsWith("("))
            {
                //find the first closing bracket
                int prefixBracketEnd = trans.name.IndexOf(")");
                if (prefixBracketEnd != -1 && prefixBracketEnd != trans.name.Length - 1) //remove it if there is
                {
                    trans.name = trans.name.Substring(prefixBracketEnd + 1).Trim();
                }
            }

            // check if there is a suffix
            if (trans.name.EndsWith(")"))
            {
                //find the first closing bracket
                int suffixBracketStart = trans.name.LastIndexOf("(");
                if (suffixBracketStart != -1 && suffixBracketStart != 0) //remove it if there is
                {
                    trans.name = trans.name.Substring(0, suffixBracketStart).Trim();
                }
            }
        }

        private static void ScanParentConstraints(Transform transform, Dictionary<Transform, HashSet<Component>> foundPc)
        {
            if (transform.TryGetComponent<ParentConstraint>(out var pcComp))
            {
                if (!foundPc.TryGetValue(transform, out var comps))
                {
                    comps = foundPc[transform] = new HashSet<Component>();
                }
                comps.Add(pcComp);
            }

#if DT_VRCSDK3A_3_7_0_OR_LATER
            if (transform.TryGetComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>(out var vrcPcComp))
            {
                var targetTrans = vrcPcComp.TargetTransform == null ? vrcPcComp.transform : vrcPcComp.TargetTransform;
                if (!foundPc.TryGetValue(targetTrans, out var comps))
                {
                    comps = foundPc[targetTrans] = new HashSet<Component>();
                }
                comps.Add(vrcPcComp);
            }
#endif

            foreach (Transform child in transform)
            {
                ScanParentConstraints(child, foundPc);
            }
        }

        private static void AddParentConstraint(Context ctx, Transform sourceTransform, Transform targetTransform, Dictionary<Transform, HashSet<Component>> foundPc)
        {
            if (foundPc.TryGetValue(sourceTransform, out var existingComps))
            {
                ctx.Report.LogWarn(LogLabel, $"Existing ParentConstraint (count {existingComps.Count}) in {sourceTransform.name} detected, destroying it to avoid issues.");
                foreach (var existingComp in existingComps)
                {
                    Object.DestroyImmediate(existingComp);
                }
            }

#if DT_VRCSDK3A_3_7_0_OR_LATER
            var vrcPcComp = sourceTransform.gameObject.AddComponent<VRC.SDK3.Dynamics.Constraint.Components.VRCParentConstraint>();
            vrcPcComp.IsActive = true;
            vrcPcComp.TargetTransform = targetTransform;
            vrcPcComp.Sources.Add(new VRC.Dynamics.VRCConstraintSource
            {
                SourceTransform = targetTransform,
                Weight = 1
            });
#else
            var pcComp = sourceTransform.gameObject.AddComponent<ParentConstraint>();
            pcComp.constraintActive = true;
            var source = new ConstraintSource
            {
                sourceTransform = targetTransform,
                weight = 1
            };
            pcComp.AddSource(source);
#endif
        }

        private static void ApplyObjectMappings(Context ctx, DTObjectMapping objMappingComp)
        {
            var foundPc = new Dictionary<Transform, HashSet<Component>>();
            ScanParentConstraints(ctx.AvatarGameObject.transform, foundPc);

            var pathRemapper = ctx.Feature<PathRemapper>();
            foreach (var mapping in objMappingComp.Mappings)
            {
                if (mapping.SourceTransform == null)
                {
                    ctx.Report.LogWarn(LogLabel, $"Source transform for target path {mapping.TargetPath} is null in {objMappingComp.name}, ignoring");
                    continue;
                }

                RemoveExistingPrefixSuffix(mapping.SourceTransform);

                var targetTransform = string.IsNullOrEmpty(mapping.TargetPath) ? ctx.AvatarGameObject.transform : ctx.AvatarGameObject.transform.Find(mapping.TargetPath);

                if (targetTransform == null)
                {
                    ctx.Report.LogWarn(LogLabel, $"Could not find target transform for target path {mapping.TargetPath} is null in {objMappingComp.name}, ignoring");
                    continue;
                }

                if (mapping.Type == DTObjectMapping.Mapping.MappingType.MoveToBone)
                {
                    Transform objectContainer;
                    if (objMappingComp.GroupObjects)
                    {
                        // group bones in a {boneName}_DT container
                        string name = targetTransform.name + "_DT";
                        objectContainer = targetTransform.Find(name);
                        if (objectContainer == null)
                        {
                            // create container if not exist
                            var obj = new GameObject(name);
                            obj.transform.SetParent(targetTransform);
                            objectContainer = obj.transform;
                        }
                        pathRemapper.TagContainerBone(objectContainer.gameObject);
                    }
                    else
                    {
                        objectContainer = targetTransform;
                    }

                    var newName = objMappingComp.Prefix + mapping.SourceTransform.name + objMappingComp.Suffix;

                    // add guid to suffix if has duplicate names
                    if (objMappingComp.PreventDuplicateNames &&
                        objectContainer.Find(newName) != null)
                    {
                        newName += $"@{Guid.NewGuid()}";
                    }

                    mapping.SourceTransform.name = newName;
                    mapping.SourceTransform.SetParent(objectContainer);
                }
                else if (mapping.Type == DTObjectMapping.Mapping.MappingType.ParentConstraint)
                {
                    AddParentConstraint(ctx, mapping.SourceTransform, targetTransform, foundPc);
                }
                else if (mapping.Type == DTObjectMapping.Mapping.MappingType.IgnoreTransform)
                {
                    // if not, move it to the tree
                    Transform dbExcludedContainer;
                    if (objMappingComp.GroupObjects)
                    {
                        var containerName = $"{targetTransform.name}_DBExcluded";
                        dbExcludedContainer = targetTransform.Find(containerName);

                        if (dbExcludedContainer == null)
                        {
                            var obj = new GameObject(containerName);
                            obj.transform.SetParent(targetTransform);
                            dbExcludedContainer = obj.transform;
                        }

                        // check if it is excluded
                        if (!dbExcludedContainer.TryGetComponent<DTIgnoreDynamics>(out _))
                        {
                            dbExcludedContainer.gameObject.AddComponent<DTIgnoreDynamics>();
                        }
                    }
                    else
                    {
                        dbExcludedContainer = targetTransform;
                        // we won't want to exclude the whole avatar bone
                        mapping.SourceTransform.gameObject.AddComponent<DTIgnoreDynamics>();
                    }

                    var newName = objMappingComp.Prefix + mapping.SourceTransform.name + objMappingComp.Suffix;
                    if (objMappingComp.PreventDuplicateNames &&
                        dbExcludedContainer.Find(newName) != null)
                    {
                        newName += $"@{Guid.NewGuid()}";
                    }

                    mapping.SourceTransform.name = newName;
                    mapping.SourceTransform.SetParent(dbExcludedContainer);
                }
            }
        }

        public override bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents)
        {
            generatedComponents = new List<DTBaseComponent>();
            ApplyObjectMappings(ctx, (DTObjectMapping)component);
            return true;
        }
    }
}
