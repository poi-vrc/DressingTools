using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.Api.Cabinet;
using Chocopoi.DressingTools.Api.Wearable;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests
{
    // a test script base containing utility functions
    public class EditorTestBase : RuntimeTestBase
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
            AssetDatabase.DeleteAsset(ApplyCabinetContext.GeneratedAssetsPath);
            AssetDatabase.CreateFolder("Assets", ApplyCabinetContext.GeneratedAssetsFolderName);
        }

        public ApplyCabinetContext CreateCabinetContext(GameObject avatarObj)
        {
            var cabinet = avatarObj.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var cabCtx = new ApplyCabinetContext()
            {
                report = new DKReport(),
                cabinetConfig = CabinetConfigUtility.Deserialize(cabinet.configJson),
                avatarGameObject = avatarObj,
                pathRemapper = new PathRemapper(avatarObj),
                avatarDynamics = DKEditorUtils.ScanDynamics(avatarObj, true)
            };
            cabCtx.animationStore = new AnimationStore(cabCtx);

            return cabCtx;
        }

        public ApplyWearableContext CreateWearableContext(ApplyCabinetContext cabCtx, GameObject wearableObj)
        {
            var wearableComp = wearableObj.GetComponent<DTWearable>();
            Assert.NotNull(wearableComp);

            var wearCtx = new ApplyWearableContext()
            {
                wearableConfig = WearableConfigUtility.Deserialize(wearableComp.configJson),
                wearableGameObject = wearableObj,
                wearableDynamics = DKEditorUtils.ScanDynamics(wearableObj, false)
            };

            cabCtx.wearableContexts[wearableComp] = wearCtx;

            return wearCtx;
        }
    }
}
