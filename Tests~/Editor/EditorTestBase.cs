using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests
{
    // a test script base containing utility functions
    internal class EditorTestBase : RuntimeTestBase
    {
        protected T LoadEditorTestAsset<T>(string relativePath) where T : Object
        {
            // load test asset from resources folder
            var path = "Packages/com.chocopoi.vrc.dressingtools/Tests/Editor/Resources/" + GetType().Name + "/" + relativePath;
            var obj = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.NotNull(obj, "Could not find test asset at path:" + path);
            return obj;
        }

        protected GameObject InstantiateEditorTestPrefab(string relativePath, Transform parent = null)
        {
            // load test prefab and instantiate it
            var prefab = LoadEditorTestAsset<GameObject>(relativePath);
            var obj = Object.Instantiate(prefab);
            instantiatedGameObjects.Add(obj);

            if (parent)
            {
                obj.transform.parent = parent;
            }
            return obj;
        }

        public override void SetUp()
        {
            base.SetUp();

            // remove previous generated files
            AssetDatabase.DeleteAsset(DKNativeContext.GeneratedAssetsPath);
            AssetDatabase.CreateFolder("Assets", DKNativeContext.GeneratedAssetsFolderName);
        }

        public CabinetContext CreateCabinetContext(GameObject avatarObj)
        {
            var cabinet = avatarObj.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var dkCtx = new DKNativeContext(avatarObj);
            var cabCtx = new CabinetContext()
            {
                dkCtx = dkCtx,
                cabinetConfig = CabinetConfigUtility.Deserialize(cabinet.ConfigJson),
                avatarDynamics = OneConfUtils.ScanAvatarOnlyDynamics(avatarObj)
            };

            return cabCtx;
        }

        public WearableContext CreateWearableContext(CabinetContext cabCtx, GameObject wearableObj)
        {
            var wearableComp = wearableObj.GetComponent<DTWearable>();
            Assert.NotNull(wearableComp);

            var wearCtx = new WearableContext()
            {
                wearableConfig = WearableConfigUtility.Deserialize(wearableComp.ConfigJson),
                wearableGameObject = wearableObj,
                wearableDynamics = DynamicsUtils.ScanDynamics(wearableObj)
            };

            cabCtx.wearableContexts[wearableComp] = wearCtx;

            return wearCtx;
        }

        public void AssertPassImportedDynamicBone()
        {
            if (DKEditorUtils.FindType("DynamicBone") == null)
            {
                Assert.Pass("This test requires DynamicBones to be imported");
            }
        }

        public void AssertPassImportedVRCSDK()
        {
#if !DT_VRCSDK3A
            Assert.Pass("This test requires VRCSDK3 (>=2022.04.21.03.29) to be imported");
#endif
        }
    }
}
