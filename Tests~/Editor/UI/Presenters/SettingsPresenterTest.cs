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
using Chocopoi.DressingTools.UI.Views;
using Moq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.UI.Presenters
{
    internal class SettingsPresenterTest : EditorTestBase
    {
        private static Mock<IToolSettingsSubView> SetupMock()
        {
            var mock = new Mock<IToolSettingsSubView>();

            mock.SetupAllProperties();

            new ToolSettingsPresenter(mock.Object);

            return mock;
        }

        private static void AssertUpdateChecker(IToolSettingsSubView view)
        {
            Assert.NotNull(UpdateChecker.CurrentVersion);
            Assert.AreEqual(UpdateChecker.CurrentVersion.fullString, view.UpdaterCurrentVersion);
        }

        private static void AssertUpdateView(Mock<IToolSettingsSubView> mock)
        {
            var view = mock.Object;
            var prefs = PreferencesUtility.GetPreferences();
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
