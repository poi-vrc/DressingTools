using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    [System.Serializable]
    public enum DTBoneMappingMode
    {
        Auto = 0,
        Override = 1,
        Manual = 2
    }

    public class ArmatureMappingModule : DTWearableModuleBase
    {
        public static class MessageCode
        {
            // Error
            public const string DresserHasErrors = "appliers.default.msgCode.error.dresserHasErrors";
            public const string CannotCreateParentConstraintWithExisting = "appliers.default.msgCode.error.cannotCreateParentConstraintWithExisting";
            public const string AvatarBonePathNotFound = "appliers.default.msgCode.error.avatarBonePathNotFound";
            public const string MappingGenerationHasErrors = "appliers.default.msgCode.error.mappingGenerationHasErrors";
            public const string ApplyingBoneMappingHasErrors = "appliers.default.msgCode.error.applyingBoneMappingHasErrors";
        }

        private const string LogLabel = "ArmatureModule";

        public override int ApplyOrder => 2;

        public override bool AllowMultiple => false;

        public string dresserName;

        public string wearableArmatureName;

        public DTBoneMappingMode boneMappingMode;

        public DTBoneMapping[] boneMappings;

        public string serializedDresserConfig;

        public bool removeExistingPrefixSuffix;

        public bool groupBones;

        public ArmatureMappingModule()
        {
            dresserName = null;
            wearableArmatureName = null;
            boneMappingMode = DTBoneMappingMode.Auto;
            boneMappings = null;
            serializedDresserConfig = "{}";
            removeExistingPrefixSuffix = true;
            groupBones = true;
        }

        private bool GenerateMappings(DTReport report, string avatarArmatureName, string wearableName, GameObject targetAvatar, GameObject targetWearable, out List<DTBoneMapping> resultantBoneMappings)
        {
            // execute dresser
            var dresser = DresserRegistry.GetDresserByTypeName(dresserName);
            var dresserSettings = dresser.DeserializeSettings(serializedDresserConfig);
            if (dresserSettings == null)
            {
                // fallback to become a empty
                dresserSettings = dresser.NewSettings();
            }
            dresserSettings.targetAvatar = targetAvatar;
            dresserSettings.targetWearable = targetWearable;
            dresserSettings.avatarArmatureName = avatarArmatureName;
            dresserSettings.wearableArmatureName = wearableArmatureName;

            var dresserReport = dresser.Execute(dresserSettings, out resultantBoneMappings);
            report.AppendReport(dresserReport);

            // abort on error
            if (dresserReport.HasLogType(DTReportLogType.Error))
            {
                report.LogErrorLocalized(LogLabel, MessageCode.DresserHasErrors, wearableName);
                return false;
            }

            // handle bone overrides
            if (boneMappingMode == DTBoneMappingMode.Manual)
            {
                resultantBoneMappings = new List<DTBoneMapping>(boneMappings);
            }
            else if (boneMappingMode == DTBoneMappingMode.Override)
            {
                foreach (var mappingOverride in boneMappings)
                {
                    foreach (var originalMapping in boneMappings)
                    {
                        // override on match
                        if (originalMapping.avatarBonePath == mappingOverride.avatarBonePath && originalMapping.wearableBonePath == mappingOverride.wearableBonePath)
                        {
                            originalMapping.mappingType = mappingOverride.mappingType;
                        }
                    }
                }
            }

            return true;
        }

        private DTBoneMapping GetBoneMappingByWearableBonePath(List<DTBoneMapping> boneMappings, string wearableBonePath)
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

        private void RemoveExistingPrefixSuffix(Transform wearableChild)
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

        private void ApplyIgnoreTransforms(string wearableName, IDynamicsProxy avatarDynamics, Transform avatarDynamicsRoot, Transform wearableDynamicsRoot)
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

        private bool ApplyBoneMappings(DTReport report, string wearableName, List<IDynamicsProxy> avatarDynamics, List<IDynamicsProxy> wearableDynamics, List<DTBoneMapping> boneMappings, Transform avatarRoot, Transform wearableRoot, Transform wearableBoneParent, string previousPath)
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

                if (removeExistingPrefixSuffix)
                {
                    RemoveExistingPrefixSuffix(wearableChild);
                }

                if (mapping != null)
                {
                    var avatarBone = avatarRoot.Find(mapping.avatarBonePath);

                    if (avatarBone != null)
                    {
                        var avatarBoneDynamics = DTRuntimeUtils.FindDynamicsWithRoot(avatarDynamics, avatarBone);
                        var wearableBoneDynamics = DTRuntimeUtils.FindDynamicsWithRoot(wearableDynamics, wearableChild);

                        if (mapping.mappingType == DTBoneMappingType.MoveToBone)
                        {
                            Transform wearableBoneContainer = null;

                            if (groupBones)
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
                        else if (mapping.mappingType == DTBoneMappingType.ParentConstraint)
                        {
                            if (wearableBoneDynamics != null)
                            {
                                // remove wearable dynamics if exist
                                Object.DestroyImmediate(wearableBoneDynamics.Component);
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
                                report.LogErrorLocalized(LogLabel, MessageCode.CannotCreateParentConstraintWithExisting, mapping.avatarBonePath, mapping.wearableBonePath);
                                return false;
                            }
                        }
                        else if (mapping.mappingType == DTBoneMappingType.IgnoreTransform)
                        {
                            if (wearableBoneDynamics != null)
                            {
                                // remove wearable dynamics if exist
                                Object.DestroyImmediate(wearableBoneDynamics.Component);
                            }

                            ApplyIgnoreTransforms(wearableName, avatarBoneDynamics, avatarBone, wearableChild);
                        }
                        else if (mapping.mappingType == DTBoneMappingType.CopyDynamics)
                        {
                            if (wearableBoneDynamics != null)
                            {
                                // remove wearable dynamics if exist
                                Object.DestroyImmediate(wearableBoneDynamics.Component);
                            }

                            // TODO: copy using reflection
                            throw new System.NotImplementedException();

                            // copy component using unityeditor internal method (easiest way)
                            //UnityEditorInternal.ComponentUtility.CopyComponent(avatarBoneDynamics.Component);
                            //UnityEditorInternal.ComponentUtility.PasteComponentAsNew(wearableChild.gameObject);

                            //// set root transform
                            //if (avatarBoneDynamics is DynamicBoneProxy)
                            //{
                            //    new DynamicBoneProxy(wearableChild.GetComponent(DynamicBoneProxy.DynamicBoneType))
                            //    {
                            //        RootTransform = wearableChild
                            //    };
                            //}
                            //else if (avatarBoneDynamics is PhysBoneProxy)
                            //{
                            //    new PhysBoneProxy(wearableChild.GetComponent(PhysBoneProxy.PhysBoneType))
                            //    {
                            //        RootTransform = wearableChild
                            //    };
                            //}
                        }
                    }
                    else
                    {
                        report.LogErrorLocalized(LogLabel, MessageCode.AvatarBonePathNotFound, mapping.avatarBonePath);
                        return false;
                    }
                }

                ApplyBoneMappings(report, wearableName, avatarDynamics, wearableDynamics, boneMappings, avatarRoot, wearableRoot, wearableChild, previousPath + boneName + "/");
            }

            return true;
        }

        public override bool Apply(DTReport report, DTCabinet cabinet, List<IDynamicsProxy> avatarDynamics, DTWearableConfig config, GameObject wearableGameObject)
        {
            // scan for wearable dynamics
            var wearableDynamics = DTRuntimeUtils.ScanDynamics(wearableGameObject);

            if (!GenerateMappings(report, cabinet.avatarArmatureName, config.info.name, cabinet.avatarGameObject, wearableGameObject, out var boneMappings))
            {
                report.LogErrorLocalized(LogLabel, MessageCode.MappingGenerationHasErrors);
                return false;
            }

            if (!ApplyBoneMappings(report, config.info.name, avatarDynamics, wearableDynamics, boneMappings, cabinet.avatarGameObject.transform, wearableGameObject.transform, wearableGameObject.transform, ""))
            {
                report.LogErrorLocalized(LogLabel, MessageCode.ApplyingBoneMappingHasErrors);
                return false;
            }

            return true;
        }
    }
}
