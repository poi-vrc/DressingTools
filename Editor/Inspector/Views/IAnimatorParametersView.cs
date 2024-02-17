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
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.Inspector.Views
{
    internal class AnimatorParametersConfig
    {
        public string parameterName;
        public Type type;
        public float defaultValue;
        public bool networkSynced;
        public bool saved;
    }

    internal interface IAnimatorParametersView : IEditorView
    {
        event Action MouseEnter;
        event Action MouseLeave;
        event Action AddConfig;
        event Action<int> RemoveConfig;
        event Action<int> ChangeConfig;

        DTAnimatorParameters Target { get; set; }
        List<AnimatorParametersConfig> Configs { get; set; }
        GameObject AnimatorParameterTextFieldAvatarGameObject { get; set; }
    }
}
