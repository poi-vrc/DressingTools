using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Wearable;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DTEditorUtils
    {
        //Reference: https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3416790
        public static void DrawHorizontalLine(int height = 1)
        {
            EditorGUILayout.Separator();
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Separator();
        }

        public static void ReadOnlyTextField(string label, string text)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                EditorGUILayout.SelectableLabel(text, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndHorizontal();
        }

        public static DTCabinet[] GetAllCabinets()
        {
            return Object.FindObjectsOfType<DTCabinet>();
        }

        public static string GetGameObjectOriginalPrefabGuid(GameObject obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj));
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            return guid;
        }

        public static DTCabinet GetAvatarCabinet(GameObject avatar, bool createIfNotExists = false)
        {
            if (avatar == null)
            {
                return null;
            }

            var comp = avatar.GetComponent<DTCabinet>();

            if (comp == null && createIfNotExists)
            {
                // create new cabinet if not exist
                comp = avatar.AddComponent<DTCabinet>();

                // TODO: read default config, scan for armature names?
                comp.avatarGameObject = avatar;
                comp.avatarArmatureName = "Armature";
            }

            return comp;
        }

        public static void AddCabinetWearable(DTCabinet cabinet, DTWearableConfig config, GameObject wearableGameObject)
        {
            if (PrefabUtility.IsPartOfAnyPrefab(wearableGameObject) && PrefabUtility.GetPrefabInstanceStatus(wearableGameObject) == PrefabInstanceStatus.NotAPrefab)
            {
                // if not in scene, we instantiate it with a prefab connection
                wearableGameObject = (GameObject)PrefabUtility.InstantiatePrefab(wearableGameObject);
            }

            // parent to avatar
            wearableGameObject.transform.SetParent(cabinet.transform);

            if (wearableGameObject.GetComponent<DTCabinetWearable>() == null)
            {
                // add cabinet wearable component
                var cabinetWearable = wearableGameObject.AddComponent<DTCabinetWearable>();

                cabinetWearable.wearableGameObject = wearableGameObject;
                cabinetWearable.configJson = config.Serialize();
            }
        }

        public static void RemoveCabinetWearable(DTCabinet cabinet, DTCabinetWearable wearable)
        {
            var cabinetWearables = cabinet.avatarGameObject.GetComponentsInChildren<DTCabinetWearable>();
            foreach (var cabinetWearable in cabinetWearables)
            {
                if (cabinetWearable == wearable)
                {
                    if (!PrefabUtility.IsOutermostPrefabInstanceRoot(cabinetWearable.gameObject))
                    {
                        Debug.Log("Prefab is not outermost. Aborting");
                        return;
                    }
                    Object.DestroyImmediate(cabinetWearable.gameObject);
                    break;
                }
            }
        }

        public static void PrepareWearableConfig(DTWearableConfig config, GameObject targetAvatar, GameObject targetWearable)
        {
            config.configVersion = DTWearableConfig.CurrentConfigVersion;

            AddWearableMetaInfo(config, targetWearable);
            AddWearableTargetAvatarConfig(config, targetAvatar, targetWearable);
        }

        public static void AddWearableTargetAvatarConfig(DTWearableConfig config, GameObject targetAvatar, GameObject targetWearable)
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(targetAvatar);

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                config.targetAvatarConfig.armatureName = "";
            }
            else
            {
                config.targetAvatarConfig.armatureName = cabinet.avatarArmatureName;
            }

            // can't do anything
            if (targetAvatar == null || targetWearable == null)
            {
                return;
            }

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(targetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            config.targetAvatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                config.targetAvatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = targetWearable.transform.position - targetAvatar.transform.position;
            var deltaRotation = targetWearable.transform.rotation * Quaternion.Inverse(targetAvatar.transform.rotation);
            config.targetAvatarConfig.worldPosition = new DTAvatarConfigVector3(deltaPos);
            config.targetAvatarConfig.worldRotation = new DTAvatarConfigQuaternion(deltaRotation);
            config.targetAvatarConfig.avatarLossyScale = new DTAvatarConfigVector3(targetAvatar.transform.lossyScale);
            config.targetAvatarConfig.wearableLossyScale = new DTAvatarConfigVector3(targetWearable.transform.lossyScale);
        }

        public static void AddWearableMetaInfo(DTWearableConfig config, GameObject targetWearable)
        {
            if (targetWearable == null)
            {
                return;
            }

            config.info.name = targetWearable.name;
            config.info.author = "";
            config.info.description = "";
        }
    }
}
