using Chocopoi.DressingTools.Cabinet;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DTRuntimeUtils
    {
        public static string GetGameObjectOriginalPrefabGuid(GameObject obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj));
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            return guid;
        }

        public static DTAvatarConfig FindAvatarConfigByGuid(DTAvatarConfig[] configs, string guid)
        {
            foreach (var avatarConfig in configs)
            {
                if (avatarConfig.guid == guid)
                {
                    return avatarConfig;
                }
            }
            return null;
        }
    }
}
