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

using System.Linq;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Cabinet;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Passes.Modifiers;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Passes.Cabinet
{
    internal class WardrobePassTest : EditorTestBase
    {
        [Test]
        public void FirstWardrobeOnlyTest()
        {
            var avatar = CreateGameObject("Avatar");
            var a = CreateGameObject("A", avatar.transform);
            var b = CreateGameObject("B", avatar.transform);
            a.AddComponent<DTWardrobe>();
            b.AddComponent<DTWardrobe>();

            var pass = new WardrobePass();
            var ctx = new DKNativeContext(avatar);
            Assert.True(pass.Invoke(ctx));

            // expect only A will automatically create a base outfit
            Assert.NotNull(a.transform.Find("Base"));
            Assert.Null(b.transform.Find("Base"));
        }

        [Test]
        public void UseAsMenuGroupTest()
        {
            var avatar = CreateGameObject("Avatar");
            var wardrobeObj = CreateGameObject("Wardrobe", avatar.transform);
            var wardrobe = wardrobeObj.AddComponent<DTWardrobe>();
            wardrobe.UseAsMenuGroup = true;
            wardrobe.GenerateSubMenuItem = true;
            wardrobe.MenuInstallPath = "SomePath";

            var pass = new WardrobePass();
            var ctx = new DKNativeContext(avatar);
            Assert.True(pass.Invoke(ctx));

            Assert.True(wardrobe.TryGetComponent<DTMenuGroup>(out _));
            Assert.True(wardrobe.TryGetComponent<DTMenuItem>(out var comp));
            Assert.AreEqual(DTMenuItem.ItemType.SubMenu, comp.Type);
            Assert.AreEqual(DTMenuItem.ItemSubMenuType.Children, comp.SubMenuType);
        }

        [Test]
        public void UseDKMenuAPITest()
        {
            var avatar = CreateGameObject("Avatar");
            var wardrobeObj = CreateGameObject("Wardrobe", avatar.transform);
            var wardrobe = wardrobeObj.AddComponent<DTWardrobe>();
            wardrobe.UseAsMenuGroup = false;

            var pass = new WardrobePass();
            var ctx = new DKNativeContext(avatar);
            Assert.True(pass.Invoke(ctx));

            Assert.False(wardrobe.TryGetComponent<DTMenuGroup>(out _));
            Assert.False(wardrobe.TryGetComponent<DTMenuItem>(out _));
        }

        [Test]
        public void CreateBaseOutfitIfNotExistTest()
        {
            var avatar = CreateGameObject("Avatar");
            var wardrobeObj = CreateGameObject("Wardrobe", avatar.transform);
            var wardrobe = wardrobeObj.AddComponent<DTWardrobe>();

            var outfit1 = CreateGameObject("Outfit1", wardrobe.transform);
            outfit1.AddComponent<DTAlternateOutfit>();
            var outfit2 = CreateGameObject("Outfit2", wardrobe.transform);
            outfit2.AddComponent<DTAlternateOutfit>();

            var pass = new WardrobePass();
            var ctx = new DKNativeContext(avatar);
            Assert.True(pass.Invoke(ctx));

            // expects have three outfits and the base is added to the top
            Assert.AreEqual(3, wardrobe.transform.childCount);
            Assert.AreEqual("Base", wardrobe.transform.GetChild(0).name);
        }

        [Test]
        public void MultipleOutfitsTest()
        {
            var avatar = CreateGameObject("Avatar");

            var wardrobeObj = CreateGameObject("Wardrobe", avatar.transform);
            var wardrobe = wardrobeObj.AddComponent<DTWardrobe>();
            wardrobe.ResetControlsOnSwitch = true;

            var baseOutfitObj = CreateGameObject("BaseOutfit", wardrobe.transform);
            var baseOutfit = baseOutfitObj.AddComponent<DTBaseOutfit>();
            var outfit1Obj = CreateGameObject("Outfit1", wardrobe.transform);
            var outfit1 = outfit1Obj.AddComponent<DTAlternateOutfit>();
            var outfit2Obj = CreateGameObject("Outfit2", wardrobe.transform);
            var outfit2 = outfit2Obj.AddComponent<DTAlternateOutfit>();

            // add outfit group for testing sub-menus
            var outfitGroupObj = CreateGameObject("OutfitGroup", wardrobe.transform);
            outfitGroupObj.AddComponent<DTOutfitGroup>();
            var outfit3Obj = CreateGameObject("Outfit3", outfitGroupObj.transform);
            var outfit3 = outfit3Obj.AddComponent<DTAlternateOutfit>();
            var outfit4Obj = CreateGameObject("Outfit4", outfitGroupObj.transform);
            var outfit4 = outfit4Obj.AddComponent<DTAlternateOutfit>();

            // add a custom menu group for outfit 2
            var menuGroupObj = CreateGameObject("MenuGroup", outfit2.transform);
            var menuGroup = menuGroupObj.AddComponent<DTMenuGroup>();
            outfit2.MenuGroup = menuGroup;
            // add a dummy smartcontrol inside the menu group
            var menuGroupCtrlObj = CreateGameObject("MenuGroupCtrl", menuGroup.transform);
            var menuGroupCtrl = menuGroupCtrlObj.AddComponent<DTSmartControl>();
            menuGroupCtrl.DriverType = DTSmartControl.SCDriverType.MenuItem;

            // add group dyn for base, outfit1, outfit2
            var baseGroupDynObj = CreateGameObject("GroupDyn", baseOutfit.transform);
            var baseGroupDyn = baseGroupDynObj.AddComponent<DTGroupDynamics>();
            baseOutfit.GroupDynamics = baseGroupDyn;
            var outfit1GroupDynObj = CreateGameObject("GroupDyn", outfit1.transform);
            var outfit1GroupDyn = outfit1GroupDynObj.AddComponent<DTGroupDynamics>();
            outfit1.GroupDynamics = outfit1GroupDyn;
            var outfit2GroupDynObj = CreateGameObject("GroupDyn", outfit2.transform);
            var outfit2GroupDyn = outfit2GroupDynObj.AddComponent<DTGroupDynamics>();
            outfit2.GroupDynamics = outfit2GroupDyn;

            // execute
            var pass = new WardrobePass();
            var ctx = new DKNativeContext(avatar);
            Assert.True(pass.Invoke(ctx));

            // expects only four children are there and no another base is created
            // should be BaseOutfit instead, Base is the one default created by pass if not exist
            Assert.AreEqual(4, wardrobe.transform.childCount);
            Assert.AreNotEqual("Base", wardrobe.transform.GetChild(0).name);

            // verify outfit group
            Assert.True(outfitGroupObj.TryGetComponent<DTMenuItem>(out var outfitGroupMenuItem));
            Assert.AreEqual(DTMenuItem.ItemType.SubMenu, outfitGroupMenuItem.Type);
            Assert.AreEqual(DTMenuItem.ItemSubMenuType.Children, outfitGroupMenuItem.SubMenuType);

            // expects outfit enable item directly on outfit1, but none for custom menu group
            Assert.True(baseOutfit.TryGetComponent<DTOutfitEnableMenuItem>(out var baseOutfitEnableItem));
            Assert.Null(baseOutfitEnableItem.TargetOutfit); // null for base
            Assert.True(outfit1.TryGetComponent<DTOutfitEnableMenuItem>(out var outfit1EnableItem));
            Assert.AreEqual(outfit1, outfit1EnableItem.TargetOutfit);
            Assert.False(outfit2.TryGetComponent<DTOutfitEnableMenuItem>(out _));
            Assert.True(outfit3.TryGetComponent<DTOutfitEnableMenuItem>(out var outfit3EnableItem));
            Assert.AreEqual(outfit3, outfit3EnableItem.TargetOutfit);
            Assert.True(outfit4.TryGetComponent<DTOutfitEnableMenuItem>(out var outfit4EnableItem));
            Assert.AreEqual(outfit4, outfit4EnableItem.TargetOutfit);

            // parameter slot controls
            Assert.True(baseOutfit.TryGetComponent<DTSmartControl>(out var baseCtrl));
            Assert.AreEqual(DTSmartControl.SCDriverType.ParameterSlot, baseCtrl.DriverType);
            Assert.AreEqual(0, baseCtrl.ParameterSlotConfig.MappedValue);

            Assert.True(outfit1.TryGetComponent<DTSmartControl>(out var outfit1Ctrl));
            Assert.AreEqual(DTSmartControl.SCDriverType.ParameterSlot, outfit1Ctrl.DriverType);
            Assert.AreEqual(1, outfit1Ctrl.ParameterSlotConfig.MappedValue);

            Assert.True(outfit2.TryGetComponent<DTSmartControl>(out var outfit2Ctrl));
            Assert.AreEqual(DTSmartControl.SCDriverType.ParameterSlot, outfit2Ctrl.DriverType);
            Assert.AreEqual(2, outfit2Ctrl.ParameterSlotConfig.MappedValue);

            Assert.True(outfit3.TryGetComponent<DTSmartControl>(out var outfit3Ctrl));
            Assert.AreEqual(DTSmartControl.SCDriverType.ParameterSlot, outfit3Ctrl.DriverType);
            Assert.AreEqual(3, outfit3Ctrl.ParameterSlotConfig.MappedValue);

            Assert.True(outfit4.TryGetComponent<DTSmartControl>(out var outfit4Ctrl));
            Assert.AreEqual(DTSmartControl.SCDriverType.ParameterSlot, outfit4Ctrl.DriverType);
            Assert.AreEqual(4, outfit4Ctrl.ParameterSlotConfig.MappedValue);

            // base group dynamics off
            Assert.True(outfit1Ctrl.ObjectToggles
                .Where(t =>
                    t.Target == baseOutfit.GroupDynamics &&
                    !t.Enabled)
                .Count() == 1);
            Assert.True(outfit2Ctrl.ObjectToggles
                .Where(t =>
                    t.Target == baseOutfit.GroupDynamics &&
                    !t.Enabled)
                .Count() == 1);
            Assert.True(outfit3Ctrl.ObjectToggles
                .Where(t =>
                    t.Target == baseOutfit.GroupDynamics &&
                    !t.Enabled)
                .Count() == 1);
            Assert.True(outfit4Ctrl.ObjectToggles
                .Where(t =>
                    t.Target == baseOutfit.GroupDynamics &&
                    !t.Enabled)
                .Count() == 1);

            // turn on own dynamics
            Assert.True(outfit1Ctrl.ObjectToggles
                .Where(t =>
                    t.Target == outfit1.GroupDynamics &&
                    t.Enabled)
                .Count() == 1);
            Assert.True(outfit2Ctrl.ObjectToggles
                .Where(t =>
                    t.Target == outfit2.GroupDynamics &&
                    t.Enabled)
                .Count() == 1);
            // none for outfit 3, 4
            Assert.AreEqual(1, outfit3Ctrl.ObjectToggles.Count);
            Assert.AreEqual(1, outfit4Ctrl.ObjectToggles.Count);
        }
    }
}
