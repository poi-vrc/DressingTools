/*
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
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Moq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.UI.Presenters
{
    public class SettingsPresenterTest : EditorTestBase
    {
        private static Mock<ISettingsSubView> SetupMock()
        {
            var mock = new Mock<ISettingsSubView>();

            mock.SetupAllProperties();
            mock.SetupProperty(m => m.AvailableLanguageKeys, new List<string>());
            mock.SetupProperty(m => m.AvailableBranchKeys, new List<string>());

            new SettingsPresenter(mock.Object);

            return mock;
        }

        private static void AssertLanguagePopup(Preferences prefs, ISettingsSubView view)
        {
            var locales = I18n.ToolTranslator.GetAvailableLocales();
            Assert.AreEqual(locales.Length, view.AvailableLanguageKeys.Count);
            var selectedLangIndex = view.AvailableLanguageKeys.IndexOf(view.LanguageSelected);
            var expectedlangIndex = Array.IndexOf(locales, prefs.app.selectedLanguage);
            Assert.AreEqual(expectedlangIndex, selectedLangIndex);
        }

        private static void AssertCabinetDefaults(Preferences prefs, ISettingsSubView view)
        {
            Assert.AreEqual(prefs.cabinet.defaultArmatureName, view.CabinetDefaultsArmatureName);
            Assert.AreEqual(prefs.cabinet.defaultGroupDynamics, view.CabinetDefaultsGroupDynamics);
            Assert.AreEqual(prefs.cabinet.defaultGroupDynamicsSeparateDynamics, view.CabinetDefaultsSeparateDynamics);
            Assert.AreEqual(prefs.cabinet.defaultAnimationWriteDefaults, view.CabinetDefaultsAnimWriteDefaults);
        }

        private static void AssertUpdateChecker(ISettingsSubView view)
        {
            Assert.NotNull(UpdateChecker.CurrentVersion);
            Assert.AreEqual(UpdateChecker.CurrentVersion.fullVersionString, view.UpdaterCurrentVersion);
        }

        private static void AssertUpdateView(Mock<ISettingsSubView> mock)
        {
            var view = mock.Object;
            var prefs = PreferencesUtility.GetPreferences();
            AssertLanguagePopup(prefs, view);
            AssertCabinetDefaults(prefs, view);
            AssertUpdateChecker(view);
            mock.Verify(m => m.Repaint(), Times.Once);
        }

        [Test]
        public void LoadUnloadTest()
        {
            var mock = SetupMock();
            mock.Raise(m => m.Load += null);
            AssertUpdateView(mock);
            mock.Raise(m => m.Unload += null);
        }

        [Test]
        public void ForceUpdateViewTest()
        {
            var mock = SetupMock();
            mock.Raise(m => m.ForceUpdateView += null);
            AssertUpdateView(mock);
        }

        [Test]
        public void LanguageChangedTest()
        {
            var mock = SetupMock();
            mock.SetupProperty(m => m.LanguageSelected, "English");
            mock.Raise(m => m.LanguageChanged += null);
            mock.Verify(m => m.AskReloadWindow(), Times.Once);
        }

        [Test]
        public void SettingsChangedTest()
        {
            var mock = SetupMock();
            mock.Raise(m => m.SettingsChanged += null);
            // TODO: add some asserts?
        }

        [Test]
        public void UpdaterCheckUpdateButtonClickedTest()
        {
            var mock = SetupMock();
            mock.Raise(m => m.UpdaterCheckUpdateButtonClicked += null);
            // TODO: assert called update checker?
            AssertUpdateView(mock);
        }

        [Test]
        public void ResetToDefaultsButtonClickedTest()
        {
            var mock = SetupMock();
            mock.Raise(m => m.ResetToDefaultsButtonClicked += null);
            // TODO: assert reset to defaults?
            AssertUpdateView(mock);
        }
    }
}
