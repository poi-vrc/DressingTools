using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal interface IWearableSetupWizardView : IEditorView, IModuleEditorViewParent
    {
        event Action PreviousButtonClick;
        event Action NextButtonClick;

        DTWearableConfig Config { get; set; }
        ArmatureMappingModule ArmatureMappingModule { get; set; }
        MoveRootModule MoveRootModule { get; set; }
        AnimationGenerationModule AnimationGenerationModule { get; set; }
        BlendshapeSyncModule BlendshapeSyncModule { get; set; }
        bool UseArmatureMapping { get; set; }
        bool UseMoveRoot { get; set; }
        bool UseAnimationGeneration { get; set; }
        bool UseBlendshapeSync { get; set; }
        int CurrentStep { get; set; }

        void RaiseDoAddToCabinetEvent();
        bool IsValid();
    }
}
