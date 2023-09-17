/*
 * File: WearableConfigView.cs
 * Project: DressingTools
 * Created Date: Wednesday, September 13th 2023, 12:22:58 am
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class WearableConfigView : ElementViewBase, IWearableConfigView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;
        private static Texture2D ThumbnailPlaceholderImage;

        public event Action TargetAvatarOrWearableChange { add { _dressingSubView.TargetAvatarOrWearableChange += value; } remove { _dressingSubView.TargetAvatarOrWearableChange -= value; } }
        public event Action InfoNewThumbnailButtonClick;
        public event Action CaptureThumbnailButtonClick;
        public event Action CaptureCancelButtonClick;
        public event Action CaptureSettingsChange;
        public event Action ToolbarAutoSetupButtonClick;
        public event Action ToolbarPreviewButtonClick;
        public event Action AdvancedModuleAddButtonClick;
        public event Action ModeChange;
        public event Action AvatarConfigChange;

        public GameObject TargetAvatar { get => _dressingSubView.TargetAvatar; set => _dressingSubView.TargetAvatar = value; }
        public GameObject TargetWearable { get => _dressingSubView.TargetWearable; set => _dressingSubView.TargetWearable = value; }

        public WearableConfig Config { get => _dressingSubView.Config; set => _dressingSubView.Config = value; }

        public int SelectedMode { get => _selectedMode; set => _selectedMode = value; }

        public Texture2D InfoThumbnail { get; set; }
        public bool InfoUseCustomWearableName { get; set; }
        public string InfoCustomWearableName { get; set; }
        public string InfoUuid { get; set; }
        public string InfoCreatedTime { get; set; }
        public string InfoUpdatedTime { get; set; }
        public string InfoAuthor { get; set; }
        public string InfoDescription { get; set; }
        public bool CaptureWearableOnly { get; set; }
        public bool CaptureRemoveBackground { get; set; }

        public bool SimpleUseArmatureMapping { get; set; }
        public bool SimpleUseMoveRoot { get; set; }
        public bool SimpleUseAnimationGeneration { get; set; }
        public bool SimpleUseBlendshapeSync { get; set; }
        public ArmatureMappingWearableModuleConfig SimpleArmatureMappingConfig { get; set; }
        public MoveRootWearableModuleConfig SimpleMoveRootConfig { get; set; }
        public AnimationGenerationWearableModuleConfig SimpleAnimationGenerationConfig { get; set; }
        public BlendshapeSyncWearableModuleConfig SimpleBlendshapeSyncConfig { get; set; }

        public List<string> AdvancedModuleNames { get; set; }
        public string AdvancedSelectedModuleName { get; set; }
        public GameObject AdvancedAvatarConfigGuidReference { get; set; }
        public string AdvancedAvatarConfigGuid { get; set; }
        public bool AdvancedAvatarConfigUseAvatarObjName { get; set; }
        public string AdvancedAvatarConfigCustomName { get; set; }
        public string AdvancedAvatarConfigArmatureName { get; set; }
        public string AdvancedAvatarConfigDeltaWorldPos { get; set; }
        public string AdvancedAvatarConfigDeltaWorldRot { get; set; }
        public string AdvancedAvatarConfigAvatarLossyScale { get; set; }
        public string AdvancedAvatarConfigWearableLossyScale { get; set; }
        public List<WearableConfigModuleViewData> AdvancedModuleViewDataList { get; set; }

        public bool ShowAvatarNoCabinetHelpBox { get; set; }
        public bool ShowArmatureNotFoundHelpBox { get; set; }
        public bool ShowArmatureGuessedHelpBox { get; set; }
        public bool ShowCabinetConfigErrorHelpBox { get; set; }

        public bool PreviewActive { get => DTEditorUtils.PreviewActive; }

        private IDressingSubView _dressingSubView;
        private WearableConfigPresenter _presenter;
        private VisualElement _capturePanel;
        private Toggle _captureWearableOnlyToggle;
        private Toggle _captureRmvBgToggle;
        private Button _captureThumbBtn;
        private Button _captureCancelBtn;
        private VisualElement _infoPanel;
        private VisualElement _infoThumbnail;
        private Label _infoWearableNameLabel;
        private TextField _infoCustomNameField;
        private Toggle _infoUseCustomNameToggle;
        private Button _infoCaptureNewThumbBtn;
        private Button[] _toolbarModeBtns;
        private int _selectedMode;
        private VisualElement _simpleContainer;
        private VisualElement _simpleHelpBoxContainer;
        private VisualElement _advancedContainer;
        private Button _toolbarPreviewBtn;
        private Button[] _simpleCategoryBtns;
        private int _selectedCategory;
        private VisualElement _simpleCategoryMappingContainer;
        private VisualElement _simpleCategoryAnimateContainer;
        private ArmatureMappingWearableModuleEditor _simpleArmatureMappingEditor;
        private MoveRootWearableModuleEditor _simpleMoveRootEditor;
        private AnimationGenerationWearableModuleEditor _simpleAnimGenEditor;
        private BlendshapeSyncWearableModuleEditor _simpleBlendshapeSyncEditor;
        private PopupField<string> _modulesPopup;
        private VisualElement _advancedModuleEditorsContainer;
        private ObjectField _advancedAvatarConfigGuidRefObjField;
        private Label _advancedAvatarConfigGuidLabel;
        private Toggle _advancedAvatarConfigUseAvatarObjNameToggle;
        private TextField _advancedAvatarConfigCustomNameField;
        private Label _advancedAvatarConfigArmatureNameLabel;
        private Label _advancedAvatarConfigDeltaWorldPosLabel;
        private Label _advancedAvatarConfigAvatarDeltaWorldPosLabel;
        private Label _advancedAvatarConfigDeltaWorldRotLabel;
        private Label _advancedAvatarConfigAvatarLossyScaleLabel;
        private Label _advancedAvatarConfigWearableLossyScaleLabel;
        private Toggle _simpleArmatureMappingToggle;
        private Toggle _simpleMoveRootToggle;
        private Toggle _simpleAnimGenToggle;
        private Toggle _simpleBlendshapeSyncToggle;
        private bool _captureActive;
        private Label _infoOthersUuidLabel;
        private Label _infoOthersCreatedTimeLabel;
        private Label _infoOthersUpdatedTimeLabel;
        private TextField _infoOthersAuthorField;
        private TextField _infoOthersDescField;
        private VisualElement _simpleArmatureMappingContainer;
        private VisualElement _simpleMoveRootContainer;
        private VisualElement _simpleAnimGenContainer;
        private VisualElement _simpleBlendshapeSyncContainer;

        public WearableConfigView(IDressingSubView dressingSubView)
        {
            _dressingSubView = dressingSubView;
            _presenter = new WearableConfigPresenter(this);

            _selectedMode = 0;
            _selectedCategory = 0;
            _captureActive = false;

            InfoThumbnail = null;
            InfoUseCustomWearableName = false;
            InfoCustomWearableName = null;
            InfoUuid = null;
            InfoCreatedTime = null;
            InfoUpdatedTime = null;
            InfoDescription = null;

            CaptureWearableOnly = true;
            CaptureRemoveBackground = true;

            SimpleUseArmatureMapping = false;
            SimpleUseMoveRoot = false;
            SimpleUseAnimationGeneration = false;
            SimpleUseBlendshapeSync = false;

            SimpleArmatureMappingConfig = new ArmatureMappingWearableModuleConfig();
            SimpleMoveRootConfig = new MoveRootWearableModuleConfig();
            SimpleAnimationGenerationConfig = new AnimationGenerationWearableModuleConfig();
            SimpleBlendshapeSyncConfig = new BlendshapeSyncWearableModuleConfig();

            AdvancedModuleNames = new List<string>() { "---" };
            AdvancedAvatarConfigGuidReference = null;
            AdvancedAvatarConfigGuid = null;
            AdvancedAvatarConfigCustomName = null;
            AdvancedAvatarConfigArmatureName = null;
            AdvancedAvatarConfigDeltaWorldPos = null;
            AdvancedAvatarConfigDeltaWorldRot = null;
            AdvancedAvatarConfigAvatarLossyScale = null;
            AdvancedAvatarConfigWearableLossyScale = null;
            AdvancedModuleViewDataList = new List<WearableConfigModuleViewData>();
            AdvancedSelectedModuleName = null;
        }

        private void InitWearableInfoInfoPanel()
        {
            _infoThumbnail = Q<VisualElement>("wearable-info-thumbnail").First();

            _infoPanel = Q<VisualElement>("wearable-info-info-panel").First();

            _infoWearableNameLabel = Q<Label>("wearable-info-name-label").First();

            _infoCustomNameField = Q<TextField>("wearable-info-custom-name-field").First();
            // TODO: due to a weird bug, we have to localize text field here
            _infoCustomNameField.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                if (InfoUseCustomWearableName)
                {
                    _infoWearableNameLabel.text = evt.newValue;
                }
                InfoCustomWearableName = _infoCustomNameField.text;
            });

            _infoUseCustomNameToggle = Q<Toggle>("wearable-info-custom-name-toggle").First();
            _infoUseCustomNameToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                if (TargetWearable != null)
                {
                    _infoCustomNameField.value = _infoWearableNameLabel.text = InfoCustomWearableName = TargetWearable.name;
                }
                _infoCustomNameField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                InfoUseCustomWearableName = evt.newValue;
            });

            _infoCaptureNewThumbBtn = Q<Button>("wearable-info-capture-new-thumbnail-btn").First();
            _infoCaptureNewThumbBtn.clicked += InfoNewThumbnailButtonClick;
        }

        private void InitWearableInfoCapturePanel()
        {
            _capturePanel = Q<VisualElement>("wearable-info-capture-panel").First();

            _captureWearableOnlyToggle = Q<Toggle>("wearable-info-capture-wearable-only-toggle").First();
            _captureRmvBgToggle = Q<Toggle>("wearable-info-capture-remove-background-toggle").First();

            _captureThumbBtn = Q<Button>("wearable-info-thumbnail-capture-btn").First();
            _captureThumbBtn.clicked += CaptureThumbnailButtonClick;
            _captureCancelBtn = Q<Button>("wearable-info-thumbnail-cancel-btn").First();
            _captureCancelBtn.clicked += CaptureCancelButtonClick;

            _captureWearableOnlyToggle.value = CaptureWearableOnly;
            _captureWearableOnlyToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                CaptureWearableOnly = evt.newValue;
                CaptureSettingsChange?.Invoke();
            });
            _captureRmvBgToggle.value = CaptureRemoveBackground;
            _captureRmvBgToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                CaptureRemoveBackground = evt.newValue;
                CaptureSettingsChange?.Invoke();
            });
        }

        private void InitWearableInfoOthersFoldout()
        {
            BindFoldoutHeaderWithContainer("wearable-info-others-foldout", "wearable-info-others-container");

            _infoOthersUuidLabel = Q<Label>("wearable-info-uuid-label").First();
            _infoOthersCreatedTimeLabel = Q<Label>("wearable-info-created-time-label").First();
            _infoOthersUpdatedTimeLabel = Q<Label>("wearable-info-updated-time-label").First();
            _infoOthersAuthorField = Q<TextField>("wearable-info-author-field").First();
            _infoOthersAuthorField.RegisterValueChangedCallback((ChangeEvent<string> evt) => InfoAuthor = evt.newValue);
            _infoOthersDescField = Q<TextField>("wearable-info-desc-field").First();
            _infoOthersDescField.RegisterValueChangedCallback((ChangeEvent<string> evt) => InfoDescription = evt.newValue);
        }

        private void InitWearableInfoFoldout()
        {
            BindFoldoutHeaderWithContainer("wearable-info-foldout", "wearable-info-container");
            InitWearableInfoInfoPanel();
            InitWearableInfoCapturePanel();
            InitWearableInfoOthersFoldout();
        }

        private void BindToolbarModes()
        {
            var simpleBtn = Q<Button>("toolbar-simple-btn");
            var advancedBtn = Q<Button>("toolbar-advanced-btn");

            _toolbarModeBtns = new Button[] { simpleBtn, advancedBtn };

            for (var i = 0; i < _toolbarModeBtns.Length; i++)
            {
                var modeIndex = i;
                _toolbarModeBtns[i].clicked += () =>
                {
                    if (_selectedMode == modeIndex) return;
                    _selectedMode = modeIndex;
                    UpdateToolbarModes();
                    ModeChange?.Invoke();
                };
                _toolbarModeBtns[i].EnableInClassList("active", modeIndex == _selectedMode);
            }
        }

        private void UpdateToolbarModes()
        {
            for (var i = 0; i < _toolbarModeBtns.Length; i++)
            {
                _toolbarModeBtns[i].EnableInClassList("active", i == _selectedMode);
            }

            if (_selectedMode == 0)
            {
                _simpleContainer.style.display = DisplayStyle.Flex;
                _advancedContainer.style.display = DisplayStyle.None;
            }
            else if (_selectedMode == 1)
            {
                _simpleContainer.style.display = DisplayStyle.None;
                _advancedContainer.style.display = DisplayStyle.Flex;
            }
        }

        private void InitToolbar()
        {
            var autoSetupBtn = Q<Button>("toolbar-auto-setup-btn").First();
            autoSetupBtn.clicked += ToolbarAutoSetupButtonClick;

            _toolbarPreviewBtn = Q<Button>("toolbar-preview-btn").First();
            _toolbarPreviewBtn.clicked += ToolbarPreviewButtonClick;

            BindToolbarModes();
        }

        private void BindSimpleCategories()
        {
            var mappingBtn = Q<Button>("simple-category-mapping-btn");
            var animateBtn = Q<Button>("simple-category-animate-btn");

            _simpleCategoryBtns = new Button[] { mappingBtn, animateBtn };

            for (var i = 0; i < _simpleCategoryBtns.Length; i++)
            {
                var categoryIndex = i;
                _simpleCategoryBtns[i].clicked += () =>
                {
                    if (_selectedCategory == categoryIndex) return;
                    _selectedCategory = categoryIndex;
                    UpdateSimpleCategories();
                };
                _simpleCategoryBtns[i].EnableInClassList("active", categoryIndex == _selectedCategory);
            }
        }

        private void UpdateSimpleCategories()
        {
            for (var i = 0; i < _simpleCategoryBtns.Length; i++)
            {
                _simpleCategoryBtns[i].EnableInClassList("active", i == _selectedCategory);
            }

            if (_selectedCategory == 0)
            {
                _simpleCategoryMappingContainer.style.display = DisplayStyle.Flex;
                _simpleCategoryAnimateContainer.style.display = DisplayStyle.None;
            }
            else if (_selectedCategory == 1)
            {
                _simpleCategoryMappingContainer.style.display = DisplayStyle.None;
                _simpleCategoryAnimateContainer.style.display = DisplayStyle.Flex;
            }
        }

        private void InitSimpleCategoryMapping()
        {
            _simpleCategoryMappingContainer = Q<VisualElement>("simple-category-mapping-container").First();

            BindFoldoutHeaderWithContainer("simple-armature-mapping-foldout", "simple-armature-mapping-container");
            _simpleArmatureMappingToggle = Q<Toggle>("simple-armature-mapping-toggle").First();
            _simpleArmatureMappingToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) => SimpleUseArmatureMapping = evt.newValue);
            _simpleArmatureMappingContainer = Q<VisualElement>("simple-armature-mapping-container").First();

            BindFoldoutHeaderWithContainer("simple-move-root-foldout", "simple-move-root-container");
            _simpleMoveRootToggle = Q<Toggle>("simple-move-root-toggle").First();
            _simpleMoveRootToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) => SimpleUseMoveRoot = evt.newValue);
            _simpleMoveRootContainer = Q<VisualElement>("simple-move-root-container").First();
        }

        private void InitSimpleCategoryAnimate()
        {
            _simpleCategoryAnimateContainer = Q<VisualElement>("simple-category-animate-container").First();

            BindFoldoutHeaderWithContainer("simple-anim-gen-foldout", "simple-anim-gen-container");
            _simpleAnimGenToggle = Q<Toggle>("simple-anim-gen-toggle").First();
            _simpleAnimGenToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) => SimpleUseAnimationGeneration = evt.newValue);
            _simpleAnimGenContainer = Q<VisualElement>("simple-anim-gen-container").First();

            BindFoldoutHeaderWithContainer("simple-blendshape-sync-foldout", "simple-blendshape-sync-container");
            _simpleBlendshapeSyncToggle = Q<Toggle>("simple-blendshape-sync-toggle").First();
            _simpleBlendshapeSyncToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) => SimpleUseBlendshapeSync = evt.newValue);
            _simpleBlendshapeSyncContainer = Q<VisualElement>("simple-blendshape-sync-container").First();
        }

        private void InitSimpleMode()
        {
            _simpleContainer = Q<VisualElement>("simple-container").First();
            _simpleHelpBoxContainer = Q<VisualElement>("simple-helpbox-container").First();

            InitSimpleCategoryMapping();
            InitSimpleCategoryAnimate();
            BindSimpleCategories();
        }

        private void InitAdvancedModules()
        {
            BindFoldoutHeaderWithContainer("advanced-modules-foldout", "advanced-modules-container");

            var popupContainer = Q<VisualElement>("advanced-modules-popup-container").First();
            _modulesPopup = new PopupField<string>(AdvancedModuleNames, 0);
            _modulesPopup.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                AdvancedSelectedModuleName = evt.newValue;
            });
            popupContainer.Add(_modulesPopup);

            var moduleAddBtn = Q<Button>("advanced-module-add-btn").First();
            moduleAddBtn.clicked += AdvancedModuleAddButtonClick;

            _advancedModuleEditorsContainer = Q<VisualElement>("advanced-modules-editors-container").First();
        }

        private void InitAdvancedAvatarConfig()
        {
            BindFoldoutHeaderWithContainer("advanced-avatar-config-foldout", "advanced-avatar-config-container");

            _advancedAvatarConfigGuidRefObjField = Q<ObjectField>("advanced-avatar-config-guid-ref-objectfield").First();
            _advancedAvatarConfigGuidRefObjField.objectType = typeof(GameObject);
            _advancedAvatarConfigGuidRefObjField.allowSceneObjects = true;
            _advancedAvatarConfigGuidRefObjField.RegisterValueChangedCallback((ChangeEvent<UnityEngine.Object> evt) =>
            {
                AdvancedAvatarConfigGuidReference = (GameObject)evt.newValue;
                AvatarConfigChange?.Invoke();
            });

            _advancedAvatarConfigGuidLabel = Q<Label>("advanced-avatar-config-guid-label").First();
            _advancedAvatarConfigUseAvatarObjNameToggle = Q<Toggle>("advanced-avatar-config-use-obj-name-toggle").First();
            _advancedAvatarConfigUseAvatarObjNameToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                AdvancedAvatarConfigUseAvatarObjName = evt.newValue;
                AvatarConfigChange?.Invoke();
            });
            _advancedAvatarConfigCustomNameField = Q<TextField>("advanced-avatar-config-custom-name-field").First();
            _advancedAvatarConfigCustomNameField.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                AdvancedAvatarConfigCustomName = evt.newValue;
                AvatarConfigChange?.Invoke();
            });
            _advancedAvatarConfigArmatureNameLabel = Q<Label>("advanced-avatar-config-armature-name-label").First();
            _advancedAvatarConfigDeltaWorldPosLabel = Q<Label>("advanced-avatar-config-delta-world-pos-label").First();
            _advancedAvatarConfigDeltaWorldRotLabel = Q<Label>("advanced-avatar-config-delta-world-rot-label").First();
            _advancedAvatarConfigAvatarLossyScaleLabel = Q<Label>("advanced-avatar-config-avatar-lossy-scale-label").First();
            _advancedAvatarConfigWearableLossyScaleLabel = Q<Label>("advanced-avatar-config-wearable-lossy-scale-label").First();
        }

        private void InitAdvancedMode()
        {
            _advancedContainer = Q<VisualElement>("advanced-container").First();

            InitAdvancedModules();
            InitAdvancedAvatarConfig();
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("WearableConfigView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("WearableConfigViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            // dummy way to repaint on interact
            RegisterCallback((MouseMoveEvent evt) =>
            {
                if (_captureActive) RepaintCapturePreview();
                _toolbarPreviewBtn.EnableInClassList("active", PreviewActive);
            });
        }

        public override void OnEnable()
        {
            InitVisualTree();
            InitWearableInfoFoldout();
            InitSimpleMode();
            InitAdvancedMode();
            InitToolbar();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _simpleArmatureMappingEditor.OnDisable();
            _simpleMoveRootEditor.OnDisable();
            _simpleAnimGenEditor.OnDisable();
            _simpleBlendshapeSyncEditor.OnDisable();
        }

        private void RepaintWearableInfo()
        {
            if (ThumbnailPlaceholderImage == null)
            {
                ThumbnailPlaceholderImage = Resources.Load<Texture2D>("thumbnailPlaceholder");
            }

            _infoThumbnail.style.backgroundImage = new StyleBackground(InfoThumbnail != null ? InfoThumbnail : ThumbnailPlaceholderImage);

            _infoUseCustomNameToggle.value = InfoUseCustomWearableName;
            if (InfoUseCustomWearableName)
            {
                _infoWearableNameLabel.text = InfoCustomWearableName;
            }
            else
            {
                _infoWearableNameLabel.text = TargetWearable != null ? TargetWearable.name : "---";
            }

            _infoOthersUuidLabel.text = InfoUuid;
            _infoOthersCreatedTimeLabel.text = InfoCreatedTime;
            _infoOthersUpdatedTimeLabel.text = InfoUpdatedTime;
            _infoOthersAuthorField.value = InfoAuthor;
            _infoOthersDescField.value = InfoDescription;
        }

        private void RepaintSimpleHelpboxes()
        {
            // add helpboxes
            _simpleHelpBoxContainer.Clear();
            if (ShowAvatarNoCabinetHelpBox)
            {
                _simpleHelpBoxContainer.Add(CreateHelpBox(t._("wearableConfig.editor.simple.autoSetup.helpbox.avatarHasNoCabinetUsingDefault"), MessageType.Warning));
            }
            if (ShowArmatureNotFoundHelpBox)
            {
                _simpleHelpBoxContainer.Add(CreateHelpBox(t._("wearableConfig.editor.simple.autoSetup.helpbox.wearableArmatureNotFound"), MessageType.Warning));
            }
            if (ShowArmatureGuessedHelpBox)
            {
                _simpleHelpBoxContainer.Add(CreateHelpBox(t._("wearableConfig.editor.simple.autoSetup.helpbox.armatureGuessed"), MessageType.Warning));
            }
            if (ShowCabinetConfigErrorHelpBox)
            {
                _simpleHelpBoxContainer.Add(CreateHelpBox(t._("dressing.editor.wizard.autoSetup.helpbox.unableToLoadCabinetConfig"), MessageType.Warning));
            }
        }

        private void RepaintSimpleModuleEditors()
        {
            _simpleArmatureMappingEditor = new ArmatureMappingWearableModuleEditor(this, null, SimpleArmatureMappingConfig);
            _simpleMoveRootEditor = new MoveRootWearableModuleEditor(this, null, SimpleMoveRootConfig);
            _simpleAnimGenEditor = new AnimationGenerationWearableModuleEditor(this, null, SimpleAnimationGenerationConfig);
            _simpleBlendshapeSyncEditor = new BlendshapeSyncWearableModuleEditor(this, null, SimpleBlendshapeSyncConfig);

            _simpleArmatureMappingEditor.OnEnable();
            _simpleMoveRootEditor.OnEnable();
            _simpleAnimGenEditor.OnEnable();
            _simpleBlendshapeSyncEditor.OnEnable();

            _simpleArmatureMappingContainer.Clear();
            _simpleMoveRootContainer.Clear();
            _simpleAnimGenContainer.Clear();
            _simpleBlendshapeSyncContainer.Clear();

            _simpleArmatureMappingContainer.Add(_simpleArmatureMappingEditor);
            _simpleMoveRootContainer.Add(_simpleMoveRootEditor);
            _simpleAnimGenContainer.Add(_simpleAnimGenEditor);
            _simpleBlendshapeSyncContainer.Add(_simpleBlendshapeSyncEditor);
        }

        public void RepaintSimpleMode()
        {
            _simpleArmatureMappingToggle.value = SimpleUseArmatureMapping;
            _simpleMoveRootToggle.value = SimpleUseMoveRoot;
            _simpleAnimGenToggle.value = SimpleUseAnimationGeneration;
            _simpleBlendshapeSyncToggle.value = SimpleUseBlendshapeSync;

            RepaintSimpleHelpboxes();
            RepaintSimpleModuleEditors();

            _simpleArmatureMappingEditor.RaiseForceUpdateViewEvent();
            _simpleMoveRootEditor.RaiseForceUpdateViewEvent();
            _simpleAnimGenEditor.RaiseForceUpdateViewEvent();
            _simpleBlendshapeSyncEditor.RaiseForceUpdateViewEvent();
        }

        private void CreateModuleViewDataFoldout(WearableConfigModuleViewData moduleViewData)
        {
            var nestedFoldoutHeader = new VisualElement();
            nestedFoldoutHeader.AddToClassList("nested-foldout-header");

            var container = new VisualElement();
            container.AddToClassList("foldout-container");
            container.Add(moduleViewData.editor);

            var foldout = new Foldout
            {
                text = moduleViewData.editor.FriendlyName
            };
            foldout.RegisterValueChangedCallback((ChangeEvent<bool> evt) => container.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
            nestedFoldoutHeader.Add(foldout);

            foldout.value = false;
            container.style.display = DisplayStyle.None;

            var rmvBtn = new Button(moduleViewData.removeButtonOnClick)
            {
                text = "X"
            };
            nestedFoldoutHeader.Add(rmvBtn);

            _advancedModuleEditorsContainer.Add(nestedFoldoutHeader);
            _advancedModuleEditorsContainer.Add(container);
        }

        public void RepaintAdvancedModeModules()
        {
            _advancedModuleEditorsContainer.Clear();

            foreach (var moduleViewData in AdvancedModuleViewDataList)
            {
                CreateModuleViewDataFoldout(moduleViewData);
            }
        }

        public void RepaintAdvancedModeAvatarConfig()
        {
            _advancedAvatarConfigGuidRefObjField.value = AdvancedAvatarConfigGuidReference;
            _advancedAvatarConfigGuidLabel.text = AdvancedAvatarConfigGuid;
            _advancedAvatarConfigUseAvatarObjNameToggle.value = AdvancedAvatarConfigUseAvatarObjName;
            _advancedAvatarConfigCustomNameField.value = AdvancedAvatarConfigCustomName;
            _advancedAvatarConfigArmatureNameLabel.text = AdvancedAvatarConfigArmatureName;
            _advancedAvatarConfigAvatarLossyScaleLabel.text = AdvancedAvatarConfigAvatarLossyScale;
            _advancedAvatarConfigWearableLossyScaleLabel.text = AdvancedAvatarConfigWearableLossyScale;
        }

        private void RepaintAdvancedMode()
        {
            RepaintAdvancedModeModules();
            RepaintAdvancedModeAvatarConfig();
        }

        public void Repaint()
        {
            RepaintWearableInfo();
            RepaintSimpleMode();
            RepaintAdvancedMode();
        }

        public void RepaintCapturePreview()
        {
            _infoThumbnail.style.backgroundImage = new StyleBackground(DTEditorUtils.GetThumbnailCameraPreview());
        }

        public void SwitchToInfoPanel()
        {
            _infoPanel.style.display = DisplayStyle.Flex;
            _capturePanel.style.display = DisplayStyle.None;
            _captureActive = false;
        }

        public void SwitchToCapturePanel()
        {
            _infoPanel.style.display = DisplayStyle.None;
            _capturePanel.style.display = DisplayStyle.Flex;
            _captureActive = true;
        }

        public void ShowModuleAddedBeforeDialog()
        {
            EditorUtility.DisplayDialog(t._("tool.name"), t._("wearableConfig.editor.dialog.msg.moduleAddedBeforeCannotMultiple"), t._("common.dialog.btn.ok"));
        }

        public bool ShowConfirmAutoSetupDialog()
        {
            return EditorUtility.DisplayDialog(t._("tool.name"), t._("wearableConfig.editor.dialog.msg.confirmAutoSetup"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no"));
        }

        public void UpdateAvatarPreview() => _presenter.UpdateAvatarPreview();

        public void AutoSetup() => _presenter.AutoSetup();

        public bool IsValid()
        {
            if (TargetAvatar == null || TargetWearable == null)
            {
                return false;
            }

            if (SelectedMode == 0)
            {
                var mappingValid = true;
                mappingValid &= !SimpleUseArmatureMapping || _simpleArmatureMappingEditor.IsValid();
                mappingValid &= !SimpleUseMoveRoot || _simpleMoveRootEditor.IsValid();
                _simpleCategoryBtns[0].EnableInClassList("invalid", !mappingValid);

                var animateValid = true;
                animateValid &= !SimpleUseAnimationGeneration || _simpleAnimGenEditor.IsValid();
                animateValid &= !SimpleUseBlendshapeSync || _simpleBlendshapeSyncEditor.IsValid();
                _simpleCategoryBtns[1].EnableInClassList("invalid", !animateValid);

                return mappingValid && animateValid;
            }
            else if (SelectedMode == 1)
            {
                return true;
            }

            return false;
        }
    }
}
