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
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Configurator.Cabinet
{
    internal class DTWardrobeProvider : IWardrobeProvider
    {
        private readonly GameObject _avatarGameObject;

        public DTWardrobeProvider(GameObject avatarGameObject)
        {
            _avatarGameObject = avatarGameObject;
        }

        public VisualElement CreateView()
        {
            throw new System.NotImplementedException();
        }

        public List<IConfigurableOutfit> GetOutfits()
        {
            var comps = _avatarGameObject.GetComponentsInChildren<DTAlternateOutfit>(true);
            var outfits = new List<IConfigurableOutfit>();
            foreach (var comp in comps)
            {
                outfits.Add(new DTConfigurableOutfit(comp));
            }
            return outfits;
        }

        public void RemoveOutfit(IConfigurableOutfit outfit)
        {
            throw new System.NotImplementedException();
        }
    }
}
