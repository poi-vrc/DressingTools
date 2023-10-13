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

using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Moq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.UI.Presenters
{
    public class MainPresenterTest : EditorTestBase
    {
        [Test]
        public void UpdateAvailableUpdateButtonClickTest()
        {
            UpdateChecker.FetchOnlineVersion();

            var mock = new Mock<IMainView>();
            var view = mock.Object;
            var presenter = new MainPresenter(view);

            mock.Raise(m => m.Load += null);

            mock.Raise(m => m.UpdateAvailableUpdateButtonClick += null);
            mock.Verify(m => m.OpenUrl(It.IsAny<string>()), Times.Once);
            // TODO: do tests?

            mock.Raise(m => m.Unload += null);
        }
    }
}
