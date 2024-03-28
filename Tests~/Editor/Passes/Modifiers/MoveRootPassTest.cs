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

using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Passes.Modifiers;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Passes.Modifiers
{
    internal class MoveRootPassTest : EditorTestBase
    {
        [Test]
        public void InvokeTest()
        {
            var pass = new MoveRootPass();
            var avatar = CreateGameObject("Avatar");
            var ctx = new DKNativeContext(avatar);

            var a = CreateGameObject("A", avatar.transform);
            var b = CreateGameObject("B", avatar.transform);

            var comp = a.AddComponent<DTMoveRoot>();
            comp.DestinationPath = "B";

            Assert.True(pass.Invoke(ctx));
            Assert.AreEqual(b.transform, a.transform.parent);
        }

        [Test]
        public void InvokeComponentPassTest()
        {
            var pass = new MoveRootPass();
            var avatar = CreateGameObject("Avatar");
            var ctx = new DKNativeContext(avatar);

            var a = CreateGameObject("A", avatar.transform);
            var b = CreateGameObject("B", avatar.transform);

            var comp = a.AddComponent<DTMoveRoot>();
            comp.DestinationPath = "B";

            Assert.True(pass.Invoke(ctx, comp, out var output));
            Assert.AreEqual(b.transform, a.transform.parent);
            Assert.AreEqual(0, output.Count);
        }
    }
}
