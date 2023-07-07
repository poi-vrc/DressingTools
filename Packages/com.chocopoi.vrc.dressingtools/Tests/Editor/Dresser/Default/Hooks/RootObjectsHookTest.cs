using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Dresser.Default.Hooks;
using Chocopoi.DressingTools.Logging;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser.Default.Hooks
{
    public class RootObjectsHookTest : DTTestBase
    {
        private bool EvaluateHook(GameObject avatarRoot, GameObject wearableRoot, out DTReport report, out List<DTObjectMapping> objectMappings)
        {
            report = new DTReport();
            var settings = new DTDefaultDresserSettings();
            var boneMappings = new List<DTBoneMapping>();
            objectMappings = new List<DTObjectMapping>();
            var hook = new RootObjectsHook();

            settings.targetAvatar = avatarRoot;
            settings.targetWearable = wearableRoot;
            settings.avatarArmatureName = "Armature";
            settings.wearableArmatureName = "Armature";
            settings.dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;

            return hook.Evaluate(report, settings, boneMappings, objectMappings);
        }

        private static readonly DTObjectMapping[] ExpectedObjectMappings = new DTObjectMapping[]
        {
            new DTObjectMapping() { avatarObjectPath = "", wearableObjectPath = "MyObject1" },
            new DTObjectMapping() { avatarObjectPath = "", wearableObjectPath = "MyObject2" },
            new DTObjectMapping() { avatarObjectPath = "", wearableObjectPath = "MyObject3" },
            new DTObjectMapping() { avatarObjectPath = "", wearableObjectPath = "MyObject4" },
            new DTObjectMapping() { avatarObjectPath = "", wearableObjectPath = "MyObject5" },
        };

        [Test]
        public void RootObjects_ReturnsCorrectObjectMappings()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_RootObjectsWearable.prefab");

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var objectMappings);
            Assert.True(result, "Hook should return true");

            Assert.AreEqual(ExpectedObjectMappings.Length, objectMappings.Count, "Object mapping length is not equal to expected length");

            foreach (var objectMapping in objectMappings)
            {
                Debug.Log(string.Format("Checking: ", objectMapping.ToString()));

                bool found = false;
                foreach (var expectedMapping in ExpectedObjectMappings)
                {
                    if (expectedMapping.Equals(objectMapping))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such mapping in expected object mapping array: " + objectMapping.ToString());
            }
        }
    }
}
