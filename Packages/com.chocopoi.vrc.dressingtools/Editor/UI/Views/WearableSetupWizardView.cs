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

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class WearableSetupWizardView : EditorViewBase, IWearableSetupWizardView
    {
        private static readonly Color PreviewButtonActiveColour = new Color(0.5f, 1, 0.5f, 1);

        public event Action TargetAvatarOrWearableChange { add { _dressingSubView.TargetAvatarOrWearableChange += value; } remove { _dressingSubView.TargetAvatarOrWearableChange -= value; } }
        public event Action PreviousButtonClick;
        public event Action NextButtonClick;
        public event Action PreviewButtonClick;

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
        public bool UseArmatureMapping { get => _useArmatureMapping; set => _useArmatureMapping = value; }
        public bool UseMoveRoot { get => _useMoveRoot; set => _useMoveRoot = value; }
        public bool UseAnimationGeneration { get => _useAnimationGeneration; set => _useAnimationGeneration = value; }
        public bool UseBlendshapeSync { get => _useBlendshapeSync; set => _useBlendshapeSync = value; }
        public int CurrentStep { get => _currentStep; set => _currentStep = value; }
        public bool ShowAvatarNoCabinetHelpBox { get; set; }
        public bool ShowArmatureNotFoundHelpBox { get; set; }
        public bool ShowArmatureGuessedHelpBox { get; set; }
        public bool ShowCabinetConfigErrorHelpBox { get; set; }
        public bool PreviewActive => DTEditorUtils.PreviewActive;

        private WearableSetupWizardPresenter _presenter;
        private IDressingSubView _dressingSubView;
        private int _currentStep;
        private bool _useArmatureMapping;
        private bool _useMoveRoot;
        private bool _useAnimationGeneration;
        private bool _useBlendshapeSync;
        private bool _foldoutArmatureMapping;
        private bool _foldoutMoveRoot;

        public WearableSetupWizardView(IDressingSubView dressingSubView)
        {
            _dressingSubView = dressingSubView;
            _presenter = new WearableSetupWizardPresenter(this);

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

        public bool IsValid()
        {
            if (TargetAvatar == null || TargetWearable == null)
            {
                return false;
            }

            var valid = true;

            valid &= !_useArmatureMapping || ArmatureMappingModuleEditor.IsValid();
            valid &= !_useMoveRoot || MoveRootModuleEditor.IsValid();
            valid &= !_useAnimationGeneration || AnimationGenerationModuleEditor.IsValid();
            valid &= !_useBlendshapeSync || BlendshapeSyncModuleEditor.IsValid();

            return valid;
        }

        public void RaiseDoAddToCabinetEvent()
        {
            _dressingSubView.RaiseDoAddToCabinetEvent();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            ArmatureMappingModuleEditor.OnEnable();
            MoveRootModuleEditor.OnEnable();
            AnimationGenerationModuleEditor.OnEnable();
            BlendshapeSyncModuleEditor.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            ArmatureMappingModuleEditor.OnDisable();
            MoveRootModuleEditor.OnDisable();
            AnimationGenerationModuleEditor.OnDisable();
            BlendshapeSyncModuleEditor.OnDisable();
        }

        private void DrawMappingStep()
        {
            ToggleLeft("Perform armature bone mapping", ref _useArmatureMapping);

            BeginDisabled(!_useArmatureMapping);
            {
                BeginFoldoutBox(ref _foldoutArmatureMapping, "Settings");
                if (_foldoutArmatureMapping)
                {
                    ArmatureMappingModuleEditor.OnGUI();
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
                    MoveRootModuleEditor.OnGUI();
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
                AnimationGenerationModuleEditor.OnGUI();
            }
            EndDisabled();

            Separator();

            ToggleLeft("Enable blendshape sync", ref _useBlendshapeSync);

            BeginDisabled(!_useBlendshapeSync);
            {
                BlendshapeSyncModuleEditor.OnGUI();
            }
            EndDisabled();
        }

        private void PreviewButton()
        {
            if (PreviewActive) GUI.backgroundColor = PreviewButtonActiveColour;
            Button("Preview", PreviewButtonClick, GUILayout.ExpandWidth(false));
            GUI.backgroundColor = Color.white;
        }

        public override void OnGUI()
        {
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
                PreviewButton();
                Button(CurrentStep == 3 ? "Finish!" : "Next >", NextButtonClick);
            }
            EndHorizontal();

            HorizontalLine();

            if (_currentStep == 0)
            {
                if (ShowCabinetConfigErrorHelpBox)
                {
                    HelpBox("Auto-setup: Unable to load cabinet config!", MessageType.Error);
                }
                if (ShowAvatarNoCabinetHelpBox)
                {
                    HelpBox("Auto-setup: Selected avatar has no cabinet, using default settings", MessageType.Warning);
                }
                if (ShowArmatureNotFoundHelpBox)
                {
                    HelpBox("Auto-setup: Wearable armature not found. Armature mapping is not enabled.", MessageType.Warning);
                }
                if (ShowArmatureGuessedHelpBox)
                {
                    HelpBox("Auto-setup: Wearable armature was guessed.", MessageType.Warning);
                }
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

        public void UpdateAvatarPreview() => _presenter.UpdateAvatarPreview();
    }
}
