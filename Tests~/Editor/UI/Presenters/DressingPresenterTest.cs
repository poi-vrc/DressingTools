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

using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Cabinet;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingFramework.Wearable;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Moq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.UI.Presenters
{
    public class DressingPresenterTest : EditorTestBase
    {
        private static Mock<IDressingSubView> SetupMock()
        {
            var mock = new Mock<IDressingSubView>();

            mock.SetupAllProperties();
            new DressingPresenter(mock.Object);

            return mock;
        }

        private static void AssertUpdateView(Mock<IDressingSubView> mock, bool hasCabinet)
        {
            var view = mock.Object;
            Assert.AreEqual(view.DisableAddToCabinetButton, !hasCabinet);
            mock.Verify(m => m.Repaint(), Times.Once);
        }

        [Test]
        public void UpdateViewCabinetExistTest()
        {
            var mock = SetupMock();

            var cabinetAvatar = CreateGameObject("CabinetAvatar");
            DKEditorUtils.GetAvatarCabinet(cabinetAvatar, true);
            mock.SetupProperty(m => m.TargetAvatar, cabinetAvatar);

            mock.Raise(m => m.Load += null);
            AssertUpdateView(mock, true);
            mock.Raise(m => m.Unload += null);
        }

        [Test]
        public void UpdateViewCabinetNotExistTest()
        {
            var mock = SetupMock();

            var noCabinetAvatar = CreateGameObject("NoCabinetAvatar");
            mock.SetupProperty(m => m.TargetAvatar, noCabinetAvatar);

            mock.Raise(m => m.Load += null);
            AssertUpdateView(mock, false);
            mock.Raise(m => m.Unload += null);
        }

        [Test]
        public void TargetAvatarOrWearableChangeNoExistingConfigTest()
        {
            var mock = SetupMock();

            var avatarObj = CreateGameObject("Avatar");
            var cabinet = DKEditorUtils.GetAvatarCabinet(avatarObj, true);
            mock.SetupProperty(m => m.TargetAvatar, avatarObj);

            var wearableObj = CreateGameObject("Wearable", avatarObj.transform);
            mock.SetupProperty(m => m.TargetWearable, wearableObj);

            mock.Raise(m => m.TargetAvatarOrWearableChange += null);

            var view = mock.Object;
            Assert.NotNull(view.Config);
            mock.Verify(m => m.ForceUpdateConfigView(), Times.Once);
            mock.Verify(m => m.AutoSetup(), Times.Once);
        }

        [Test]
        public void TargetAvatarOrWearableChangeExistingConfigTest()
        {
            var mock = SetupMock();

            var avatarObj = CreateGameObject("Avatar");
            var cabinet = DKEditorUtils.GetAvatarCabinet(avatarObj, true);
            mock.SetupProperty(m => m.TargetAvatar, avatarObj);

            var wearableObj = CreateGameObject("Wearable", avatarObj.transform);
            var config = new WearableConfig();
            cabinet.AddWearable(config, wearableObj);
            mock.SetupProperty(m => m.TargetWearable, wearableObj);

            mock.Raise(m => m.TargetAvatarOrWearableChange += null);

            var view = mock.Object;
            Assert.NotNull(view.Config);
            Assert.AreEqual(WearableConfigUtility.Serialize(config), WearableConfigUtility.Serialize(view.Config));
            mock.VerifySet(m => m.SelectedDressingMode = 1);
            mock.Verify(m => m.ForceUpdateConfigView(), Times.Once);
        }

        [Test]
        public void AddToCabinetButtonClickInvalidEditorConfigTest()
        {
            var mock = SetupMock();
            mock.Setup(m => m.IsConfigValid()).Returns(false);

            mock.Raise(m => m.AddToCabinetButtonClick += null);
            mock.Verify(m => m.ShowFixAllInvalidConfig(), Times.Once);
        }

        [Test]
        public void AddToCabinetButtonClickValidEditorConfigTest()
        {
            var mock = SetupMock();

            mock.SetupProperty(m => m.Config, new WearableConfig());

            var avatarObj = CreateGameObject("Avatar");
            var cabinet = DKEditorUtils.GetAvatarCabinet(avatarObj, true);
            cabinet.configJson = CabinetConfigUtility.Serialize(new CabinetConfig());
            mock.SetupProperty(m => m.TargetAvatar, avatarObj);

            var wearableObj = CreateGameObject("Wearable");
            mock.SetupProperty(m => m.TargetWearable, wearableObj);

            mock.Setup(m => m.IsConfigValid()).Returns(true);

            mock.Raise(m => m.AddToCabinetButtonClick += null);

            mock.Verify(m => m.ApplyToConfig(), Times.Once);
            Assert.AreEqual(avatarObj.transform, wearableObj.transform.parent);
            mock.Verify(m => m.ResetWizardAndConfigView(), Times.Once);
            mock.Verify(m => m.SelectTab(0), Times.Once);
            mock.Verify(m => m.ForceUpdateCabinetSubView(), Times.Once);

            // TODO: implement config validation and do checking here
        }

        [Test]
        public void ForceUpdateViewTest()
        {
            var mock = SetupMock();

            var avatar = CreateGameObject("Avatar");
            DKEditorUtils.GetAvatarCabinet(avatar, true);
            mock.SetupProperty(m => m.TargetAvatar, avatar);

            mock.Raise(m => m.ForceUpdateView += null);
            AssertUpdateView(mock, true);
        }
    }
}
