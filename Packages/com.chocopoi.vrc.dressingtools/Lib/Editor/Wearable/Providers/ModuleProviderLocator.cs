/*
 * File: ModuleServiceLocator.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 22nd 2023, 10:41:21 am
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

using System;
using System.Collections.Generic;

namespace Chocopoi.DressingTools.Lib.Wearable.Modules.Providers
{
    public class ModuleProviderLocator
    {
        private static ModuleProviderLocator s_instance = null;

        public static ModuleProviderLocator Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new ModuleProviderLocator();
                }
                return s_instance;
            }
        }

        private Dictionary<string, ModuleProviderBase> _registry;

        private ModuleProviderLocator()
        {
            _registry = new Dictionary<string, ModuleProviderBase>();
        }

        public void Register(ModuleProviderBase provider)
        {
            _registry[provider.ModuleIdentifier] = provider;
        }

        public void Unregister(string moduleName)
        {
            if (_registry.ContainsKey(moduleName))
            {
                _registry.Remove(moduleName);
            }
        }

        public ModuleProviderBase GetProviderByType(Type type)
        {
            foreach (var service in _registry.Values)
            {
                if (service.GetType() == type)
                {
                    return service;
                }
            }
            return null;
        }

        public T GetProvider<T>(string moduleName) where T : ModuleProviderBase
        {
            if (!_registry.ContainsKey(moduleName))
            {
                return null;
            }
            return (T)_registry[moduleName];
        }

        public ModuleProviderBase GetProvider(string moduleName)
        {
            if (!_registry.ContainsKey(moduleName))
            {
                return null;
            }
            return _registry[moduleName];
        }

        public ModuleProviderBase[] GetAllProviders()
        {
            var array = new ModuleProviderBase[_registry.Values.Count];
            _registry.Values.CopyTo(array, 0);
            return array;
        }
    }
}
