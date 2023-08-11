using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.UI.View;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    internal class DTMappingEditorContainer
    {
        public GameObject targetAvatar;
        public GameObject targetWearable;
        public DTBoneMappingMode boneMappingMode;
        public List<DTBoneMapping> generatedBoneMappings;
        public List<DTBoneMapping> outputBoneMappings;

        public DTMappingEditorContainer()
        {
            targetAvatar = null;
            targetWearable = null;
            boneMappingMode = DTBoneMappingMode.Auto;
            generatedBoneMappings = null;
            outputBoneMappings = null;
        }
    }

    internal class DTMappingEditorWindow : EditorWindow
    {
        private MappingEditorView _view;
        private DTMappingEditorContainer _container;

        public DTMappingEditorWindow()
        {
            _view = new MappingEditorView();
            _container = null;
        }

        public void SetContainer(DTMappingEditorContainer container)
        {
            _container = container;
            _view.SetContainer(container);
        }

        public void OnEnable()
        {
            _view.OnEnable();
        }

        public void OnDisable()
        {
            _view.OnDisable();
        }

        public void OnGUI()
        {
            if (_container == null)
            {
                Close();
            }

            _view.OnGUI();
        }
    }
}
