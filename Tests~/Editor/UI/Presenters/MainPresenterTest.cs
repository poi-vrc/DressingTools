﻿/*
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

using System.Collections.Generic;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.UI.Presenters
{
    internal class MainPresenterTest : EditorTestBase
    {
        // [Test]
        // public void UpdateAvailableUpdateButtonClickTest()
        // {
        //     UpdateChecker.InvalidateVersionCheckCache();

        //     var mock = new Mock<IMainView>();
        //     var view = mock.Object;
        //     var presenter = new MainPresenter(view);

        //     mock.Raise(m => m.Load += null);

        //     mock.Raise(m => m.UpdateAvailableUpdateButtonClick += null);
        //     mock.Verify(m => m.OpenUrl(It.IsAny<string>()), Times.Once);
        //     // TODO: do tests?

        //     mock.Raise(m => m.Unload += null);
        // }

        [Test]
        public void OnPrefabStageClosingTest()
        {
            var mock = new Mock<IMainView>();
            mock.SetupAllProperties();
            var view = mock.Object;
            new MainPresenter(view);
            view.AvailableAvatars = new List<GameObject>();

            mock.Raise(m => m.Load += null);
            mock.Raise(m => m.PrefabStageClosing += null, (object)null);
            mock.VerifySet(m => m.ShowExitPrefabModeHelpbox = false);
            mock.Verify(m => m.Repaint(), Times.AtLeastOnce());
            mock.Raise(m => m.Unload += null);
        }

        [Test]
        public void OnPrefabStageOpenedTest()
        {
            var mock = new Mock<IMainView>();
            mock.SetupAllProperties();
            var view = mock.Object;
            new MainPresenter(view);
            view.AvailableAvatars = new List<GameObject>();

            mock.Raise(m => m.Load += null);
            mock.Raise(m => m.PrefabStageOpened += null, (object)null);
            mock.VerifySet(m => m.ShowExitPrefabModeHelpbox = true);
            mock.Verify(m => m.Repaint(), Times.AtLeastOnce());
            mock.Raise(m => m.Unload += null);
        }
    }
}
