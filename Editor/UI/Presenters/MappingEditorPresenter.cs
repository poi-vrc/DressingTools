/*
 * File: MappingEditorPresenter.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 12:21:25 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MappingEditorPresenter
    {
        private readonly IMappingEditorView _view;

        public MappingEditorPresenter(IMappingEditorView view)
        {
            _view = view;
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.BoneMappingModeChange += OnBoneMappingModeChange;
            _view.BoneMappingDisplayModeChange += OnBoneMappingDisplayModeChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.BoneMappingModeChange -= OnBoneMappingModeChange;
            _view.BoneMappingDisplayModeChange -= OnBoneMappingDisplayModeChange;
        }

        private void OnBoneMappingModeChange()
        {
            DTMappingEditorWindow.Data.boneMappingMode = (BoneMappingMode)_view.SelectedBoneMappingMode;
            DTMappingEditorWindow.Data.RaiseMappingEditorChangedEvent();
            UpdateOutputBoneMappings();
            UpdateView();
        }

        private void OnBoneMappingDisplayModeChange()
        {
            UpdateView();
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private List<BoneMapping> GetAvatarBoneMapping(List<BoneMapping> boneMappings, Transform avatarRoot, Transform targetAvatarBone)
        {
            var path = AnimationUtils.GetRelativePath(targetAvatarBone, avatarRoot);

            var avatarBoneMappings = new List<BoneMapping>();

            foreach (var boneMapping in boneMappings)
            {
                if (boneMapping.avatarBonePath == path)
                {
                    avatarBoneMappings.Add(boneMapping);
                }
            }

            return avatarBoneMappings;
        }

        private void UpdateOutputBoneMappings()
        {
            if (DTMappingEditorWindow.Data.generatedBoneMappings == null) return;

            if (DTMappingEditorWindow.Data.boneMappingMode == BoneMappingMode.Auto || DTMappingEditorWindow.Data.boneMappingMode == BoneMappingMode.Manual)
            {
                // copy generated to output
                DTMappingEditorWindow.Data.outputBoneMappings = new List<BoneMapping>(DTMappingEditorWindow.Data.generatedBoneMappings);
            }
            else
            {
                // empty list if override mode
                DTMappingEditorWindow.Data.outputBoneMappings = new List<BoneMapping>();
            }
        }

        private void UpdateView()
        {
            if (DTMappingEditorWindow.Data.targetAvatar == null || DTMappingEditorWindow.Data.targetWearable == null)
            {
                _view.ShowNoAvatarOrWearableSelectedHelpbox = true;
                return;
            }
            _view.ShowNoAvatarOrWearableSelectedHelpbox = false;

            var generatedBoneMappingsAvailable = DTMappingEditorWindow.Data.generatedBoneMappings != null;
            _view.ShowGeneratedBoneMappingNotAvailableHelpbox = !generatedBoneMappingsAvailable;

            _view.TargetAvatar = DTMappingEditorWindow.Data.targetAvatar;
            _view.TargetWearable = DTMappingEditorWindow.Data.targetWearable;
            _view.SelectedBoneMappingMode = (int)DTMappingEditorWindow.Data.boneMappingMode;

            _view.AvatarHierachyNodes.Clear();
            if (_view.SelectedBoneMappingMode == 0 && generatedBoneMappingsAvailable)
            {
                UpdateAvatarHierarchy(DTMappingEditorWindow.Data.generatedBoneMappings, DTMappingEditorWindow.Data.targetAvatar.transform, _view.AvatarHierachyNodes);
            }
            else if (_view.SelectedBoneMappingMode == 1 && generatedBoneMappingsAvailable)
            {
                if (_view.SelectedBoneMappingDisplayMode == 1)
                {
                    // override mode and resultant display mode
                    var previewBoneMappings = new List<BoneMapping>(DTMappingEditorWindow.Data.generatedBoneMappings);
                    OneConfUtils.HandleBoneMappingOverrides(previewBoneMappings, DTMappingEditorWindow.Data.outputBoneMappings);
                    UpdateAvatarHierarchy(previewBoneMappings, DTMappingEditorWindow.Data.targetAvatar.transform, _view.AvatarHierachyNodes);
                }
                else
                {
                    UpdateAvatarHierarchy(DTMappingEditorWindow.Data.outputBoneMappings, DTMappingEditorWindow.Data.targetAvatar.transform, _view.AvatarHierachyNodes);
                }
            }
            else if (_view.SelectedBoneMappingMode == 2)
            {
                UpdateAvatarHierarchy(DTMappingEditorWindow.Data.outputBoneMappings, DTMappingEditorWindow.Data.targetAvatar.transform, _view.AvatarHierachyNodes);
            }
        }

        public void UpdateAvatarHierarchy(List<BoneMapping> boneMappings, Transform parent, List<ViewAvatarHierachyNode> nodeList)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                // create a new node
                var node = new ViewAvatarHierachyNode()
                {
                    avatarObjectTransform = child,
                    AddMappingButtonClick = () =>
                    {
                        boneMappings.Add(new BoneMapping()
                        {
                            avatarBonePath = AnimationUtils.GetRelativePath(child, DTMappingEditorWindow.Data.targetAvatar.transform),
                            wearableBonePath = null,
                            mappingType = BoneMappingType.DoNothing
                        });
                        DTMappingEditorWindow.Data.RaiseMappingEditorChangedEvent();
                        UpdateView();
                    }
                };
                nodeList.Add(node);

                // obtain associated mappings
                var avatarBoneMappings = GetAvatarBoneMapping(boneMappings, DTMappingEditorWindow.Data.targetAvatar.transform, child);
                foreach (var boneMapping in avatarBoneMappings)
                {
                    var wearableTransform = boneMapping.wearableBonePath != null ? DTMappingEditorWindow.Data.targetWearable.transform.Find(boneMapping.wearableBonePath) : null;

                    var viewBoneMapping = new ViewBoneMapping()
                    {
                        isInvalid = wearableTransform == null,
                        wearablePath = boneMapping.wearableBonePath,
                        mappingType = (int)boneMapping.mappingType,
                        wearableObject = wearableTransform != null ? wearableTransform.gameObject : null
                    };
                    viewBoneMapping.MappingChange = () =>
                    {
                        var path = viewBoneMapping.wearableObject != null ? AnimationUtils.GetRelativePath(viewBoneMapping.wearableObject.transform, DTMappingEditorWindow.Data.targetWearable.transform) : null;

                        viewBoneMapping.isInvalid = path == null;
                        viewBoneMapping.wearablePath = path;

                        boneMapping.avatarBonePath = AnimationUtils.GetRelativePath(child, DTMappingEditorWindow.Data.targetAvatar.transform);
                        boneMapping.wearableBonePath = path;
                        boneMapping.mappingType = (BoneMappingType)viewBoneMapping.mappingType;
                        DTMappingEditorWindow.Data.RaiseMappingEditorChangedEvent();
                    };
                    viewBoneMapping.RemoveMappingButtonClick = () =>
                    {
                        DTMappingEditorWindow.Data.outputBoneMappings.Remove(boneMapping);
                        node.wearableMappings.Remove(viewBoneMapping);
                        DTMappingEditorWindow.Data.RaiseMappingEditorChangedEvent();
                    };

                    node.wearableMappings.Add(viewBoneMapping);
                }

                UpdateAvatarHierarchy(boneMappings, child, node.childs);
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
