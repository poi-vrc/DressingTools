#if VRC_SDK_VRCSDK3
using UnityEngine;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using VRC.SDKBase;

namespace Chocopoi.DressingTools.Integrations.VRC
{
    [InitializeOnLoad]
    public class BuildDTCabinetCallback : IVRCSDKPreprocessAvatarCallback, IVRCSDKPostprocessAvatarCallback
    {
        public int callbackOrder => -25;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            Debug.Log("Preprocess avatar");

            var cabinet = DTEditorUtils.GetAvatarCabinet(avatarGameObject);
            if (cabinet == null)
            {
                // avatar has no cabinet, skipping
                return true;
            }

            // display progress bar
            EditorUtility.DisplayProgressBar("DressingTools", "Preparing to process avatar...", 0);

            try
            {
                // create hook instances
                var hooks = new IBuildDTCabinetHook[]
                {
                    new ApplyCabinetHook(cabinet),
                    new GenerateAnimationsHook(cabinet),
                };

                // execute hooks
                foreach (var hook in hooks)
                {
                    if (!hook.OnPreprocessAvatar(avatarGameObject))
                    {
                        return false;
                    }
                }

                return true;
            } catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("DressingTools", "Error processing avatar: " + ex.Message, "OK");
                return false;
            } finally
            {
                // hide the progress bar
                EditorUtility.ClearProgressBar();
            }
        }

        public void OnPostprocessAvatar()
        {
            Debug.Log("Postprocess avatar");
        }
    }

#region IEditorOnly Workaround
    // temporary workaround with VRCSDK to not remove IEditorOnly objects at early stage which causes problems
    // code referenced from MA: https://github.com/bdunderscore/modular-avatar/blob/main/Packages/nadena.dev.modular-avatar/Editor/PreventStripTagObjects.cs
    // https://feedback.vrchat.com/sdk-bug-reports/p/ieditoronly-components-should-be-destroyed-late-in-the-build-process

    [InitializeOnLoad]
    internal static class RemoveOriginalEditorOnlyCallback
    {
        static RemoveOriginalEditorOnlyCallback()
        {
            EditorApplication.delayCall += () =>
            {
                // obtain the private static field via reflection
                var callbackStaticField = typeof(VRCBuildPipelineCallbacks).GetField("_preprocessAvatarCallbacks", BindingFlags.Static | BindingFlags.NonPublic);
                var callbacks = (List<IVRCSDKPreprocessAvatarCallback>)callbackStaticField.GetValue(null);

                // remove RemoveAvatarEditorOnly
                var modifiedCallbacks = callbacks.Where(c => !(c is RemoveAvatarEditorOnly)).ToList();
                callbackStaticField.SetValue(null, modifiedCallbacks);
            };
        }
    }

    internal class ReplacementRemoveAvatarEditorOnly : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -1024;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            // iterate the avatar transforms to see if has tag EditorOnly
            foreach (var transform in avatarGameObject.GetComponentsInChildren<Transform>(true))
            {
                // remove if has tag
                if (transform != null && transform.CompareTag("EditorOnly"))
                {
                    Object.DestroyImmediate(transform.gameObject);
                }
            }
            return true;
        }
    }

    internal class ReplacementRemoveIEditorOnly : IVRCSDKPreprocessAvatarCallback
    {
        // execute the callback at a very very late order
        public int callbackOrder => int.MaxValue;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            // iterate all IEditorOnly objects
            foreach (var component in avatarGameObject.GetComponentsInChildren<IEditorOnly>(true))
            {
                Object.DestroyImmediate(component as Object);
            }
            return true;
        }
    }
#endregion IEditorOnly Workaround

}
#endif
