/*
 * File: DresserRegistry.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
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
using System.Linq;

namespace Chocopoi.DressingTools.Dresser
{
    internal class DresserRegistry
    {
        private static readonly List<IDresser> dressers = new List<IDresser>()
        {
            new DefaultDresser()
        };

        public static IDresser GetDresserByTypeName(string name)
        {
            foreach (var dresser in dressers)
            {
                var type = dresser.GetType();
                if (name == type.FullName || name == type.Name)
                {
                    return dresser;
                }
            }
            return null;
        }

        public static string[] GetAvailableDresserKeys()
        {
            string[] dresserKeys = new string[dressers.Count];
            for (var i = 0; i < dressers.Count; i++)
            {
                dresserKeys[i] = dressers[i].FriendlyName;
            }
            return dresserKeys;
        }

        public static IDresser GetDresserByIndex(int index) => dressers[index];

        public static int GetDresserKeyIndexByTypeName(string name)
        {
            for (var i = 0; i < dressers.Count; i++)
            {
                var type = dressers[i].GetType();
                if (name == type.FullName || name == type.Name)
                {
                    return i;
                }
            }
            return -1;
        }

        public static IDresser GetDresserByName(string name)
        {
            return dressers.FirstOrDefault(d => d.FriendlyName == name);
        }
    }
}
