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

        private DTMappingEditorContainer mappingEditorContainer;

        public WearableConfigPresenter(IWearableConfigView wearableConfigView)
        {
            this.wearableConfigView = wearableConfigView;
            mappingEditorContainer = new DTMappingEditorContainer();
            ResetMappingEditorContainer();
        }

        public DTMappingEditorContainer GetMappingEditorContainer()
        {
            return mappingEditorContainer;
        }

        private void ResetMappingEditorContainer()
        {
            mappingEditorContainer.dresserSettings = null;
            mappingEditorContainer.boneMappings = null;
            mappingEditorContainer.objectMappings = null;
            mappingEditorContainer.boneMappingMode = DTWearableMappingMode.Auto;
            mappingEditorContainer.objectMappingMode = DTWearableMappingMode.Auto;
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

        public IDTDresser GetDresserByTypeName(string name)
        {
            foreach (var dresser in dressers.Values)
            {
                var type = dresser.GetType();
                if (name == type.FullName || name == type.Name)
                {
                    return dresser;
                }
            }
            return null;
        }

        public DTReport GenerateDresserMappings(IDTDresser dresser, DTDresserSettings dresserSettings)
        {
            // reset mapping editor container
            ResetMappingEditorContainer();
            mappingEditorContainer.dresserSettings = dresserSettings;

            // execute dresser
            var dresserReport = dresser.Execute(dresserSettings, out mappingEditorContainer.boneMappings, out mappingEditorContainer.objectMappings);

            return dresserReport;
        }

        public void StartMappingEditor()
        {
            var boneMappingEditorWindow = (DTMappingEditorWindow)EditorWindow.GetWindow(typeof(DTMappingEditorWindow));

            boneMappingEditorWindow.SetSettings(mappingEditorContainer);
            boneMappingEditorWindow.titleContent = new GUIContent("DT Mapping Editor");
            boneMappingEditorWindow.Show();
        }
    }
}
