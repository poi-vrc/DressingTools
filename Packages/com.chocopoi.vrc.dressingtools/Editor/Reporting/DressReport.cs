using System.Collections.Generic;
using Chocopoi.DressingTools.Debugging;
using Chocopoi.DressingTools.Hooks;
using Chocopoi.DressingTools.Proxy;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Reporting
{
    public class DressReport
    {
        private static readonly IDressHook[] hooks = new IDressHook[]
        {
            new NotAPrefabHook(),
            new NoMissingScriptsHook(),
            new ObjectPlacementHook(),
            new ExistingPrefixSuffixHook(),
            new FindAvatarDynamicsHook(),
            new ArmatureHook(),
            new GroupRootObjectsHook(),
            new FindClothesDynamicsHook(),
            new GroupClothesDynamicsHook()
        };

        private static AnimatorController testModeAnimationController;

        public DressCheckResult result;

        public DressCheckCodeMask.Info infos;

        public DressCheckCodeMask.Warn warnings;

        public DressCheckCodeMask.Error errors;

        // stores the gameobjects/dynamicbones detected during the check

        public List<IDynamicsProxy> avatarDynamics;

        public List<GameObject> clothesAllObjects;

        public List<IDynamicsProxy> clothesOriginalDynamics;

        public List<IDynamicsProxy> clothesDynamics;

        public List<GameObject> clothesMeshDataObjects;

        public DebugDump.DressReportDumpJson dressReportDump;

        private DressReport()
        {
            testModeAnimationController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/chocopoi/DressingTools/Animations/TestModeAnimationController.controller");

            if (testModeAnimationController == null)
            {
                Debug.LogError("[DressingTools] Could not load \"TestModeAnimationController\" from \"Assets/chocopoi/DressingTools/Animations\". Did you move it to another location?");
            }

            avatarDynamics = new List<IDynamicsProxy>();
            clothesDynamics = new List<IDynamicsProxy>();
            clothesOriginalDynamics = new List<IDynamicsProxy>();
            clothesAllObjects = new List<GameObject>();
            clothesMeshDataObjects = new List<GameObject>();
        }

        public static void CleanUp()
        {
            var allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.StartsWith("DressingToolsPreview_"))
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        public static DressReport GenerateReport(DressSettings settings)
        {
            return Execute(settings, false);
        }

        public static DressReport Execute(DressSettings settings, bool write)
        {
            CleanUp();

            var report = new DressReport();

            if (settings.activeAvatar == null || settings.clothesToDress == null)
            {
                report.result = DressCheckResult.INVALID_SETTINGS;
                report.errors |= DressCheckCodeMask.Error.NULL_ACTIVE_AVATAR_OR_CLOTHES;
                return report;
            }

            // generate dump of original first

            report.dressReportDump = new DebugDump.DressReportDumpJson
            {
                present = true,
                original_avatar_tree_csv = DebugDump.GenerateGameObjectTreeCsv(settings.activeAvatar.transform, DebugDump.GameObjectCsvHeader),
                original_clothes_tree_csv = DebugDump.GenerateGameObjectTreeCsv(settings.clothesToDress.transform, DebugDump.GameObjectCsvHeader)
            };

            var avatarNewName = "DressingToolsPreview_" + settings.activeAvatar.gameObject.name;
            var clothesNewName = "DressingToolsPreview_" + settings.clothesToDress.name;

            GameObject targetAvatar;
            GameObject targetClothes;

            if (!write)
            {
                targetAvatar = Object.Instantiate(settings.activeAvatar.gameObject);
                targetAvatar.name = avatarNewName;

                var newAvatarPosition = targetAvatar.transform.position;
                newAvatarPosition.x -= 20;
                targetAvatar.transform.position = newAvatarPosition;

                targetClothes = Object.Instantiate(settings.clothesToDress);
                targetClothes.name = clothesNewName;

                var newClothesPosition = targetClothes.transform.position;
                newClothesPosition.x -= 20;
                targetClothes.transform.position = newClothesPosition;

                var animator = targetAvatar.GetComponent<Animator>();

                //add animation controller
                if (animator != null)
                {
                    animator.runtimeAnimatorController = testModeAnimationController;
                }

                //add dummy focus sceneview script
                targetAvatar.AddComponent<DummyFocusSceneViewScript>();
            }
            else
            {
                targetAvatar = settings.activeAvatar.gameObject;
                targetClothes = settings.clothesToDress;
            }

            foreach (var hook in hooks)
            {
                if (!hook.Evaluate(report, settings, targetAvatar, targetClothes))
                {
                    break;
                }
            }

            if (report.errors > 0)
            {
                report.result = DressCheckResult.INCOMPATIBLE;
            }
            else if (report.warnings > 0)
            {
                report.result = DressCheckResult.COMPATIBLE;
            }
            else
            {
                report.result = DressCheckResult.OK;
            }

            report.dressReportDump.avatar_components_csv = DebugDump.GenerateSpecialComponentsCsv(report.avatarDynamics, DebugDump.FindParentConstraints(targetAvatar.transform));
            report.dressReportDump.clothes_components_csv = DebugDump.GenerateSpecialComponentsCsv(report.clothesDynamics, null);
            report.dressReportDump.resultant_avatar_tree_csv = DebugDump.GenerateGameObjectTreeCsv(targetAvatar.transform, DebugDump.GameObjectCsvHeader);
            report.dressReportDump.resultant_clothes_tree_csv = DebugDump.GenerateGameObjectTreeCsv(targetClothes.transform, DebugDump.GameObjectCsvHeader);
            report.dressReportDump.errors = (int)report.errors;
            report.dressReportDump.infos = (int)report.infos;
            report.dressReportDump.result = (int)report.result;
            report.dressReportDump.warnings = (int)report.warnings;

#if UNITY_EDITOR
            Selection.activeGameObject = targetAvatar;
            SceneView.FrameLastActiveSceneView();
#endif
            return report;
        }
    }
}
