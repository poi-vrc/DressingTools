/*
 * File: DTLibRuntimeUtils.cs
 * Project: DressingTools
 * Created Date: Saturday, August 20th 2023, 01:22:50 pm
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
using Chocopoi.DressingTools.Lib.Cabinet;

namespace Chocopoi.DressingTools.Lib
{
    internal static class DTLibRuntimeUtils
    {
        private static readonly System.Random Random = new System.Random();

        public enum LifecycleStage
        {
            Awake,
            Start
        }

        public delegate void OnCabinetLifecycleDelegate(LifecycleStage stage, DTCabinet cabinet);
        public static OnCabinetLifecycleDelegate OnCabinetLifecycle = (stage, cabinet) => { };

        public static string RandomString(int length)
        {
            // i just copied from stackoverflow :D
            // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings?page=1&tab=scoredesc#tab-top
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}
