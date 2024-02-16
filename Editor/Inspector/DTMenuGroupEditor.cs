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
using Chocopoi.DressingTools.Inspector.Views;
using Chocopoi.DressingTools.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Inspector
{
    [ExecuteInEditMode]
    [ExcludeFromCodeCoverage]
    [CustomEditor(typeof(DTMenuGroup))]
    internal class DTMenuGroupEditor : Editor
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private MenuGroupView _view;

        public override VisualElement CreateInspectorGUI()
        {
            return _view;
        }

        public void OnEnable()
        {
            _view = new MenuGroupView() { Target = (DTMenuGroup)target };
            _view.OnEnable();
        }

        public void OnDisable()
        {
            _view?.OnDisable();
            _view = null;
        }
    }
}
