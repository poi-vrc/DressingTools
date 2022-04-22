using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Chocopoi.DressingTools
{
    public class DressReport
    {
        private static readonly IDressCheckRule[] rules = new IDressCheckRule[]
        {
            new NotAPrefabRule(),
            new ExistingPrefixSuffixRule(),
            new FindAvatarDynamicsRule(),
            new ArmatureRule(),
            new MeshDataRule(),
            new FindClothesDynamicsRule()
        };

        private static AnimatorController testModeAnimationController;

        public DressCheckResult result;

        public DressCheckCodeMask.Info infos;

        public DressCheckCodeMask.Warn warnings;

        public DressCheckCodeMask.Error errors;

        // stores the gameobjects/dynamicbones detected during the check

        public List<DynamicBone> avatarDynBones;

        public List<VRCPhysBone> avatarPhysBones;

        public List<GameObject> clothesAllObjects;

        public List<DynamicBone> clothesOriginalDynBones;

        public List<DynamicBone> clothesDynBones;

        public List<VRCPhysBone> clothesOriginalPhysBones;

        public List<VRCPhysBone> clothesPhysBones;

        public List<GameObject> clothesMeshDataObjects;

        private DressReport()
        {
            testModeAnimationController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/chocopoi/DressingTools/Animations/TestModeAnimationController.controller");

            if (testModeAnimationController == null)
            {
                Debug.LogError("[DressingTools] Could not load \"TestModeAnimationController\" from \"Assets/chocopoi/DressingTools/Animations\". Did you move it to another location?");
            }

            avatarDynBones = new List<DynamicBone>();
            avatarPhysBones = new List<VRCPhysBone>();
            clothesDynBones = new List<DynamicBone>();
            clothesPhysBones = new List<VRCPhysBone>();
            clothesOriginalDynBones = new List<DynamicBone>();
            clothesOriginalPhysBones = new List<VRCPhysBone>();
            clothesAllObjects = new List<GameObject>();
            clothesMeshDataObjects = new List<GameObject>();
        }

        public static void CleanUp()
        {
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
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

            DressReport report = new DressReport();

            if (settings.activeAvatar == null || settings.clothesToDress == null)
            {
                report.result = DressCheckResult.INVALID_SETTINGS;
                report.errors |= DressCheckCodeMask.Error.NULL_ACTIVE_AVATAR_OR_CLOTHES;
                return report;
            }

            string avatarNewName = "DressingToolsPreview_" + settings.activeAvatar.gameObject.name;
            string clothesNewName = "DressingToolsPreview_" + settings.clothesToDress.name;

            GameObject targetAvatar;
            GameObject targetClothes;

            if (!write)
            {
                targetAvatar = Object.Instantiate(settings.activeAvatar.gameObject);
                targetAvatar.name = avatarNewName;

                Vector3 newAvatarPosition = targetAvatar.transform.position;
                newAvatarPosition.x -= 20;
                targetAvatar.transform.position = newAvatarPosition;

                targetClothes = Object.Instantiate(settings.clothesToDress);
                targetClothes.name = clothesNewName;

                Vector3 newClothesPosition = targetClothes.transform.position;
                newClothesPosition.x -= 20;
                targetClothes.transform.position = newClothesPosition;

                Animator animator = targetAvatar.GetComponent<Animator>();

                //add animation controller
                if (animator != null)
                {
                    animator.runtimeAnimatorController = testModeAnimationController;
                }

                //add dummy focus sceneview script
                targetAvatar.AddComponent<DummyFocusSceneViewScript>();
            } else
            {
                targetAvatar = settings.activeAvatar.gameObject;
                targetClothes = settings.clothesToDress;
            }

            foreach (IDressCheckRule rule in rules)
            {
                if (!rule.Evaluate(report, settings, targetAvatar, targetClothes))
                {
                    break;
                }
            }

            if (report.errors > 0)
            {
                report.result = DressCheckResult.INCOMPATIBLE;
            } else if (report.warnings > 0)
            {
                report.result = DressCheckResult.COMPATIBLE;
            } else
            {
                report.result = DressCheckResult.OK;
            }

#if UNITY_EDITOR
            Selection.activeGameObject = targetAvatar;
            SceneView.FrameLastActiveSceneView();
#endif
            return report;
        }
    }
}
