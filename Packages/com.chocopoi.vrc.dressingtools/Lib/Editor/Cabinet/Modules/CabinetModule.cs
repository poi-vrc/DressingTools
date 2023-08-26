﻿/*
 * File: CabinetModule.cs
 * Project: DressingTools
 * Created Date: Thursday, August 24th 2023, 7:53:33 pm
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

using Chocopoi.DressingTools.Lib.Extensibility.Providers;

namespace Chocopoi.DressingTools.Lib.Cabinet.Modules
{
    public class CabinetModule
    {
        public string moduleName;
        public IModuleConfig config;
    }
}