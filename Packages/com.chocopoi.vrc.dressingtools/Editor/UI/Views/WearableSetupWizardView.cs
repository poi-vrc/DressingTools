/*
 * File: WearableSetupWizardView.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 1:22:09 am
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

using System;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class WearableSetupWizardView : ElementViewBase, IWearableSetupWizardView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;
        private static readonly Color PreviewButtonActiveColour = new Color(0.5f, 1, 0.5f, 1);
        private static readonly string[] ToolbarKeys = new string[] {
            "1.\n" + t._("dressing.wearableSetupWizard.steps.mapping"),
            "2.\n" + t._("dressing.wearableSetupWizard.steps.animate"),
            "3.\n" + t._("dressing.wearableSetupWizard.steps.integrate"),
            "4.\n" + t._("dressing.wearableSetupWizard.steps.optimize")
            };
        public event Action TargetAvatarOrWearableChange { add { _dressingSubView.TargetAvatarOrWearableChange += value; } remove { _dressingSubView.TargetAvatarOrWearableChange -= value; } }
        public event Action PreviousButtonClick;
        public event Action NextButtonClick;
        public event Action PreviewButtonClick;
        public event Action CaptureNewThumbnailButtonClick;
        public event Action ThumbnailCaptureButtonClick;
        public event Action ThumbnailCancelButtonClick;
        public event Action ThumbnailCaptureSettingsChange;

        public ArmatureMappingWearableModuleConfig ArmatureMappingModuleConfig { get; set; }
        public MoveRootWearableModuleConfig MoveRootModuleConfig { get; set; }
        public AnimationGenerationWearableModuleConfig AnimationGenerationModuleConfig { get; set; }
        public BlendshapeSyncWearableModuleConfig BlendshapeSyncModuleConfig { get; set; }
        public ArmatureMappingWearableModuleEditor ArmatureMappingModuleEditor { get; set; }
        public MoveRootWearableModuleEditor MoveRootModuleEditor { get; set; }
        public AnimationGenerationWearableModuleEditor AnimationGenerationModuleEditor { get; set; }
        public BlendshapeSyncWearableModuleEditor BlendshapeSyncModuleEditor { get; set; }
        public GameObject TargetAvatar { get => _dressingSubView.TargetAvatar; set => _dressingSubView.TargetAvatar = value; }
        public GameObject TargetWearable { get => _dressingSubView.TargetWearable; set => _dressingSubView.TargetWearable = value; }
        public WearableConfig Config { get => _dressingSubView.Config; set => _dressingSubView.Config = value; }
        public bool UseArmatureMapping { get; set; }
        public bool UseMoveRoot { get; set; }
        public bool UseAnimationGeneration { get; set; }
        public bool UseBlendshapeSync { get; set; }
        public int CurrentStep { get; set; }
        public bool ShowAvatarNoCabinetHelpBox { get; set; }
        public bool ShowArmatureNotFoundHelpBox { get; set; }
        public bool ShowArmatureGuessedHelpBox { get; set; }
        public bool ShowCabinetConfigErrorHelpBox { get; set; }
        public bool UseCustomWearableName { get; set; }
        public string CustomWearableName { get; set; }
        public bool ThumbnailCaptureWearableOnly { get; set; }
        public bool ThumbnailCaptureRemoveBackground { get; set; }
        public bool CaptureActive { get; set; }
        public bool PreviewActive => DTEditorUtils.PreviewActive;

        private WearableSetupWizardPresenter _presenter;
        private IDressingSubView _dressingSubView;
        private Button[] _stepBtns;
        private VisualElement _stepMappingContainer;
        private VisualElement _stepAnimateContainer;
        private TextField _textFieldCustomName;
        private Label _labelWearableName;
        private Toggle _toggleUseCustomName;
        private Toggle _toggleCaptureWearableOnly;
        private Toggle _toggleCaptureRemoveBackground;
        private Button _btnPrevious;
        private Button _btnNext;
        private Button _btnPreview;
        private Button _btnCaptureNewThumbnail;
        private Button _btnThumbnailCapture;
        private Button _btnThumbnailCancel;
        private VisualElement _infoPanel;
        private VisualElement _capturePanel;
        private VisualElement _thumbnail;
        private Toggle _toggleArmatureMapping;
        private Toggle _toggleMoveRoot;
        private Toggle _toggleAnimGen;
        private Toggle _toggleBlendshapeSync;
        private VisualElement _armatureMappingContainer;
        private VisualElement _moveRootContainer;
        private VisualElement _animGenContainer;
        private VisualElement _blendshapeSyncContainer;
        private VisualElement _helpBoxContainer;

        public WearableSetupWizardView(IDressingSubView dressingSubView)
        {
            _dressingSubView = dressingSubView;
            _presenter = new WearableSetupWizardPresenter(this);

            UseArmatureMapping = false;
            UseMoveRoot = false;
            UseAnimationGeneration = false;
            UseBlendshapeSync = false;

            ThumbnailCaptureWearableOnly = true;
            ThumbnailCaptureRemoveBackground = true;
            CaptureActive = false;

            ArmatureMappingModuleConfig = new ArmatureMappingWearableModuleConfig();
            MoveRootModuleConfig = new MoveRootWearableModuleConfig();
            AnimationGenerationModuleConfig = new AnimationGenerationWearableModuleConfig();
            BlendshapeSyncModuleConfig = new BlendshapeSyncWearableModuleConfig();

            // TODO: do not pass null to provider argument

            ArmatureMappingModuleEditor = new ArmatureMappingWearableModuleEditor(this, null, ArmatureMappingModuleConfig);
            MoveRootModuleEditor = new MoveRootWearableModuleEditor(this, null, MoveRootModuleConfig);
            AnimationGenerationModuleEditor = new AnimationGenerationWearableModuleEditor(this, null, AnimationGenerationModuleConfig);
            BlendshapeSyncModuleEditor = new BlendshapeSyncWearableModuleEditor(this, null, BlendshapeSyncModuleConfig);
        }

        public void GenerateConfig() => _presenter.GenerateConfig();

        public void ShowNoAvatarOrWearableDialog()
        {
            EditorUtility.DisplayDialog(t._("tool.name"), t._("dressing.wearableSetupWizard.dialog.msg.selectAvatarWearable"), t._("common.dialog.btn.ok"));
        }

        public void ShowInvalidConfigDialog()
        {
            EditorUtility.DisplayDialog(t._("tool.name"), t._("dressing.wearableSetupWizard.dialog.msg.fixInvalidConfig"), t._("common.dialog.btn.ok"));
        }

        public bool IsValid()
        {
            if (TargetAvatar == null || TargetWearable == null)
            {
                return false;
            }

            var mappingValid = true;
            mappingValid &= !UseArmatureMapping || ArmatureMappingModuleEditor.IsValid();
            mappingValid &= !UseMoveRoot || MoveRootModuleEditor.IsValid();
            _stepBtns[0].EnableInClassList("invalid", !mappingValid);

            var animateValid = true;
            animateValid &= !UseAnimationGeneration || AnimationGenerationModuleEditor.IsValid();
            animateValid &= !UseBlendshapeSync || BlendshapeSyncModuleEditor.IsValid();
            _stepBtns[1].EnableInClassList("invalid", !animateValid);

            return mappingValid && animateValid;
        }

        public void RaiseDoAddToCabinetEvent()
        {
            _dressingSubView.RaiseDoAddToCabinetEvent();
        }

        public void ShowInfoPanel()
        {
            _infoPanel.style.display = DisplayStyle.Flex;
            _capturePanel.style.display = DisplayStyle.None;
            CaptureActive = false;
        }

        public void ShowCapturePanel()
        {
            _infoPanel.style.display = DisplayStyle.None;
            _capturePanel.style.display = DisplayStyle.Flex;
            CaptureActive = true;
        }

        public void SetThumbnailTexture(Texture2D texture)
        {
            _thumbnail.style.backgroundImage = new StyleBackground(texture);
        }

        public void RepaintCapturePreview()
        {
            SetThumbnailTexture(DTEditorUtils.GetThumbnailCameraPreview());
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("WearableSetupWizardView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("WearableSetupWizardViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            // TODO: perform check if config changed only
            RegisterCallback((MouseMoveEvent evt) =>
            {
                if (CaptureActive)
                {
                    RepaintCapturePreview();
                }
                _btnPreview.EnableInClassList("active", PreviewActive);
                IsValid();
            });

            _stepMappingContainer = Q<VisualElement>("step-mapping-container").First();
            _stepAnimateContainer = Q<VisualElement>("step-animate-container").First();

            _textFieldCustomName = Q<TextField>("textfield-custom-wearable-name").First();
            // TODO: due to a weird bug, we have to localize text field here
            _textFieldCustomName.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                if (UseCustomWearableName)
                {
                    _labelWearableName.text = evt.newValue;
                }
                CustomWearableName = _textFieldCustomName.text;
            });
            _textFieldCustomName.label = t._(_textFieldCustomName.label.Substring(1));

            _labelWearableName = Q<Label>("label-wearable-name").First();

            _toggleUseCustomName = Q<Toggle>("toggle-use-custom-name").First();
            _toggleUseCustomName.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                if (TargetWearable != null)
                {
                    _textFieldCustomName.value = _labelWearableName.text = CustomWearableName = TargetWearable.name;
                }
                _textFieldCustomName.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                UseCustomWearableName = evt.newValue;
            });

            _toggleCaptureWearableOnly = Q<Toggle>("toggle-capture-wearable-only").First();
            _toggleCaptureRemoveBackground = Q<Toggle>("toggle-capture-remove-background").First();

            _btnPrevious = Q<Button>("btn-wizard-previous").First();
            _btnPrevious.clicked += PreviousButtonClick;
            _btnNext = Q<Button>("btn-wizard-next").First();
            _btnNext.clicked += NextButtonClick;
            _btnPreview = Q<Button>("btn-wizard-preview").First();
            _btnPreview.clicked += PreviewButtonClick;

            _thumbnail = Q<VisualElement>("thumbnail").First();

            _btnCaptureNewThumbnail = Q<Button>("btn-capture-new-thumbnail").First();
            _btnCaptureNewThumbnail.clicked += CaptureNewThumbnailButtonClick;
            _btnThumbnailCapture = Q<Button>("btn-thumbnail-capture").First();
            _btnThumbnailCapture.clicked += ThumbnailCaptureButtonClick;
            _btnThumbnailCancel = Q<Button>("btn-thumbnail-cancel").First();
            _btnThumbnailCancel.clicked += ThumbnailCancelButtonClick;

            _toggleCaptureWearableOnly.value = ThumbnailCaptureWearableOnly;
            _toggleCaptureWearableOnly.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                ThumbnailCaptureWearableOnly = evt.newValue;
                ThumbnailCaptureSettingsChange?.Invoke();
            });
            _toggleCaptureRemoveBackground.value = ThumbnailCaptureRemoveBackground;
            _toggleCaptureRemoveBackground.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                ThumbnailCaptureRemoveBackground = evt.newValue;
                ThumbnailCaptureSettingsChange?.Invoke();
            });

            _infoPanel = Q<VisualElement>("info-panel").First();
            _capturePanel = Q<VisualElement>("capture-panel").First();

            _toggleArmatureMapping = Q<Toggle>("toggle-armature-mapping").First();
            _toggleArmatureMapping.RegisterValueChangedCallback((ChangeEvent<bool> evt) => UseArmatureMapping = evt.newValue);
            _toggleMoveRoot = Q<Toggle>("toggle-move-root").First();
            _toggleMoveRoot.RegisterValueChangedCallback((ChangeEvent<bool> evt) => UseMoveRoot = evt.newValue);
            _toggleAnimGen = Q<Toggle>("toggle-anim-gen").First();
            _toggleAnimGen.RegisterValueChangedCallback((ChangeEvent<bool> evt) => UseAnimationGeneration = evt.newValue);
            _toggleBlendshapeSync = Q<Toggle>("toggle-blendshape-sync").First();
            _toggleBlendshapeSync.RegisterValueChangedCallback((ChangeEvent<bool> evt) => UseBlendshapeSync = evt.newValue);

            _armatureMappingContainer = Q<VisualElement>("armature-mapping-container").First();
            _moveRootContainer = Q<VisualElement>("move-root-container").First();
            _animGenContainer = Q<VisualElement>("anim-gen-container").First();
            _blendshapeSyncContainer = Q<VisualElement>("blendshape-sync-container").First();

            _helpBoxContainer = Q<VisualElement>("helpbox-container").First();

            _armatureMappingContainer.Add(ArmatureMappingModuleEditor);
            _moveRootContainer.Add(MoveRootModuleEditor);
            _animGenContainer.Add(AnimationGenerationModuleEditor);
            _blendshapeSyncContainer.Add(BlendshapeSyncModuleEditor);
        }

        private void BindFoldouts()
        {
            BindFoldoutHeaderWithContainer("foldout-wearable-info", "wearable-info-container");
            BindFoldoutHeaderWithContainer("foldout-armature-mapping", "armature-mapping-container");
            BindFoldoutHeaderWithContainer("foldout-move-root", "move-root-container");
            BindFoldoutHeaderWithContainer("foldout-anim-gen", "anim-gen-container");
            BindFoldoutHeaderWithContainer("foldout-blendshape-sync", "blendshape-sync-container");
        }

        private void BindSteps()
        {
            var mappingBtn = Q<Button>("btn-step-mapping");
            var animateBtn = Q<Button>("btn-step-animate");

            _stepBtns = new Button[] { mappingBtn, animateBtn };

            for (var i = 0; i < _stepBtns.Length; i++)
            {
                var stepIndex = i;
                _stepBtns[i].clicked += () =>
                {
                    if (CurrentStep == stepIndex) return;
                    CurrentStep = stepIndex;
                    UpdateSteps();
                };
                _stepBtns[i].EnableInClassList("active", stepIndex == CurrentStep);
            }
        }

        private void UpdateSteps()
        {
            for (var i = 0; i < _stepBtns.Length; i++)
            {
                _stepBtns[i].EnableInClassList("active", i == CurrentStep);
            }

            if (CurrentStep == 0)
            {
                _stepMappingContainer.style.display = DisplayStyle.Flex;
                _stepAnimateContainer.style.display = DisplayStyle.None;
            }
            else if (CurrentStep == 1)
            {
                _stepMappingContainer.style.display = DisplayStyle.None;
                _stepAnimateContainer.style.display = DisplayStyle.Flex;
            }

            UpdateButtonNextText();
        }

        public override void OnEnable()
        {
            InitVisualTree();
            BindSteps();
            BindFoldouts();

            t.LocalizeElement(this);

            ArmatureMappingModuleEditor.OnEnable();
            MoveRootModuleEditor.OnEnable();
            AnimationGenerationModuleEditor.OnEnable();
            BlendshapeSyncModuleEditor.OnEnable();

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            ArmatureMappingModuleEditor.OnDisable();
            MoveRootModuleEditor.OnDisable();
            AnimationGenerationModuleEditor.OnDisable();
            BlendshapeSyncModuleEditor.OnDisable();
        }

        public void UpdateAvatarPreview() => _presenter.UpdateAvatarPreview();

        public void Repaint()
        {
            UpdateSteps();

            // add helpboxes
            _helpBoxContainer.Clear();
            if (ShowAvatarNoCabinetHelpBox)
            {
                _helpBoxContainer.Add(CreateHelpBox(t._("dressing.editor.wizard.autoSetup.helpbox.avatarHasNoCabinetUsingDefault"), MessageType.Warning));
            }
            if (ShowArmatureNotFoundHelpBox)
            {
                _helpBoxContainer.Add(CreateHelpBox(t._("dressing.editor.wizard.autoSetup.helpbox.wearableArmatureNotFound"), MessageType.Warning));
            }
            if (ShowArmatureGuessedHelpBox)
            {
                _helpBoxContainer.Add(CreateHelpBox(t._("dressing.editor.wizard.autoSetup.helpbox.armatureGuessed"), MessageType.Warning));
            }
            if (ShowCabinetConfigErrorHelpBox)
            {
                _helpBoxContainer.Add(CreateHelpBox(t._("dressing.editor.wizard.autoSetup.helpbox.unableToLoadCabinetConfig"), MessageType.Warning));
            }

            _toggleUseCustomName.value = UseCustomWearableName;
            _labelWearableName.text = UseCustomWearableName ? CustomWearableName : (TargetWearable != null ? TargetWearable.name : "---");

            _toggleArmatureMapping.value = UseArmatureMapping;
            Q<Foldout>("foldout-armature-mapping").First().value = UseArmatureMapping;
            _toggleMoveRoot.value = UseMoveRoot;
            Q<Foldout>("foldout-move-root").First().value = UseMoveRoot;
            _toggleAnimGen.value = UseAnimationGeneration;
            Q<Foldout>("foldout-anim-gen").First().value = UseAnimationGeneration;
            _toggleBlendshapeSync.value = UseBlendshapeSync;
            Q<Foldout>("foldout-blendshape-sync").First().value = UseBlendshapeSync;
        }

        private void UpdateButtonNextText()
        {
            // last step set to finish
            _btnNext.text = CurrentStep == 1 ? "Finish!" : "Next >";
        }
    }
}
