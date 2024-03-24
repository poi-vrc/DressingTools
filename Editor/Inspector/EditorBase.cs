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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Inspector
{
    [ExcludeFromCodeCoverage]
    internal abstract class EditorBase : Editor
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;
        private static string[] s_availableLocales = null;
        private static List<string> s_localeChoices = null;

        private VisualElement _viewContainer;
        private ElementView _view;
        private PopupField<string> _languagePopup;

        public EditorBase()
        {
            var prefs = PreferencesUtility.GetPreferences();
            I18nManager.Instance.SetLocale(prefs.app.selectedLanguage);
        }

        private static void AddIcon(VisualElement elem)
        {
            var iconStyleSheet = Resources.Load<StyleSheet>("DTIconStyles");
            if (!elem.styleSheets.Contains(iconStyleSheet))
            {
                elem.styleSheets.Add(iconStyleSheet);
            }

            var icon = new VisualElement();
            icon.AddToClassList("dt-inspector-icon");
            elem.Add(icon);
        }

        private void AddLanguagePopup(VisualElement elem)
        {
            if (s_availableLocales == null || s_localeChoices == null)
            {
                s_availableLocales = t.GetAvailableLocales();
                s_localeChoices = new List<string>();
                foreach (var locale in s_availableLocales)
                {
                    s_localeChoices.Add(new CultureInfo(locale).NativeName);
                }
            }

            var container = new VisualElement();
            container.style.marginTop = 8;
            container.style.flexDirection = FlexDirection.Row;
            elem.Add(container);

            var placeholder = new VisualElement();
            placeholder.style.flexGrow = 1;
            container.Add(placeholder);

            var langIndex = Array.IndexOf(s_availableLocales, PreferencesUtility.GetPreferences().app.selectedLanguage);
            if (langIndex == -1)
            {
                langIndex = 0;
            }

            _languagePopup = new PopupField<string>(s_localeChoices, langIndex);
            _languagePopup.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                var locale = s_availableLocales[_languagePopup.index];
                PreferencesUtility.GetPreferences().app.selectedLanguage = locale;
                I18nManager.Instance.SetLocale(locale);
                PreferencesUtility.SavePreferences();
                CreateNewView();
            });
            container.Add(_languagePopup);
        }

        private void CleanUp()
        {
            if (_view != null)
            {
                _view.OnDisable();
                if (_viewContainer != null && _viewContainer.Contains(_view))
                {
                    _viewContainer.Remove(_view);
                }
                _view = null;
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            CleanUp();

            var rootElement = new VisualElement();
            AddIcon(rootElement);

            _viewContainer = new VisualElement();
            rootElement.Add(_viewContainer);

            AddLanguagePopup(rootElement);
            CreateNewView();
            return rootElement;
        }

        private void CreateNewView()
        {
            CleanUp();

            _view = CreateView();
            _viewContainer.Add(_view);
            _view.OnEnable();
            _view.Bind(new SerializedObject(target));
        }

        public abstract ElementView CreateView();

        public void OnDisable()
        {
            _view?.Unbind();
            _view?.OnDisable();
            _view = null;
        }
    }
}
