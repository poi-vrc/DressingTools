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

using System.Collections.Generic;
using Chocopoi.DressingTools.Components.Cabinet;
using Chocopoi.DressingTools.Configurator.Modules;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Configurator.Cabinet
{
    internal class DTConfigurableOutfit : IConfigurableOutfit
    {
        public Transform RootTransform { get => _outfitComp.RootTransform; }
        public string Name => _outfitComp.Name;
        public Texture2D Icon => _outfitComp.Icon;

        private readonly DTAlternateOutfit _outfitComp;

        public DTConfigurableOutfit(DTAlternateOutfit outfitComp)
        {
            _outfitComp = outfitComp;
        }

        public VisualElement CreateView()
        {
            throw new System.NotImplementedException();
        }

        public List<IModule> GetModules()
        {
            throw new System.NotImplementedException();
        }

        public void Preview(GameObject previewAvatarGameObject, GameObject previewOutfitGameObject)
        {
            throw new System.NotImplementedException();
        }
    }
}
