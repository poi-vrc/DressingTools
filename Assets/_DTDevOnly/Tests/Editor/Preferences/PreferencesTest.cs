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
using System.Text.RegularExpressions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace Chocopoi.DressingTools.Tests.Prefs
{
    public class PreferencesTest : EditorTestBase
    {
        private const string EditorPrefsKey = "Chocopoi.DressingTools.Preferences";

        private string _backupPrefs = null;

        public override void SetUp()
        {
            base.SetUp();
            if (EditorPrefs.HasKey(EditorPrefsKey))
            {
                _backupPrefs = EditorPrefs.GetString(EditorPrefsKey);
            }
        }

        public override void TearDown()
        {
            base.TearDown();
            if (_backupPrefs != null)
            {
                EditorPrefs.SetString(EditorPrefsKey, _backupPrefs);
            }
            else
            {
                EditorPrefs.DeleteKey(EditorPrefsKey);
            }
            PreferencesUtility.ui = new PreferencesUtility.UnityEditorUI();
        }

        [Test]
        public void ResetToDefaultsTest()
        {
            var prefs = new Preferences();

            // reset to defs
            prefs.app.updateBranch = "abcdefg";
            prefs.cabinet.defaultArmatureName = "abcdefg";
            prefs.ResetToDefaults();
            Assert.AreNotEqual("abcdefg", prefs.app.updateBranch);
            Assert.AreNotEqual("abcdefg", prefs.cabinet.defaultArmatureName);
        }

        [Test]
        public void NoExistingPrefsTest()
        {
            EditorPrefs.DeleteKey(EditorPrefsKey);
            var prefs = PreferencesUtility.LoadPreferences();
            Assert.AreEqual(JsonConvert.SerializeObject(new Preferences()), JsonConvert.SerializeObject(prefs), "Should be the same as a new preferences");
        }

        [Test]
        public void IncompatiblePrefsTest()
        {
            var mock = new Mock<PreferencesUtility.IUI>();
            PreferencesUtility.ui = mock.Object;

            var expectedVersion = $"{Preferences.CurrentConfigVersion.Major + 1}.0.0";
            EditorPrefs.SetString(EditorPrefsKey, $"{{\"version\":\"{expectedVersion}\"}}");

            var prefs = PreferencesUtility.LoadPreferences();
            Assert.AreEqual(JsonConvert.SerializeObject(new Preferences()), JsonConvert.SerializeObject(prefs), "Should be the same as a new preferences");
            mock.Verify(ui => ui.ShowIncompatiblePrefsVersionUsingDefaultDialog(expectedVersion), Times.Once);
        }

        [Test]
        public void UnableToLoadPrefsTest()
        {
            var mock = new Mock<PreferencesUtility.IUI>();
            PreferencesUtility.ui = mock.Object;

            LogAssert.Expect(UnityEngine.LogType.Exception, new Regex("^JsonReaderException.+"));

            EditorPrefs.SetString(EditorPrefsKey, "abcdefg");
            var prefs = PreferencesUtility.LoadPreferences();
            Assert.AreEqual(JsonConvert.SerializeObject(new Preferences()), JsonConvert.SerializeObject(prefs), "Should be the same as a new preferences");
            mock.Verify(ui => ui.ShowUnableToLoadPrefsVersionUsingDefaultDialog(It.IsAny<Exception>()), Times.Once);
        }

        [Test]
        public void LoadPrefsTest()
        {
            var prefs = new Preferences();
            prefs.app.updateBranch = "hi";
            EditorPrefs.SetString(EditorPrefsKey, JsonConvert.SerializeObject(prefs));
            var deserialized = PreferencesUtility.LoadPreferences();
            Assert.AreEqual(JsonConvert.SerializeObject(prefs), JsonConvert.SerializeObject(deserialized));
        }

        [Test]
        public void SavePrefsTest()
        {
            EditorPrefs.SetString(EditorPrefsKey, "abcdefg");
            PreferencesUtility.SavePreferences();
            Assert.AreNotEqual("abcdefg", EditorPrefs.GetString(EditorPrefsKey, "abcdefg"));
        }
    }
}
