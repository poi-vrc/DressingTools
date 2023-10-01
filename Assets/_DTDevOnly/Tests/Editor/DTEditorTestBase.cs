using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests
{
    // a test script base containing utility functions
    public class DTEditorTestBase : DTRuntimeTestBase
    {
        protected T LoadEditorTestAsset<T>(string relativePath) where T : Object
        {
            // load test asset from resources folder
            var path = "Assets/_DTDevOnly/Tests/Editor/Resources/" + GetType().Name + "/" + relativePath;
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
    }
}
