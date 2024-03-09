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

using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Animations.Passes;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Menu;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Animations.Passes
{
    internal class ComposeAnimatorParametersPassTest : EditorTestBase
    {
        private void SetupEnv(out GameObject avatar, out DKNativeContext ctx, out AnimatorParameters animParams)
        {
            avatar = CreateGameObject("Avatar");
            ctx = new DKNativeContext(avatar);
            animParams = ctx.Feature<AnimatorParameters>();
        }

        private static void AddConfig(GameObject go, string param, bool sync, bool save)
        {
            var comp = go.AddComponent<DTAnimatorParameters>();
            comp.Configs.Add(new DTAnimatorParameters.ParameterConfig() { ParameterName = param, NetworkSynced = sync, Saved = save });
        }

        [Test]
        public void TraverseAndReplaceTest()
        {
            SetupEnv(out var avatar, out var ctx, out var animParams);

            var a = CreateGameObject("A", avatar.transform);
            AddConfig(a, "A", true, false);
            var aa = CreateGameObject("AA", a.transform);
            AddConfig(aa, "A", false, true);
            // aa's config is replaced

            var b = CreateGameObject("B", avatar.transform);
            var ba = CreateGameObject("BA", b.transform);
            AddConfig(ba, "B", false, true);
            var baa = CreateGameObject("BAA", ba.transform);
            AddConfig(baa, "B", true, false);
            // baa's config is replaced

            var c = CreateGameObject("C", avatar.transform);
            AddConfig(c, "C", false, true);

            new ComposeAnimatorParametersPass().Invoke(ctx);

            var confA = animParams.FindConfig("A");
            Assert.True(confA.networkSynced);
            Assert.False(confA.saved);

            var confB = animParams.FindConfig("B");
            Assert.False(confB.networkSynced);
            Assert.True(confB.saved);

            var confC = animParams.FindConfig("C");
            Assert.False(confC.networkSynced);
            Assert.True(confC.saved);
        }

        private static void VerifyMenuItemParamConfig(AnimatorParameters animParams, string param)
        {
            var conf = animParams.FindConfig("Btn");
            Assert.True(conf.networkSynced);
            Assert.True(conf.saved);
        }

        [Test]
        public void MenuItemParamsTest()
        {
            SetupEnv(out var avatar, out var ctx, out var animParams);

            var mg = CreateGameObject("Menu", avatar.transform);
            mg.AddComponent<DTMenuGroup>();

            {
                var go = CreateGameObject("Btn", mg.transform);
                var item = go.AddComponent<DTMenuItem>();
                item.Type = DTMenuItem.ItemType.Button;
                item.Controller.Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter;
                item.Controller.AnimatorParameterName = "Btn";
            }

            {
                var go = CreateGameObject("TwoAxis", mg.transform);
                var item = go.AddComponent<DTMenuItem>();
                item.Type = DTMenuItem.ItemType.TwoAxis;
                item.SubControllers = new DTMenuItem.ItemController[] {
                    new DTMenuItem.ItemController() {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = "TwoAxis1"
                    },
                    new DTMenuItem.ItemController() {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = "TwoAxis2"
                    }
                };
            }

            {
                var go = CreateGameObject("FourAxis", mg.transform);
                var item = go.AddComponent<DTMenuItem>();
                item.Type = DTMenuItem.ItemType.FourAxis;
                item.SubControllers = new DTMenuItem.ItemController[] {
                    new DTMenuItem.ItemController() {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = "FourAxis1"
                    },
                    new DTMenuItem.ItemController() {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = "FourAxis2"
                    },
                    new DTMenuItem.ItemController() {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = "FourAxis3"
                    },
                    new DTMenuItem.ItemController() {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = "FourAxis4"
                    }
                };
            }

            {
                var go = CreateGameObject("Radial", mg.transform);
                var item = go.AddComponent<DTMenuItem>();
                item.Type = DTMenuItem.ItemType.Radial;
                item.SubControllers = new DTMenuItem.ItemController[] {
                    new DTMenuItem.ItemController() {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = "Radial"
                    }
                };
            }

            new ComposeAnimatorParametersPass().Invoke(ctx);

            VerifyMenuItemParamConfig(animParams, "Btn");
            VerifyMenuItemParamConfig(animParams, "TwoAxis1");
            VerifyMenuItemParamConfig(animParams, "TwoAxis2");
            VerifyMenuItemParamConfig(animParams, "FourAxis1");
            VerifyMenuItemParamConfig(animParams, "FourAxis2");
            VerifyMenuItemParamConfig(animParams, "FourAxis3");
            VerifyMenuItemParamConfig(animParams, "FourAxis4");
            VerifyMenuItemParamConfig(animParams, "Radial");
        }
    }
}
