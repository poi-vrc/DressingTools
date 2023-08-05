using System;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.UIBase;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class WearableSetupWizardView : EditorViewBase, IWearableSetupWizardView
    {
        public event Action TargetAvatarOrWearableChange { add { _dressingSubView.TargetAvatarOrWearableChange += value; } remove { _dressingSubView.TargetAvatarOrWearableChange -= value; } }
        public event Action PreviousButtonClick;
        public event Action NextButtonClick;

        public ArmatureMappingModule ArmatureMappingModule { get; set; }
        public MoveRootModule MoveRootModule { get; set; }
        public AnimationGenerationModule AnimationGenerationModule { get; set; }
        public BlendshapeSyncModule BlendshapeSyncModule { get; set; }
        public GameObject TargetAvatar { get => _dressingSubView.TargetAvatar; set => _dressingSubView.TargetAvatar = value; }
        public GameObject TargetWearable { get => _dressingSubView.TargetWearable; set => _dressingSubView.TargetWearable = value; }
        public DTWearableConfig Config { get => _dressingSubView.Config; set => _dressingSubView.Config = value; }
        public bool UseArmatureMapping { get => _useArmatureMapping; set => _useArmatureMapping = value; }
        public bool UseMoveRoot { get => _useMoveRoot; set => _useMoveRoot = value; }
        public bool UseAnimationGeneration { get => _useAnimationGeneration; set => _useAnimationGeneration = value; }
        public bool UseBlendshapeSync { get => _useBlendshapeSync; set => _useBlendshapeSync = value; }
        public int CurrentStep { get => _currentStep; set => _currentStep = value; }

        private WearableSetupWizardPresenter _presenter;
        private IDressingSubView _dressingSubView;
        private int _currentStep;
        private bool _useArmatureMapping;
        private bool _useMoveRoot;
        private bool _useAnimationGeneration;
        private bool _useBlendshapeSync;
        private ArmatureMappingModuleEditor _armatureMappingModuleEditor;
        private MoveRootModuleEditor _moveRootModuleEditor;
        private AnimationGenerationModuleEditor _animationGenerationModuleEditor;
        private BlendshapeSyncModuleEditor _blendshapeSyncModuleEditor;
        private bool _foldoutArmatureMapping;
        private bool _foldoutMoveRoot;

        public WearableSetupWizardView(IDressingSubView dressingSubView)
        {
            _dressingSubView = dressingSubView;
            _presenter = new WearableSetupWizardPresenter(this);

            ArmatureMappingModule = new ArmatureMappingModule();
            MoveRootModule = new MoveRootModule();
            AnimationGenerationModule = new AnimationGenerationModule();
            BlendshapeSyncModule = new BlendshapeSyncModule();
        }

        public bool IsValid()
        {
            if (TargetAvatar == null || TargetWearable == null)
            {
                return false;
            }

            var valid = true;

            valid &= !_useArmatureMapping || _armatureMappingModuleEditor.IsValid();
            valid &= !_useMoveRoot || _moveRootModuleEditor.IsValid();
            valid &= !_useAnimationGeneration || _animationGenerationModuleEditor.IsValid();
            valid &= !_useBlendshapeSync || _blendshapeSyncModuleEditor.IsValid();

            return valid;
        }

        public void RaiseDoAddToCabinetEvent()
        {
            _dressingSubView.RaiseDoAddToCabinetEvent();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _armatureMappingModuleEditor?.OnEnable();
            _moveRootModuleEditor?.OnEnable();
            _animationGenerationModuleEditor?.OnEnable();
            _blendshapeSyncModuleEditor?.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            _armatureMappingModuleEditor?.OnDisable();
            _moveRootModuleEditor?.OnDisable();
            _animationGenerationModuleEditor?.OnDisable();
            _blendshapeSyncModuleEditor?.OnDisable();
        }

        private void InitializeModuleEditorsIfNeeded()
        {
            if (_armatureMappingModuleEditor == null)
            {
                _armatureMappingModuleEditor = new ArmatureMappingModuleEditor(this, ArmatureMappingModule);
                _armatureMappingModuleEditor.OnEnable();
            }

            if (_moveRootModuleEditor == null)
            {
                _moveRootModuleEditor = new MoveRootModuleEditor(this, MoveRootModule);
                _moveRootModuleEditor.OnEnable();
            }

            if (_animationGenerationModuleEditor == null)
            {
                _animationGenerationModuleEditor = new AnimationGenerationModuleEditor(this, AnimationGenerationModule);
                _animationGenerationModuleEditor.OnEnable();
            }

            if (_blendshapeSyncModuleEditor == null)
            {
                _blendshapeSyncModuleEditor = new BlendshapeSyncModuleEditor(this, BlendshapeSyncModule);
                _blendshapeSyncModuleEditor.OnEnable();
            }
        }

        private void DrawMappingStep()
        {
            ToggleLeft("Perform armature bone mapping", ref _useArmatureMapping);

            BeginDisabled(!_useArmatureMapping);
            {
                BeginFoldoutBox(ref _foldoutArmatureMapping, "Settings");
                if (_foldoutArmatureMapping)
                {
                    _armatureMappingModuleEditor.OnGUI();
                }
                EndFoldoutBox();
            }
            EndDisabled();

            Separator();

            ToggleLeft("Move wearable root to avatar object", ref _useMoveRoot);

            BeginDisabled(!_useMoveRoot);
            {
                BeginFoldoutBox(ref _foldoutMoveRoot, "Settings");
                if (_foldoutMoveRoot)
                {
                    _moveRootModuleEditor.OnGUI();
                }
                EndFoldoutBox();
            }
            EndDisabled();
        }

        private void DrawAnimateStep()
        {
            ToggleLeft("Enable animation generation", ref _useAnimationGeneration);

            BeginDisabled(!_useAnimationGeneration);
            {
                _animationGenerationModuleEditor.OnGUI();
            }
            EndDisabled();

            Separator();

            ToggleLeft("Enable blendshape sync", ref _useBlendshapeSync);

            BeginDisabled(!_useBlendshapeSync);
            {
                _blendshapeSyncModuleEditor.OnGUI();
            }
            EndDisabled();
        }

        public override void OnGUI()
        {
            InitializeModuleEditorsIfNeeded();

            Toolbar(ref _currentStep, new string[] { " 1.\nMapping", "2.\nAnimate", "3.\nIntegrate", "4.\nOptimize" });

            Separator();

            BeginHorizontal();
            {
                BeginDisabled(CurrentStep == 0);
                {
                    Button("< Previous", PreviousButtonClick);
                }
                EndDisabled();
                GUILayout.FlexibleSpace();
                Button(CurrentStep == 3 ? "Finish!" : "Next >", NextButtonClick);
            }
            EndHorizontal();

            HorizontalLine();

            if (_currentStep == 0)
            {
                DrawMappingStep();
            }
            else if (_currentStep == 1)
            {
                DrawAnimateStep();
            }
            else if (_currentStep == 2)
            {
                HelpBox("Integrations wizard not implemented", MessageType.Info);
            }
            else if (_currentStep == 3)
            {
                HelpBox("Optimization wizard not implemented", MessageType.Info);
            }
        }
    }
}
