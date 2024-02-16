/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Localization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Inspector
{
    [ExecuteInEditMode]
    [ExcludeFromCodeCoverage]
    [CustomEditor(typeof(DTMenuInstall))]
    internal class DTMenuInstallEditor : Editor
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();

            var iconStyleSheet = Resources.Load<StyleSheet>("DTIconStyles");
            if (!rootElement.styleSheets.Contains(iconStyleSheet))
            {
                rootElement.styleSheets.Add(iconStyleSheet);
            }

            var icon = new VisualElement();
            icon.AddToClassList("dt-inspector-icon");
            rootElement.Add(icon);

#if DT_VRCSDK3A
            var vrcSourceMenuObjField = new ObjectField(t._("inspector.menu.install.objectField.sourceVrcMenu"))
            {
                objectType = typeof(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu),
                bindingPath = "m_VRCSourceMenu"
            };
            rootElement.Add(vrcSourceMenuObjField);
#endif
            var installPathField = new TextField(t._("inspector.menu.install.textField.installPath"))
            {
                bindingPath = "m_InstallPath"
            };
            rootElement.Add(installPathField);

            return rootElement;
        }
    }
}
