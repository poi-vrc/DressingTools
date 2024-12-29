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
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Configurator.Views;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Configurator.Cabinet
{
    internal class OneConfCabinetProvider : IWardrobeProvider
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private readonly GameObject _avatarGameObject;

        public OneConfCabinetProvider(GameObject avatarGameObject)
        {
            _avatarGameObject = avatarGameObject;
        }

        public VisualElement CreateView()
        {
            var view = new OneConfCabinetView(_avatarGameObject);
            view.OnEnable();
            return view;
        }

        public List<IConfigurableOutfit> GetOutfits()
        {
            var wearables = OneConfUtils.GetCabinetWearables(_avatarGameObject);
            var outfits = new List<IConfigurableOutfit>();
            foreach (var wearable in wearables)
            {
                outfits.Add(new OneConfConfigurableOutfit(_avatarGameObject, wearable));
            }
            return outfits;
        }

        public void RemoveOutfit(IConfigurableOutfit outfit)
        {
            // if outfit is an object inside of a prefab, do not remove it
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(outfit.RootTransform.gameObject) && PrefabUtility.IsPartOfAnyPrefab(outfit.RootTransform.gameObject))
            {
                EditorUtility.DisplayDialog(t._("tool.name"), t._("configurator.cabinet.oneConf.dialog.outfitPartOfPrefabObjectNotRemoved"), t._("common.dialog.btn.ok"));
                if (outfit.RootTransform.TryGetComponent<DTWearable>(out var comp))
                {
                    Undo.DestroyObjectImmediate(comp);
                }
                return;
            }
            Undo.DestroyObjectImmediate(outfit.RootTransform.gameObject);
        }
    }
}
