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

using System;
using System.Collections.Generic;
using System.Reflection;
using Chocopoi.DressingTools.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Passes
{
    internal class ComponentApplier
    {
        private static Dictionary<Type, Type> s_componentPassTypes = null;

        private readonly ComponentApplyContext _ctx;
        private readonly Dictionary<Type, ComponentPass> _passInstances;

        public ComponentApplier(GameObject avatarGameObject)
        {
            if (s_componentPassTypes == null)
            {
                ScanComponentPassTypes();
            }
            _ctx = new ComponentApplyContext(avatarGameObject);
            _passInstances = new Dictionary<Type, ComponentPass>();
        }

        private static void ScanComponentPassTypes()
        {
            s_componentPassTypes = new Dictionary<Type, Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(ComponentPass)))
                    {
                        var attr = type.GetCustomAttribute<ComponentPassFor>();
                        if (attr == null)
                        {
                            continue;
                        }
                        if (s_componentPassTypes.ContainsKey(attr.Type))
                        {
                            Debug.LogError($"[DressingTools] There are more than one component pass providing service for component {attr.Type.FullName}, ignoring this component");
                            continue;
                        }
                        s_componentPassTypes[attr.Type] = type;
                    }
                }
            }
        }

        public bool Apply(DTBaseComponent component, bool deep = false)
        {
            var compType = component.GetType();
            if (!s_componentPassTypes.ContainsKey(compType))
            {
                Debug.LogWarning($"[DressingTools] Component {compType.FullName} does not support single component apply, ignoring");
                return true;
            }

            var passType = s_componentPassTypes[compType];
            if (!_passInstances.TryGetValue(passType, out var pass))
            {
                _passInstances[passType] = pass = (ComponentPass)Activator.CreateInstance(passType);
            }

            if (!pass.Invoke(_ctx, component, out var generatedComponents))
            {
                return false;
            }

            if (deep)
            {
                // TODO: dependency
                foreach (var generatedComponent in generatedComponents)
                {
                    if (!Apply(generatedComponent, deep))
                    {
                        return false;
                    }
                }
            }

            // destroy the component after success
            Object.DestroyImmediate(component);

            return true;
        }
    }
}
