/*
 * File: CabinetModuleProviderLocator.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 24th 2023, 08:00:00 pm
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

namespace Chocopoi.DressingTools.Lib.Extensibility.Providers
{
    public class CabinetModuleProviderLocator : ModuleProviderLocatorBase<CabinetModuleProviderBase>
    {
        private static CabinetModuleProviderLocator s_instance = null;

        public static CabinetModuleProviderLocator Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new CabinetModuleProviderLocator();
                }
                return s_instance;
            }
        }
    }
}
