using System.Collections.Generic;
using Chocopoi.DressingTools.Dresser;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;

namespace Chocopoi.DressingTools.Tests
{
    // a test script base containing utility functions
    public class DTTestBase
    {
        private List<GameObject> instantiatedGameObjects;

        protected T LoadTestAsset<T>(string relativePath) where T : Object
        {
            // load test asset from resources folder
            var path = "Packages/com.chocopoi.vrc.dressingtools/Tests/Resources/" + GetType().Name + "/" + relativePath;
            var obj = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.NotNull(obj, "Could not find test asset at path:" + path);
            return obj;
        }

        protected GameObject CreateGameObject(string name, Transform parent = null)
        {
            // create an object and bound it to the parent (if any)
            var obj = new GameObject(name);
            if (parent)
            {
                obj.transform.parent = parent.transform;
            }
            instantiatedGameObjects.Add(obj);
            return obj;
        }

        protected GameObject InstantiateTestPrefab(string relativePath, Transform parent = null)
        {
            // load test prefab and instantiate it
            var prefab = LoadTestAsset<GameObject>(relativePath);
            var obj = Object.Instantiate(prefab);
            instantiatedGameObjects.Add(obj);

            if (parent)
            {
                obj.transform.parent = parent;
            }
            return obj;
        }

#if VRC_SDK_VRCSDK3
        private const string InitialVrcAvatarPrefabGuid = "505980abfabf5544b913cd14405b65e6";

        protected GameObject InstantiateInitialVRCAvatar()
        {
            // instantiate the initial vrc avatar prefab and return it
            var prefabPath = AssetDatabase.GUIDToAssetPath(InitialVrcAvatarPrefabGuid);
            var obj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            instantiatedGameObjects.Add(obj);
            return obj;
        }
#endif

        [SetUp]
        public virtual void Prepare()
        {
            // init list
            instantiatedGameObjects = new List<GameObject>();
        }

        [TearDown]
        public virtual void CleanUp()
        {
            // remove all instantiated gameobjects from tests
            foreach (var obj in instantiatedGameObjects)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
