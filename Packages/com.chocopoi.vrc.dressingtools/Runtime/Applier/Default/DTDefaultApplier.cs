using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Applier.Default
{
    public class DTDefaultApplier : IDTApplier
    {
        private void ApplyTransforms(DTReport report, DTAvatarConfig avatarConfig, GameObject targetAvatar, GameObject targetWearable, out Transform lastAvatarParent, out Vector3 lastAvatarScale)
        {
            // check position delta and adjust
            {
                var wearableWorldPos = avatarConfig.worldPosition.ToVector3();
                if (targetWearable.transform.position - targetAvatar.transform.position != wearableWorldPos)
                {
                    report.LogInfo(0, "Position delta mismatch, adjusting wearable position: " + wearableWorldPos.ToString());
                    targetWearable.transform.position += wearableWorldPos;
                }
            }

            // check rotation delta and adjust
            {
                var wearableWorldRot = avatarConfig.worldRotation.ToQuaternion();
                if (targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation) != wearableWorldRot)
                {
                    report.LogInfo(0, "Rotation delta mismatch, adjusting wearable rotation: " + wearableWorldRot.ToString());
                    targetWearable.transform.rotation *= wearableWorldRot;
                }
            }

            // apply avatar scale
            lastAvatarParent = targetAvatar.transform.parent;
            lastAvatarScale = Vector3.zero + targetAvatar.transform.localScale;
            if (lastAvatarParent != null)
            {
                // tricky workaround to apply lossy world scale is to unparent
                targetAvatar.transform.SetParent(null);
            }
            targetAvatar.transform.localScale = avatarConfig.avatarLossyScale.ToVector3();

            // apply wearable scale
            targetWearable.transform.localScale = avatarConfig.wearableLossyScale.ToVector3();
        }

        private void RollbackTransform(GameObject targetAvatar, Transform lastAvatarParent, Vector3 lastAvatarScale)
        {
            // restore avatar scale
            if (lastAvatarParent != null)
            {
                targetAvatar.transform.SetParent(lastAvatarParent);
            }
            targetAvatar.transform.localScale = lastAvatarScale;
        }

        private bool GenerateMappings(DTReport report, string avatarArmatureName, DTWearableConfig wearableConfig, GameObject targetAvatar, GameObject targetWearable, out List<DTBoneMapping> boneMappings, out List<DTObjectMapping> objectMappings)
        {
            // execute dresser
            var dresser = DresserRegistry.GetDresserByTypeName(wearableConfig.dresserName);
            var dresserSettings = dresser.DeserializeSettings(wearableConfig.serializedDresserConfig);
            if (dresserSettings == null)
            {
                // fallback to become a empty
                dresserSettings = dresser.NewSettings();
            }
            dresserSettings.targetAvatar = targetAvatar;
            dresserSettings.targetWearable = targetWearable;
            dresserSettings.avatarArmatureName = avatarArmatureName;
            dresserSettings.wearableArmatureName = wearableConfig.wearableArmatureName;
            var dresserReport = dresser.Execute(dresserSettings, out boneMappings, out objectMappings);

            // abort on error
            if (dresserReport.Result != DTReportResult.Compatible && dresserReport.Result != DTReportResult.Ok)
            {
                report.LogError(0, string.Format("Unable to wear \"{0}\" with dresser report errors!", wearableConfig.info.name));
                return false;
            }

            // handle bone overrides
            if (wearableConfig.boneMappingMode == DTWearableMappingMode.Manual)
            {
                boneMappings = new List<DTBoneMapping>(wearableConfig.boneMappings);
            }
            else if (wearableConfig.boneMappingMode == DTWearableMappingMode.Override)
            {
                foreach (var mappingOverride in wearableConfig.boneMappings)
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

            // handle object overrides
            if (wearableConfig.objectMappingMode == DTWearableMappingMode.Manual)
            {
                objectMappings = new List<DTObjectMapping>(wearableConfig.objectMappings);
            }
            else if (wearableConfig.objectMappingMode == DTWearableMappingMode.Override)
            {
                foreach (var mappingOverride in wearableConfig.objectMappings)
                {
                    foreach (var originalMapping in objectMappings)
                    {
                        // override on match
                        if (originalMapping.wearableObjectPath == mappingOverride.wearableObjectPath)
                        {
                            originalMapping.avatarObjectPath = mappingOverride.avatarObjectPath;
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

        private DTObjectMapping GetObjectMappingByWearableBonePath(List<DTObjectMapping> objectMappings, string wearableObjectPath)
        {
            foreach (var mapping in objectMappings)
            {
                if (mapping.wearableObjectPath == wearableObjectPath)
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

        private void ApplyIgnoreTransforms(DTCabinet cabinet, DTWearableConfig wearableConfig, IDynamicsProxy avatarDynamics, Transform avatarDynamicsRoot, Transform wearableDynamicsRoot)
        {
            string name = avatarDynamicsRoot.name + "_DBExcluded";
            var dynBoneChild = avatarDynamicsRoot.Find(name);

            if (dynBoneChild == null)
            {
                var obj = new GameObject(name);
                obj.transform.SetParent(avatarDynamicsRoot);
                dynBoneChild = obj.transform;
            }
            RecordAppliedContainer(cabinet, dynBoneChild);

            // check if it is excluded

            if (avatarDynamics != null && !avatarDynamics.IgnoreTransforms.Contains(dynBoneChild))
            {
                avatarDynamics.IgnoreTransforms.Add(dynBoneChild);
            }

            wearableDynamicsRoot.name = string.Format("{0} ({1})", wearableDynamicsRoot.name, wearableConfig.info.name);
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
                    RecordAppliedObject(wearableConfig, child);

                    // continue to add IgnoreTransform if having matching avatar bone
                    ApplyIgnoreTransforms(cabinet, wearableConfig, avatarDynamics, avatarTrans, child);
                }
            }
        }

        private void RecordAppliedObject(DTWearableConfig wearableConfig, Transform wearableChild)
        {
            // add applied object to cabinet wearable (if any)
            if (wearableConfig is DTCabinetWearable cabinetWearable)
            {
                if (!cabinetWearable.appliedObjects.Contains(wearableChild.gameObject))
                {
                    cabinetWearable.appliedObjects.Add(wearableChild.gameObject);
                }
            }
        }

        private void RecordAppliedContainer(DTCabinet cabinet, Transform wearableContainer)
        {
            // add bone container to cabinet (if any)
            if (cabinet != null)
            {
                if (!cabinet.appliedContainers.Contains(wearableContainer.gameObject))
                {
                    cabinet.appliedContainers.Add(wearableContainer.gameObject);
                }
            }
        }

        public bool ApplyBoneMappings(DTReport report, DTApplierSettings settings, string wearableName, List<IDynamicsProxy> avatarDynamics, List<IDynamicsProxy> wearableDynamics, List<DTBoneMapping> boneMappings, GameObject targetAvatar, GameObject targetWearable)
        {
            return ApplyBoneMappings(report, settings, wearableName, avatarDynamics, wearableDynamics, boneMappings, targetAvatar.transform, targetWearable.transform, targetWearable.transform, "", null);
        }

        private bool ApplyBoneMappings(DTReport report, DTApplierSettings settings, string wearableName, List<IDynamicsProxy> avatarDynamics, List<IDynamicsProxy> wearableDynamics, List<DTBoneMapping> boneMappings, Transform avatarRoot, Transform wearableRoot, Transform wearableBoneParent, string previousPath, DTWearableConfig wearableConfig = null, DTCabinet cabinet = null)
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

                if (settings.removeExistingPrefixSuffix)
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

                        RecordAppliedObject(wearableConfig, wearableChild);

                        if (mapping.mappingType == DTBoneMappingType.MoveToBone)
                        {
                            Transform wearableBoneContainer = null;

                            if (settings.groupBones)
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
                                RecordAppliedContainer(cabinet, wearableBoneContainer);
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
                                report.LogError(0, string.Format("Cannot create ParentConstraint to \"{0}\" because an existing ParentConstraint is already on the wearable bone: {1}", mapping.avatarBonePath, mapping.wearableBonePath));
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

                            ApplyIgnoreTransforms(cabinet, wearableConfig, avatarBoneDynamics, avatarBone, wearableChild);
                        }
                        else if (mapping.mappingType == DTBoneMappingType.CopyDynamics)
                        {
                            if (wearableBoneDynamics != null)
                            {
                                // remove wearable dynamics if exist
                                Object.DestroyImmediate(wearableBoneDynamics.Component);
                            }

                            // copy component using unityeditor internal method (easiest way)

                            UnityEditorInternal.ComponentUtility.CopyComponent(avatarBoneDynamics.Component);
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(wearableChild.gameObject);

                            // set root transform
                            if (avatarBoneDynamics is DynamicBoneProxy)
                            {
                                new DynamicBoneProxy(wearableChild.GetComponent(DynamicBoneProxy.DynamicBoneType))
                                {
                                    RootTransform = wearableChild
                                };
                            }
                            else if (avatarBoneDynamics is PhysBoneProxy)
                            {
                                new PhysBoneProxy(wearableChild.GetComponent(PhysBoneProxy.PhysBoneType))
                                {
                                    RootTransform = wearableChild
                                };
                            }
                        }
                    }
                    else
                    {
                        report.LogError(0, string.Format("Avatar bone path \"{0}\" in mapping not found", mapping.avatarBonePath));
                        return false;
                    }
                }

                ApplyBoneMappings(report, settings, wearableName, avatarDynamics, wearableDynamics, boneMappings, avatarRoot, wearableRoot, wearableChild, previousPath + boneName + "/", wearableConfig, cabinet);
            }

            return true;
        }

        public bool ApplyObjectMappings(DTReport report, DTApplierSettings settings, string wearableName, List<DTObjectMapping> objectMappings, GameObject targetAvatar, GameObject targetWearable)
        {
            return ApplyObjectMappings(report, settings, wearableName, objectMappings, targetAvatar.transform, targetWearable.transform, null);
        }

        private bool ApplyObjectMappings(DTReport report, DTApplierSettings settings, string wearableName, List<DTObjectMapping> objectMappings, Transform avatarBoneParent, Transform wearableBoneParent, DTWearableConfig wearableConfig = null)
        {
            var wearableChilds = new List<Transform>();

            for (int i = 0; i < wearableBoneParent.childCount; i++)
            {
                wearableChilds.Add(wearableBoneParent.GetChild(i));
            }

            // create wearable container
            Transform wearableContainer;
            if (settings.groupRootObjects)
            {
                string name = "DT_" + wearableName;
                wearableContainer = avatarBoneParent.Find(name);

                if (wearableContainer != null)
                {
                    report.LogError(0, "Existing wearable detected! Aborting operation.");
                    return false;
                }

                var obj = new GameObject(name);
                obj.transform.SetParent(avatarBoneParent);
                wearableContainer = obj.transform;

                RecordAppliedObject(wearableConfig, wearableContainer);
            }
            else
            {
                wearableContainer = avatarBoneParent;
            }

            foreach (var wearableChild in wearableChilds)
            {
                var path = AnimationUtils.GetRelativePath(wearableChild, wearableBoneParent);
                var mapping = GetObjectMappingByWearableBonePath(objectMappings, path);

                if (settings.removeExistingPrefixSuffix)
                {
                    RemoveExistingPrefixSuffix(wearableChild);
                }

                if (mapping != null)
                {
                    var avatarObject = mapping.avatarObjectPath == "" || mapping.avatarObjectPath == "." ? avatarBoneParent : avatarBoneParent.Find(mapping.avatarObjectPath);

                    if (avatarObject != null)
                    {
                        wearableChild.SetParent(wearableContainer);
                        RecordAppliedObject(wearableConfig, wearableChild);
                    }
                    else
                    {
                        report.LogError(0, string.Format("Avatar object path \"{0}\" in mapping not found", mapping.avatarObjectPath));
                        return false;
                    }
                }
            }

            return true;
        }

        private void CleanUp(GameObject targetAvatar, Transform lastAvatarParent, Vector3 lastAvatarScale, GameObject wearableObj)
        {
            // rollback parents and scaling
            RollbackTransform(targetAvatar, lastAvatarParent, lastAvatarScale);

            // destroy instantiated object
            Object.DestroyImmediate(wearableObj);
        }

        private bool ApplyWearable(DTReport report, DTApplierSettings settings, DTWearableConfig wearableConfig, List<IDynamicsProxy> avatarDynamics, DTCabinet cabinet = null, GameObject targetAvatar = null, GameObject targetWearable = null)
        {
            // TODO: check config version and do migration here

            if (targetAvatar == null)
            {
                // obtain from cabinet
                targetAvatar = cabinet.avatarGameObject;
            }

            var guid = DTRuntimeUtils.GetGameObjectOriginalPrefabGuid(targetAvatar);
            if (guid == null || guid == "")
            {
                report.LogWarn(0, "Cannot find GUID of avatar, maybe not a prefab? We cannot check the compatibility of the configuration.");
            }
            else if (System.Array.IndexOf(wearableConfig.targetAvatarConfig.guids, guid) == -1)
            {
                report.LogWarn(0, "The configuration does not contain the avatar's GUID! It might not be compatibile with the avatar!");
            }

            if (targetWearable == null)
            {
                // obtain from wearable
                if (wearableConfig is DTCabinetWearable cabinetWearable)
                {
                    targetWearable = cabinetWearable.wearableGameObject;
                }
            }

            // instantiate wearable prefab
            var wearableObj = Object.Instantiate(targetWearable);

            // scan for wearable dynamics
            var wearableDynamics = DTRuntimeUtils.ScanDynamics(wearableObj);

            // apply translation and scaling
            ApplyTransforms(report, wearableConfig.targetAvatarConfig, targetAvatar, wearableObj, out var lastAvatarParent, out var lastAvatarScale);

            if (!GenerateMappings(report, cabinet.avatarArmatureName, wearableConfig, targetAvatar, wearableObj, out var boneMappings, out var objectMappings))
            {
                Debug.Log("Generate mapping error");
                // abort on error
                CleanUp(targetAvatar, lastAvatarParent, lastAvatarScale, wearableObj);
                return false;
            }

            if (wearableConfig.wearableType == DTWearableType.ArmatureBased)
            {
                // apply bone mappings
                if (!ApplyBoneMappings(report, settings, wearableConfig.info.name, avatarDynamics, wearableDynamics, boneMappings, targetAvatar.transform, wearableObj.transform, wearableObj.transform, "", wearableConfig, cabinet))
                {
                    Debug.Log("Bone mapping error");
                    // abort on error
                    CleanUp(targetAvatar, lastAvatarParent, lastAvatarScale, wearableObj);
                    return false;
                }

                // apply object mappings
                if (!ApplyObjectMappings(report, settings, wearableConfig.info.name, objectMappings, targetAvatar.transform, wearableObj.transform, wearableConfig))
                {
                    Debug.Log("Object mapping error");
                    // abort on error
                    CleanUp(targetAvatar, lastAvatarParent, lastAvatarScale, wearableObj);
                    return false;
                }
            }

            CleanUp(targetAvatar, lastAvatarParent, lastAvatarScale, wearableObj);

            return true;
        }

        public DTReport ApplyCabinet(DTApplierSettings settings, DTCabinet cabinet)
        {
            var report = new DTReport();

            // scan for avatar dynamics
            var avatarDynamics = DTRuntimeUtils.ScanDynamics(cabinet.avatarGameObject);

            foreach (var wearableConfig in cabinet.wearables)
            {
                if (!ApplyWearable(report, settings, wearableConfig, avatarDynamics, cabinet))
                {
                    report.LogError(0, "Error applying wearable, aborting: " + wearableConfig.info.name);
                    break;
                }
            }

            return report;
        }

        public DTApplierSettings DeserializeSettings(string serializedJson)
        {
            return JsonConvert.DeserializeObject<DTDefaultApplierSettings>(serializedJson);
        }

        public DTApplierSettings NewSettings()
        {
            return new DTDefaultApplierSettings();
        }
    }
}
