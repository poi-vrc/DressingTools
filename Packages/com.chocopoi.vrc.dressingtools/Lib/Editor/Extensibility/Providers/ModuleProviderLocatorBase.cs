/*
 * File: ModuleProviderLocatorBase.cs
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

namespace Chocopoi.DressingTools.Lib.Extensibility.Providers
{
    public abstract class ModuleProviderLocatorBase<T> where T : ModuleProviderBase
    {
        private Dictionary<string, T> _registry;

        protected ModuleProviderLocatorBase()
        {
            _registry = new Dictionary<string, T>();
        }

        public void Register(T provider)
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

        public T GetProviderByType(Type type)
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

        public U GetProvider<U>(string moduleName) where U : T
        {
            if (!_registry.ContainsKey(moduleName))
            {
                return null;
            }
            return (U)_registry[moduleName];
        }

        public T GetProvider(string moduleName)
        {
            if (!_registry.ContainsKey(moduleName))
            {
                return null;
            }
            return _registry[moduleName];
        }

        public T[] GetAllProviders()
        {
            var array = new T[_registry.Values.Count];
            _registry.Values.CopyTo(array, 0);
            return array;
        }
    }
}
