/*
 * File: ArmatureMappingModule.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 12:21:25 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Modules.Providers;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    [System.Serializable]
    internal enum BoneMappingMode
    {
        Auto = 0,
        Override = 1,
        Manual = 2
    }

    internal class ArmatureMappingModuleConfig : IModuleConfig
    {
        public string dresserName;

        public string wearableArmatureName;

        public BoneMappingMode boneMappingMode;

        public List<BoneMapping> boneMappings;

        public string serializedDresserConfig;

        public bool removeExistingPrefixSuffix;

        public bool groupBones;

        public ArmatureMappingModuleConfig()
        {
            dresserName = null;
            wearableArmatureName = null;
            boneMappingMode = BoneMappingMode.Auto;
            boneMappings = null;
            serializedDresserConfig = "{}";
            removeExistingPrefixSuffix = true;
            groupBones = true;
        }
    }

    [InitializeOnLoad]
    internal class ArmatureMappingModuleProvider : ModuleProviderBase
    {
        public const string Identifier = "com.chocopoi.dressingtools.built-in.armature-mapping";
        public static class MessageCode
        {
            // Error
            public const string DresserHasErrors = "appliers.default.msgCode.error.dresserHasErrors";
            public const string CannotCreateParentConstraintWithExisting = "appliers.default.msgCode.error.cannotCreateParentConstraintWithExisting";
            public const string AvatarBonePathNotFound = "appliers.default.msgCode.error.avatarBonePathNotFound";
            public const string MappingGenerationHasErrors = "appliers.default.msgCode.error.mappingGenerationHasErrors";
            public const string ApplyingBoneMappingHasErrors = "appliers.default.msgCode.error.applyingBoneMappingHasErrors";
        }

        private static readonly System.Random Random = new System.Random();
        private const string LogLabel = "ArmatureModule";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => Identifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Armature Mapping";
        [ExcludeFromCodeCoverage] public override int ApplyOrder => 2;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        private static bool GenerateMappings(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ArmatureMappingModuleConfig moduleConfig, string wearableName, out List<BoneMapping> resultantBoneMappings)
        {
            // execute dresser
            var dresser = DresserRegistry.GetDresserByTypeName(moduleConfig.dresserName);
            var dresserSettings = dresser.DeserializeSettings(moduleConfig.serializedDresserConfig);
            if (dresserSettings == null)
            {
                // fallback to become a empty
                dresserSettings = dresser.NewSettings();
            }
            dresserSettings.targetAvatar = cabCtx.cabinet.AvatarGameObject;
            dresserSettings.targetWearable = wearCtx.wearableGameObject;
            dresserSettings.avatarArmatureName = cabCtx.cabinet.AvatarArmatureName;
            dresserSettings.wearableArmatureName = moduleConfig.wearableArmatureName;

            var dresserReport = dresser.Execute(dresserSettings, out resultantBoneMappings);
            cabCtx.report.AppendReport(dresserReport);

            // abort on error
            if (dresserReport.HasLogType(DTReportLogType.Error))
            {
                DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.DresserHasErrors, wearableName);
                return false;
            }

            // handle bone overrides
            if (moduleConfig.boneMappingMode == BoneMappingMode.Manual)
            {
                resultantBoneMappings = new List<BoneMapping>(moduleConfig.boneMappings);
            }
            else if (moduleConfig.boneMappingMode == BoneMappingMode.Override)
            {
                DTRuntimeUtils.HandleBoneMappingOverrides(resultantBoneMappings, moduleConfig.boneMappings);
            }

            return true;
        }

        private static BoneMapping GetBoneMappingByWearableBonePath(List<BoneMapping> boneMappings, string wearableBonePath)
        {
            foreach (var mapping in boneMappings)
            {
                if (mapping.wearableBonePath == wearableBonePath)
                {
                    return mapping;
                }
            }
            return null;
        }

        private static void RemoveExistingPrefixSuffix(Transform wearableChild)
        {
            // check if there is a prefix
            if (wearableChild.name.StartsWith("("))
            {
                //find the first closing bracket
                int prefixBracketEnd = wearableChild.name.IndexOf(")");
                if (prefixBracketEnd != -1 && prefixBracketEnd != wearableChild.name.Length - 1) //remove it if there is
                {
                    wearableChild.name = wearableChild.name.Substring(prefixBracketEnd + 1).Trim();
                }
            }

            // check if there is a suffix
            if (wearableChild.name.EndsWith(")"))
            {
                //find the first closing bracket
                int suffixBracketStart = wearableChild.name.LastIndexOf("(");
                if (suffixBracketStart != -1 && suffixBracketStart != 0) //remove it if there is
                {
                    wearableChild.name = wearableChild.name.Substring(0, suffixBracketStart).Trim();
                }
            }
        }

        private static void ApplyIgnoreTransforms(string wearableName, IDynamicsProxy avatarDynamics, Transform avatarDynamicsRoot, Transform wearableDynamicsRoot)
        {
            string name = avatarDynamicsRoot.name + "_DBExcluded";
            var dynBoneChild = avatarDynamicsRoot.Find(name);

            if (dynBoneChild == null)
            {
                var obj = new GameObject(name);
                obj.transform.SetParent(avatarDynamicsRoot);
                dynBoneChild = obj.transform;
            }

            // check if it is excluded

            if (avatarDynamics != null && !avatarDynamics.IgnoreTransforms.Contains(dynBoneChild))
            {
                avatarDynamics.IgnoreTransforms.Add(dynBoneChild);
            }

            wearableDynamicsRoot.name = string.Format("{0} ({1})", wearableDynamicsRoot.name, wearableName);
            wearableDynamicsRoot.SetParent(dynBoneChild);

            // scan for other child bones

            var childs = new List<Transform>();

            for (int i = 0; i < wearableDynamicsRoot.childCount; i++)
            {
                childs.Add(wearableDynamicsRoot.GetChild(i));
            }

            foreach (var child in childs)
            {
                var avatarTrans = avatarDynamicsRoot.Find(child.name);
                if (avatarTrans != null)
                {
                    // continue to add IgnoreTransform if having matching avatar bone
                    ApplyIgnoreTransforms(wearableName, avatarDynamics, avatarTrans, child);
                }
            }
        }

        private static bool ApplyBoneMappings(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ArmatureMappingModuleConfig moduleConfig, string wearableName, List<BoneMapping> boneMappings, Transform avatarRoot, Transform wearableRoot, Transform wearableBoneParent, string previousPath)
        {
            var wearableChilds = new List<Transform>();

            for (int i = 0; i < wearableBoneParent.childCount; i++)
            {
                wearableChilds.Add(wearableBoneParent.GetChild(i));
            }

            foreach (var wearableChild in wearableChilds)
            {
                // we have to backup the boneName here for constructing a path later
                var boneName = wearableChild.name;
                var path = previousPath + boneName;
                var mapping = GetBoneMappingByWearableBonePath(boneMappings, path);

                if (moduleConfig.removeExistingPrefixSuffix)
                {
                    RemoveExistingPrefixSuffix(wearableChild);
                }

                if (mapping != null)
                {
                    var avatarBone = avatarRoot.Find(mapping.avatarBonePath);

                    if (avatarBone != null)
                    {
                        var avatarBoneDynamics = DTRuntimeUtils.FindDynamicsWithRoot(cabCtx.avatarDynamics, avatarBone);
                        var wearableBoneDynamics = DTRuntimeUtils.FindDynamicsWithRoot(wearCtx.wearableDynamics, wearableChild);

                        if (mapping.mappingType == BoneMappingType.MoveToBone)
                        {
                            Transform wearableBoneContainer = null;

                            if (moduleConfig.groupBones)
                            {
                                // group bones in a {boneName}_DT container
                                string name = avatarBone.name + "_DT";
                                wearableBoneContainer = avatarBone.Find(name);
                                if (wearableBoneContainer == null)
                                {
                                    // create container if not exist
                                    var obj = new GameObject(name);
                                    obj.transform.SetParent(avatarBone);
                                    wearableBoneContainer = obj.transform;
                                }
                            }
                            else
                            {
                                wearableBoneContainer = avatarBone;
                            }

                            wearableChild.SetParent(wearableBoneContainer);
                            // TODO: handle custom prefixes?
                            wearableChild.name = string.Format("{0} ({1})", wearableChild.name, wearableName);
                        }
                        else if (mapping.mappingType == BoneMappingType.ParentConstraint)
                        {
                            if (wearableBoneDynamics != null)
                            {
                                // remove wearable dynamics if exist
                                Object.DestroyImmediate(wearableBoneDynamics.Component);
                                wearCtx.wearableDynamics.Remove(wearableBoneDynamics);
                            }

                            // add parent constraint

                            var comp = wearableChild.gameObject.AddComponent<ParentConstraint>();

                            if (comp != null)
                            {
                                comp.constraintActive = true;

                                var source = new ConstraintSource
                                {
                                    sourceTransform = avatarBone,
                                    weight = 1
                                };
                                comp.AddSource(source);
                            }
                            else
                            {
                                DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.CannotCreateParentConstraintWithExisting, mapping.avatarBonePath, mapping.wearableBonePath);
                                return false;
                            }
                        }
                        else if (mapping.mappingType == BoneMappingType.IgnoreTransform)
                        {
                            if (wearableBoneDynamics != null)
                            {
                                // remove wearable dynamics if exist
                                Object.DestroyImmediate(wearableBoneDynamics.Component);
                                wearCtx.wearableDynamics.Remove(wearableBoneDynamics);
                            }

                            ApplyIgnoreTransforms(wearableName, avatarBoneDynamics, avatarBone, wearableChild);
                        }
                        else if (mapping.mappingType == BoneMappingType.CopyDynamics)
                        {
                            if (wearableBoneDynamics != null)
                            {
                                // remove wearable dynamics if exist
                                Object.DestroyImmediate(wearableBoneDynamics.Component);
                                wearCtx.wearableDynamics.Remove(wearableBoneDynamics);
                            }

                            // copy component with reflection
                            var comp = DTRuntimeUtils.CopyComponent(avatarBoneDynamics.Component, wearableChild.gameObject);

                            // set root transform
                            if (avatarBoneDynamics is DynamicBoneProxy)
                            {
                                new DynamicBoneProxy(comp)
                                {
                                    RootTransform = wearableChild
                                };
                            }
                            else if (avatarBoneDynamics is PhysBoneProxy)
                            {
                                new PhysBoneProxy(comp)
                                {
                                    RootTransform = wearableChild
                                };
                            }
                        }
                    }
                    else
                    {
                        DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.AvatarBonePathNotFound, mapping.avatarBonePath);
                        return false;
                    }
                }

                ApplyBoneMappings(cabCtx, wearCtx, moduleConfig, wearableName, boneMappings, avatarRoot, wearableRoot, wearableChild, previousPath + boneName + "/");
            }

            return true;
        }

        private static string RandomString(int length)
        {
            // i just copied from stackoverflow :D
            // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings?page=1&tab=scoredesc#tab-top
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public override bool OnApplyWearable(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, WearableModule module)
        {
            var armatureMappingConfig = (ArmatureMappingModuleConfig)module.config;

            if (!GenerateMappings(cabCtx, wearCtx, armatureMappingConfig, wearCtx.config.Info.name, out var boneMappings))
            {
                DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.MappingGenerationHasErrors);
                return false;
            }

            var generatedName = string.Format("{0}-{1}", wearCtx.config.Info.name, RandomString(32));

            if (!ApplyBoneMappings(cabCtx, wearCtx, armatureMappingConfig, generatedName, boneMappings, cabCtx.cabinet.AvatarGameObject.transform, wearCtx.wearableGameObject.transform, wearCtx.wearableGameObject.transform, ""))
            {
                DTReportUtils.LogErrorLocalized(cabCtx.report, LogLabel, MessageCode.ApplyingBoneMappingHasErrors);
                return false;
            }

            return true;
        }

        static ArmatureMappingModuleProvider()
        {
            ModuleProviderLocator.Instance.Register(new ArmatureMappingModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<ArmatureMappingModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new ArmatureMappingModuleConfig();
    }
}
