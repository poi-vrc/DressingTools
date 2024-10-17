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
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.Components.Cabinet;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Configurator.Cabinet;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chocopoi.DressingTools.Configurator.Avatar
{
    internal static class AvatarUtils
    {
        public static List<GameObject> FindSceneAvatars(Scene scene)
        {
            return DKRuntimeUtils.FindSceneAvatars(scene);
        }

        public static GameObject GetAvatarRoot(GameObject gameObject)
        {
            return DKRuntimeUtils.GetAvatarRoot(gameObject);
        }

        public static IAvatarSettings GetAvatarSettings(GameObject avatarGameObject)
        {
            if (avatarGameObject == null)
            {
                return null;
            }

            if (avatarGameObject.TryGetComponent<DTCabinet>(out _))
            {
                return new OneConfAvatarSettings(avatarGameObject);
            }
            // TODO: standalone avatar settings component
            return null;
        }

        public static IWardrobeProvider GetWardrobeProvider(GameObject avatarGameObject)
        {
            if (avatarGameObject == null)
            {
                return null;
            }

            if (avatarGameObject.TryGetComponent<DTCabinet>(out _))
            {
                return new OneConfCabinetProvider(avatarGameObject);
            }
            if (avatarGameObject.TryGetComponent<DTWardrobe>(out _))
            {
                return new DTWardrobeProvider(avatarGameObject);
            }
            return null;
        }
    }
}
