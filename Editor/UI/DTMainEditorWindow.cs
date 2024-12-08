/*
 * File: DTMainEditorWindow.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 12:27:04 am
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

namespace Chocopoi.DressingTools.UI
{
    [ExcludeFromCodeCoverage]
    internal class DTMainEditorWindow : EditorWindow
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;
        private static string[] s_availableLocales = null;
        private static List<string> s_localeChoices = null;

        private MainView _view;
        private PopupField<string> _languagePopup;


        [MenuItem("Tools/chocopoi/Reload Translations", false, 0)]
        public static void ReloadTranslations()
        {
            I18n.ReloadTranslations();
        }

        [MenuItem("Tools/chocopoi/DressingTools", false, 0)]
        public static void ShowWindow()
        {
            var window = (DTMainEditorWindow)GetWindow(typeof(DTMainEditorWindow));
            window.titleContent = new GUIContent(t._("tool.name"));
            window.Show();
        }

        public void SelectAvatar(GameObject avatarGameObject) => _view.SelectAvatar(avatarGameObject);

        public void StartDressing(GameObject targetOutfit, GameObject targetAvatar)
        {
            _view.SelectedTab = 1;
            _view.StartDressing(targetOutfit, targetAvatar);
        }

        private void AddLanguagePopup()
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

            var langIndex = Array.IndexOf(s_availableLocales, PreferencesUtility.GetPreferences().app.selectedLanguage);
            if (langIndex == -1)
            {
                langIndex = 0;
            }

            _languagePopup = new PopupField<string>(s_localeChoices, langIndex);
            _languagePopup.style.position = Position.Absolute;
            _languagePopup.style.top = 0;
            _languagePopup.style.right = 0;
            _languagePopup.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                var locale = s_availableLocales[_languagePopup.index];
                PreferencesUtility.GetPreferences().app.selectedLanguage = locale;
                I18nManager.Instance.SetLocale(locale);
                PreferencesUtility.SavePreferences();
                CreateNewView();
            });
            rootVisualElement.Add(_languagePopup);
        }

        private void CleanUp()
        {
            if (_view != null)
            {
                _view.OnDisable();
                if (rootVisualElement.Contains(_view))
                {
                    rootVisualElement.Remove(_view);
                }
                _view = null;
            }
        }

        private void CreateNewView()
        {
            CleanUp();
            _view = new MainView();
            rootVisualElement.Add(_view);
            _view.OnEnable();
            AddLanguagePopup();
        }

        public void OnEnable()
        {
            CreateNewView();
        }

        public void OnDisable()
        {
            _view.OnDisable();
        }
    }
}
