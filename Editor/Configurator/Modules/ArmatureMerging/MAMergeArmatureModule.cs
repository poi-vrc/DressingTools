/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */
#if DT_MA
using Chocopoi.DressingTools.UI.Views;
using nadena.dev.modular_avatar.core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Configurator.Modules
{
    internal class MAMergeArmatureModule : IArmatureMergingModule
    {
        public Transform TargetArmature { get => _comp.mergeTargetObject != null ? _comp.mergeTargetObject.transform : null; set => _comp.mergeTarget.Set(value != null ? value.gameObject : null); }
        public Transform SourceArmature { get => _comp.transform; }

        private readonly ModularAvatarMergeArmature _comp;

        public MAMergeArmatureModule(ModularAvatarMergeArmature comp)
        {
            _comp = comp;
        }

        public VisualElement CreateView()
        {
            throw new System.NotImplementedException();
        }
    }
}
#endif
