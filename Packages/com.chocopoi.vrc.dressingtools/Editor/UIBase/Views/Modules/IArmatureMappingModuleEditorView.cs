using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Dresser;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal class ReportData
    {
        public List<string> errorMsgs;
        public List<string> warnMsgs;
        public List<string> infoMsgs;

        public ReportData()
        {
            errorMsgs = new List<string>();
            warnMsgs = new List<string>();
            infoMsgs = new List<string>();
        }
    }

    internal interface IArmatureMappingModuleEditorView : IEditorView
    {
        event Action TargetAvatarOrWearableChange;
        event Action DresserChange;
        event Action ModuleSettingsChange;
        event Action DresserSettingsChange;
        event Action RegenerateMappingsButtonClick;
        event Action ViewEditMappingsButtonClick;

        ReportData DresserReportData { get; set; }
        DTDresserSettings DresserSettings { get; set; }
        string[] AvailableDresserKeys { get; set; }
        int SelectedDresserIndex { get; set; }
        bool IsAvatarAssociatedWithCabinet { get; set; }
        string AvatarArmatureName { get; set; }
        bool RemoveExistingPrefixSuffix { get; set; }
        bool GroupBones { get; set; }
    }
}
