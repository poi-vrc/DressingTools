﻿using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class BlendshapeSyncModuleEditorPresenter
    {
        private IBlendshapeSyncModuleEditorView view_;
        private IWearableConfigView configView_;
        private BlendshapeSyncModule module_;

        public BlendshapeSyncModuleEditorPresenter(IBlendshapeSyncModuleEditorView view, IWearableConfigView configView, BlendshapeSyncModule module)
        {
            view_ = view;
            configView_ = configView;
            module_ = module;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            view_.Load += OnLoad;
            view_.Unload += OnUnload;

            view_.AddBlendshapeSyncButtonClick += OnAddBlendshapeSyncButtonClick;
        }

        private void UnsubscribeEvents()
        {
            view_.Load -= OnLoad;
            view_.Unload -= OnUnload;

            view_.AddBlendshapeSyncButtonClick -= OnAddBlendshapeSyncButtonClick;
        }

        private void OnAddBlendshapeSyncButtonClick()
        {
            module_.blendshapeSyncs.Add(new DTAnimationBlendshapeSync());
            UpdateView();
        }

        private string[] GetBlendshapeNames(Mesh mesh)
        {
            if (mesh == null)
            {
                return null;
            }

            string[] names = new string[mesh.blendShapeCount];
            for (var i = 0; i < names.Length; i++)
            {
                names[i] = mesh.GetBlendShapeName(i);
            }
            return names;
        }

        public void UpdateView()
        {
            // do not render editor if these two are not available
            if (configView_.TargetAvatar == null || configView_.TargetWearable == null)
            {
                view_.ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox = true;
                return;
            }
            view_.ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox = false;

            view_.BlendshapeSyncs.Clear();
            foreach (var blendshapeSync in module_.blendshapeSyncs)
            {
                var avatarTransform = blendshapeSync.avatarPath != null ? configView_.TargetAvatar.transform.Find(blendshapeSync.avatarPath) : null;
                var avatarSmr = avatarTransform?.GetComponent<SkinnedMeshRenderer>();
                var avatarMesh = avatarSmr != null ? avatarSmr.sharedMesh : null;
                var avatarBlendshapeNames = GetBlendshapeNames(avatarMesh);

                var wearableTransform = blendshapeSync.wearablePath != null ? configView_.TargetWearable.transform.Find(blendshapeSync.wearablePath) : null;
                var wearableSmr = wearableTransform?.GetComponent<SkinnedMeshRenderer>();
                var wearableMesh = wearableSmr != null ? wearableSmr.sharedMesh : null;
                var wearableBlendshapeNames = GetBlendshapeNames(wearableMesh);

                // TODO: custom boundaries, now simply just invert 0-100 to 100-0
                var invertedBoundaries = blendshapeSync.avatarFromValue == 0 && blendshapeSync.avatarToValue == 100 && blendshapeSync.wearableFromValue == 100 && blendshapeSync.wearableToValue == 0;

                var blendshapeSyncData = new BlendshapeSyncData()
                {
                    isAvatarGameObjectInvalid = avatarTransform == null || avatarMesh == null,
                    avatarGameObject = avatarTransform?.gameObject,
                    avatarAvailableBlendshapeNames = avatarBlendshapeNames,

                    isWearableGameObjectInvalid = wearableTransform == null || wearableMesh == null,
                    wearableGameObject = wearableTransform?.gameObject,
                    wearableAvailableBlendshapeNames = wearableBlendshapeNames,

                    inverted = invertedBoundaries
                };

                // select blendshape index
                if (avatarBlendshapeNames != null)
                {
                    var avatarSelectedBlendshapeIndex = System.Array.IndexOf(blendshapeSyncData.avatarAvailableBlendshapeNames, blendshapeSync.avatarBlendshapeName);
                    if (avatarSelectedBlendshapeIndex == -1)
                    {
                        avatarSelectedBlendshapeIndex = 0;
                    }
                    blendshapeSyncData.avatarSelectedBlendshapeIndex = avatarSelectedBlendshapeIndex;
                }

                if (wearableBlendshapeNames != null)
                {
                    var wearableSelectedBlendshapeIndex = System.Array.IndexOf(blendshapeSyncData.wearableAvailableBlendshapeNames, blendshapeSync.wearableBlendshapeName);
                    if (wearableSelectedBlendshapeIndex == -1)
                    {
                        wearableSelectedBlendshapeIndex = 0;
                    }
                    blendshapeSyncData.wearableSelectedBlendshapeIndex = wearableSelectedBlendshapeIndex;
                }

                blendshapeSyncData.avatarGameObjectFieldChangeEvent = () =>
                {
                    var newSmr = blendshapeSyncData.avatarGameObject?.GetComponent<SkinnedMeshRenderer>();
                    var newMesh = newSmr != null ? newSmr.sharedMesh : null;
                    if (blendshapeSyncData.avatarGameObject != null && newMesh != null && newMesh.blendShapeCount > 0 && DTRuntimeUtils.IsGrandParent(configView_.TargetAvatar.transform, blendshapeSyncData.avatarGameObject.transform))
                    {
                        // renew path if changed
                        blendshapeSyncData.isAvatarGameObjectInvalid = false;
                        blendshapeSync.avatarPath = DTRuntimeUtils.GetRelativePath(blendshapeSyncData.avatarGameObject.transform, configView_.TargetAvatar.transform);

                        // generate blendshape names
                        blendshapeSyncData.avatarAvailableBlendshapeNames = GetBlendshapeNames(newMesh);
                        blendshapeSyncData.avatarSelectedBlendshapeIndex = 0;
                        blendshapeSyncData.avatarBlendshapeValue = 0;

                        blendshapeSync.avatarBlendshapeName = blendshapeSyncData.avatarAvailableBlendshapeNames[blendshapeSyncData.avatarSelectedBlendshapeIndex];
                    }
                    else
                    {
                        blendshapeSyncData.isAvatarGameObjectInvalid = true;
                    }
                };
                blendshapeSyncData.avatarBlendshapeNameChangeEvent = () => blendshapeSync.avatarBlendshapeName = blendshapeSyncData.avatarAvailableBlendshapeNames[blendshapeSyncData.avatarSelectedBlendshapeIndex];

                blendshapeSyncData.wearableGameObjectFieldChangeEvent = () =>
                {
                    var newSmr = blendshapeSyncData.wearableGameObject?.GetComponent<SkinnedMeshRenderer>();
                    var newMesh = newSmr != null ? newSmr.sharedMesh : null;
                    if (blendshapeSyncData.wearableGameObject != null && newMesh != null && newMesh.blendShapeCount > 0 && DTRuntimeUtils.IsGrandParent(configView_.TargetWearable.transform, blendshapeSyncData.wearableGameObject.transform))
                    {
                        // renew path if changed
                        blendshapeSyncData.isWearableGameObjectInvalid = false;
                        blendshapeSync.wearablePath = DTRuntimeUtils.GetRelativePath(blendshapeSyncData.wearableGameObject.transform, configView_.TargetWearable.transform);

                        // generate blendshape names
                        blendshapeSyncData.wearableAvailableBlendshapeNames = GetBlendshapeNames(newMesh);
                        blendshapeSyncData.wearableSelectedBlendshapeIndex = 0;
                        blendshapeSyncData.wearableBlendshapeValue = 0;

                        blendshapeSync.wearableBlendshapeName = blendshapeSyncData.wearableAvailableBlendshapeNames[blendshapeSyncData.wearableSelectedBlendshapeIndex];
                    }
                    else
                    {
                        blendshapeSyncData.isWearableGameObjectInvalid = true;
                    }
                };
                blendshapeSyncData.wearableBlendshapeNameChangeEvent = () => blendshapeSync.wearableBlendshapeName = blendshapeSyncData.wearableAvailableBlendshapeNames[blendshapeSyncData.wearableSelectedBlendshapeIndex];

                blendshapeSyncData.invertedToggleChangeEvent = () =>
                {
                    // TODO: custom boundaries, now simply just invert 0-100 to 100-0
                    if (blendshapeSyncData.inverted)
                    {
                        blendshapeSync.avatarFromValue = 0;
                        blendshapeSync.avatarToValue = 100;
                        blendshapeSync.wearableFromValue = 100;
                        blendshapeSync.wearableToValue = 0;
                    }
                    else
                    {
                        blendshapeSync.avatarFromValue = 0;
                        blendshapeSync.avatarToValue = 100;
                        blendshapeSync.wearableFromValue = 0;
                        blendshapeSync.wearableToValue = 100;
                    }

                };

                blendshapeSyncData.removeButtonClickEvent = () =>
                {
                    module_.blendshapeSyncs.Remove(blendshapeSync);
                    view_.BlendshapeSyncs.Remove(blendshapeSyncData);
                };

                view_.BlendshapeSyncs.Add(blendshapeSyncData);
            }
        }


        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
