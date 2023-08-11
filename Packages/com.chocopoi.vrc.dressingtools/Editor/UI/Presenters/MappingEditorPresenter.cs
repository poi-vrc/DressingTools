using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
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
            _container.boneMappingMode = (DTBoneMappingMode)_view.SelectedBoneMappingMode;
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

        private List<DTBoneMapping> GetAvatarBoneMapping(List<DTBoneMapping> boneMappings, Transform avatarRoot, Transform targetAvatarBone)
        {
            var path = AnimationUtils.GetRelativePath(targetAvatarBone, avatarRoot);

            var avatarBoneMappings = new List<DTBoneMapping>();

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
            if (_container.boneMappingMode == DTBoneMappingMode.Auto || _container.boneMappingMode == DTBoneMappingMode.Manual)
            {
                // copy generated to output
                _container.outputBoneMappings = new List<DTBoneMapping>(_container.generatedBoneMappings);
            }
            else
            {
                // empty list if override mode
                _container.outputBoneMappings = new List<DTBoneMapping>();
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
                var previewBoneMappings = new List<DTBoneMapping>(_container.generatedBoneMappings);
                DTRuntimeUtils.HandleBoneMappingOverrides(previewBoneMappings, _container.outputBoneMappings);
                UpdateAvatarHierarchy(previewBoneMappings, _container.targetAvatar.transform, _view.AvatarHierachyNodes);
            }
            else
            {
                UpdateAvatarHierarchy(_container.outputBoneMappings, _container.targetAvatar.transform, _view.AvatarHierachyNodes);
            }
        }

        public void UpdateAvatarHierarchy(List<DTBoneMapping> boneMappings, Transform parent, List<ViewAvatarHierachyNode> nodeList)
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
                        boneMappings.Add(new DTBoneMapping()
                        {
                            avatarBonePath = AnimationUtils.GetRelativePath(child, _container.targetAvatar.transform),
                            wearableBonePath = null,
                            mappingType = DTBoneMappingType.DoNothing
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
                        boneMapping.mappingType = (DTBoneMappingType)viewBoneMapping.mappingType;
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
