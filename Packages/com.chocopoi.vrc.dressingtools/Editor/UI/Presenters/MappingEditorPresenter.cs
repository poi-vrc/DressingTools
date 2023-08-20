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
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MappingEditorPresenter
    {
        private IMappingEditorView _view;
        private DTMappingEditorContainer _container;

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
            _container.boneMappingMode = (BoneMappingMode)_view.SelectedBoneMappingMode;
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

        public void SetContainer(DTMappingEditorContainer container)
        {
            _container = container;
            UpdateOutputBoneMappings();
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
            if (_container.boneMappingMode == BoneMappingMode.Auto || _container.boneMappingMode == BoneMappingMode.Manual)
            {
                // copy generated to output
                _container.outputBoneMappings = new List<BoneMapping>(_container.generatedBoneMappings);
            }
            else
            {
                // empty list if override mode
                _container.outputBoneMappings = new List<BoneMapping>();
            }
        }

        private void UpdateView()
        {
            if (_container == null || _container.targetAvatar == null || _container.targetWearable == null || _container.generatedBoneMappings == null)
            {
                _view.ShowBoneMappingNotAvailableHelpbox = true;
                return;
            }

            _view.ShowBoneMappingNotAvailableHelpbox = false;
            _view.TargetAvatar = _container.targetAvatar;
            _view.TargetWearable = _container.targetWearable;

            _view.AvatarHierachyNodes.Clear();
            // override mode and resultant display mode
            if (_view.SelectedBoneMappingMode == 1 && _view.SelectedBoneMappingDisplayMode == 1)
            {
                var previewBoneMappings = new List<BoneMapping>(_container.generatedBoneMappings);
                DTRuntimeUtils.HandleBoneMappingOverrides(previewBoneMappings, _container.outputBoneMappings);
                UpdateAvatarHierarchy(previewBoneMappings, _container.targetAvatar.transform, _view.AvatarHierachyNodes);
            }
            else
            {
                UpdateAvatarHierarchy(_container.outputBoneMappings, _container.targetAvatar.transform, _view.AvatarHierachyNodes);
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
                            avatarBonePath = AnimationUtils.GetRelativePath(child, _container.targetAvatar.transform),
                            wearableBonePath = null,
                            mappingType = BoneMappingType.DoNothing
                        });
                        UpdateView();
                    }
                };
                nodeList.Add(node);

                // obtain associated mappings
                var avatarBoneMappings = GetAvatarBoneMapping(boneMappings, _container.targetAvatar.transform, child);
                foreach (var boneMapping in avatarBoneMappings)
                {
                    var wearableTransform = boneMapping.wearableBonePath != null ? _container.targetWearable.transform.Find(boneMapping.wearableBonePath) : null;

                    var viewBoneMapping = new ViewBoneMapping()
                    {
                        isInvalid = wearableTransform == null,
                        wearablePath = boneMapping.wearableBonePath,
                        mappingType = (int)boneMapping.mappingType,
                        wearableObject = wearableTransform != null ? wearableTransform.gameObject : null
                    };
                    viewBoneMapping.MappingChange = () =>
                    {
                        var path = viewBoneMapping.wearableObject != null ? AnimationUtils.GetRelativePath(viewBoneMapping.wearableObject.transform, _container.targetWearable.transform) : null;

                        viewBoneMapping.isInvalid = path == null;
                        viewBoneMapping.wearablePath = path;

                        boneMapping.avatarBonePath = AnimationUtils.GetRelativePath(child, _container.targetAvatar.transform);
                        boneMapping.wearableBonePath = path;
                        boneMapping.mappingType = (BoneMappingType)viewBoneMapping.mappingType;
                    };
                    viewBoneMapping.RemoveMappingButtonClick = () =>
                    {
                        _container.outputBoneMappings.Remove(boneMapping);
                        node.wearableMappings.Remove(viewBoneMapping);
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
