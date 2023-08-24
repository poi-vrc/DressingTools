using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Chocopoi.DressingTools.Tests.Cabinet
{
    public class DTCabinetTest : DTRuntimeTestBase
    {
        [UnityTest]
        public IEnumerator ApplyInPlayModeOnLoad_AppliesNormally()
        {
            var avatarRoot = InstantiateRuntimeTestPrefab("DTTest_PhysBoneAvatarWithWearable.prefab");
            yield return null;
            // we are unable to check DTReport logs so we just check is the armature empty here
            var wearableRoot = avatarRoot.transform.Find("DTTest_PhysBoneWearable");
            Assert.NotNull(wearableRoot);
            var armature = wearableRoot.transform.Find("Armature");
            Assert.NotNull(armature);
            Assert.AreEqual(0, armature.childCount);
        }
    }
}
