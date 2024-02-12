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
using Chocopoi.DressingTools.UI.Views;
using Moq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.UI.Presenters
{
    internal class LegacyPresenterTest : EditorTestBase
    {
        private static Mock<ILegacyView> SetupMock()
        {
            var mock = new Mock<ILegacyView>();

            mock.SetupAllProperties();
            new LegacyPresenter(mock.Object);

            return mock;
        }

        [Test]
        public void UseCustomArmatureNameTest()
        {
            var mock = SetupMock();
            var view = mock.Object;
            var expectedStr = "ababababa";
            view.AvatarArmatureObjectName = expectedStr;
            view.ClothesArmatureObjectName = expectedStr;
            view.UseCustomArmatureName = true;
            mock.Raise(m => m.Load += null);
            Assert.AreEqual(expectedStr, view.AvatarArmatureObjectName);
            Assert.AreEqual(expectedStr, view.ClothesArmatureObjectName);
            mock.Raise(m => m.Unload += null);
        }

        [Test]
        public void NotUseCustomArmatureNameTest()
        {
            var mock = SetupMock();
            var view = mock.Object;
            view.AvatarArmatureObjectName = "ababababa";
            view.ClothesArmatureObjectName = "ababababa";
            view.UseCustomArmatureName = false;
            mock.Raise(m => m.Load += null);
            Assert.AreEqual("Armature", view.AvatarArmatureObjectName);
            Assert.AreEqual("Armature", view.ClothesArmatureObjectName);
            mock.Raise(m => m.Unload += null);
        }
    }
}
