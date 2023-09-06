/*
 * File: IPathRemapper.cs
 * Project: DressingTools
 * Created Date: Tuesday, September 5th 2023, 9:18:30 pm
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

using UnityEngine;

namespace Chocopoi.DressingTools.Lib.Animations
{
    public interface IPathRemapper
    {
        void InvalidateCache();
        void TagContainerBone(GameObject gameObject);
        string Remap(string originalPath, bool avoidContainerBones = false);
    }
}
