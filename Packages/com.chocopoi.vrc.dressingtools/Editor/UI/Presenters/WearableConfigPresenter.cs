using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class WearableConfigPresenter : IWearableConfigPresenter
    {
        private static readonly Dictionary<string, IDTDresser> dressers = new Dictionary<string, IDTDresser>()
        {
            { "Default", new DTDefaultDresser() }
        };

        private IWearableConfigView wearableConfigView;

        private DTMappingEditorContainer boneMappingEditorContainer;

        public WearableConfigPresenter(IWearableConfigView wearableConfigView)
        {
            this.wearableConfigView = wearableConfigView;
            boneMappingEditorContainer = new DTMappingEditorContainer();
            ResetMappingEditorContainer();
        }

        private void ResetMappingEditorContainer()
        {
            boneMappingEditorContainer.dresserSettings = null;
            boneMappingEditorContainer.boneMappings = null;
            boneMappingEditorContainer.objectMappings = null;
            boneMappingEditorContainer.boneMappingMode = DTWearableMappingMode.Auto;
            boneMappingEditorContainer.objectMappingMode = DTWearableMappingMode.Auto;
        }

        public string[] GetAvailableDresserKeys()
        {
            string[] dresserKeys = new string[dressers.Keys.Count];
            dressers.Keys.CopyTo(dresserKeys, 0);
            return dresserKeys;
        }

        public IDTDresser GetDresserByName(string name)
        {
            dressers.TryGetValue(name, out var dresser);
            return dresser;
        }

        public DTReport GenerateDresserMappings(IDTDresser dresser, DTDresserSettings dresserSettings)
        {
            // reset mapping editor container
            ResetMappingEditorContainer();
            boneMappingEditorContainer.dresserSettings = dresserSettings;

            // execute dresser
            var dresserReport = dresser.Execute(dresserSettings, out boneMappingEditorContainer.boneMappings, out boneMappingEditorContainer.objectMappings);

            return dresserReport;
        }

        public void StartMappingEditor()
        {
            var boneMappingEditorWindow = (DTMappingEditorWindow)EditorWindow.GetWindow(typeof(DTMappingEditorWindow));

            boneMappingEditorWindow.SetSettings(boneMappingEditorContainer);
            boneMappingEditorWindow.titleContent = new GUIContent("DT Mapping Editor");
            boneMappingEditorWindow.Show();
        }
    }
}
