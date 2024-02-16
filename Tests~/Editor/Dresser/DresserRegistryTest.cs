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

using Chocopoi.DressingTools.Dresser;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Dresser
{
    internal class DresserRegistryTest : EditorTestBase
    {
        [Test]
        public void GetDresserByTypeNameTest()
        {
            Assert.NotNull(DresserRegistry.GetDresserByTypeName(typeof(DefaultDresser).FullName));
            Assert.Null(DresserRegistry.GetDresserByTypeName("Some.Random.Name.Does.Not.Exist"));
        }

        [Test]
        public void GetAvailableDresserKeysTest()
        {
            var keys = DresserRegistry.GetAvailableDresserKeys();
            Assert.NotNull(keys);
            Assert.GreaterOrEqual(keys.Length, 1);
        }

        [Test]
        public void GetDresserByIndexTest()
        {
            Assert.NotNull(DresserRegistry.GetDresserByIndex(0));
        }

        [Test]
        public void GetDresserKeyIndexByTypeNameTest()
        {
            Assert.AreEqual(DresserRegistry.GetDresserKeyIndexByTypeName(typeof(DefaultDresser).FullName), 0);
            Assert.AreEqual(DresserRegistry.GetDresserKeyIndexByTypeName("Some.Random.Name.Does.Not.Exist"), -1);
        }

        [Test]
        public void GetDresserByNameTest()
        {
            Assert.NotNull(DresserRegistry.GetDresserByName("Default Dresser"));
        }
    }
}
