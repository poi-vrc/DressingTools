using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class TestModeRule : IDressCheckRule
    {
        private static AnimatorController testModeAnimationController;

        public TestModeRule()
        {
            testModeAnimationController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/chocopoi/DressingTools/Animations/TestModeAnimationController.controller");

            if (testModeAnimationController == null)
            {
                Debug.LogError("[DressingTools] Could not load \"TestModeAnimationController\" from \"Assets/chocopoi/DressingTools/Animations\". Did you move it to another location?");
            }
        }

        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            Animator animator = targetAvatar.GetComponent<Animator>();

            //add animation controller
            if (animator != null)
            {
                animator.runtimeAnimatorController = testModeAnimationController;
            }

            //add dummy focus sceneview script
            targetAvatar.AddComponent<DummyFocusSceneViewScript>();

            return true;
        }
    }
}
