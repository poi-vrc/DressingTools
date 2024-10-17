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

using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Components.Modifiers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Configurator.Modules
{
    internal class DTArmatureMappingModule : IArmatureMergingModule
    {
        public Transform TargetArmature
        {
            get => string.IsNullOrEmpty(_comp.TargetArmaturePath) ?
                null :
                _avatarGameObject.transform.Find(_comp.TargetArmaturePath);
            set =>
                _comp.TargetArmaturePath =
                    (value == null || value == _avatarGameObject.transform) ?
                    "" :
                    AnimationUtils.GetRelativePath(value.transform, _avatarGameObject.transform);
        }
        public Transform SourceArmature { get => _comp.SourceArmature; set => _comp.SourceArmature = value; }

        private readonly GameObject _avatarGameObject;
        private readonly DTArmatureMapping _comp;

        public DTArmatureMappingModule(GameObject avatarGameObject, DTArmatureMapping comp)
        {
            _avatarGameObject = avatarGameObject;
            _comp = comp;
        }

        public VisualElement CreateView()
        {
            throw new System.NotImplementedException();
        }
    }
}
