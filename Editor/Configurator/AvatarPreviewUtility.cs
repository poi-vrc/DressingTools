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

using System;
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.Configurator.Cabinet;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chocopoi.DressingTools.Configurator
{
    internal static class AvatarPreviewUtility
    {
#if UNITY_2020_1_OR_NEWER
        private class AvatarPreviewStage : PreviewSceneStage
        {
            public GameObject avatarGameObject;
            public IConfigurableOutfit outfit;

            private GameObject _previewAvatarGameObject;
            private GameObject _previewOutfitGameObject;

            protected override GUIContent CreateHeaderContent()
            {
                return new GUIContent("DressingTools Preview");
            }

            private void PrepareAvatarPreview()
            {
                var lighting = new GameObject("Lighting");
                lighting.AddComponent<Light>().type = LightType.Directional;
                SceneManager.MoveGameObjectToScene(lighting, scene);

                _previewAvatarGameObject = Instantiate(avatarGameObject);
                _previewAvatarGameObject.name = avatarGameObject.name;
                if (DKEditorUtils.IsGrandParent(_previewAvatarGameObject.transform, outfit.RootTransform))
                {
                    // if it's inside, reuse it
                    var path = DKEditorUtils.GetRelativePath(outfit.RootTransform, _previewAvatarGameObject.transform);
                    _previewOutfitGameObject = _previewAvatarGameObject.transform.Find(path).gameObject;
                }
                else
                {
                    // if it's outside, create a new one and set parent
                    _previewOutfitGameObject = Instantiate(outfit.RootTransform.gameObject);
                    _previewOutfitGameObject.name = outfit.RootTransform.name;
                    _previewOutfitGameObject.transform.SetParent(_previewAvatarGameObject.transform, false);
                }
                outfit.Preview(_previewAvatarGameObject, _previewOutfitGameObject);
                SceneManager.MoveGameObjectToScene(_previewAvatarGameObject, scene);
            }

            protected override bool OnOpenStage()
            {
                base.OnOpenStage();
                scene = EditorSceneManager.NewPreviewScene();
                PrepareAvatarPreview();
                return true;
            }

            protected override void OnCloseStage()
            {
                foreach (var go in scene.GetRootGameObjects())
                {
                    DestroyImmediate(go);
                }
                EditorSceneManager.ClosePreviewScene(scene);
                base.OnCloseStage();
            }
        }
#endif

        public static void StartAvatarPreview(GameObject avatarGameObject, IConfigurableOutfit outfit, bool legacy = false)
        {
#if UNITY_2020_1_OR_NEWER
            if (!legacy)
            {
                var stage = ScriptableObject.CreateInstance<AvatarPreviewStage>();
                stage.avatarGameObject = avatarGameObject;
                stage.outfit = outfit;
                StageUtility.GoToStage(stage, true);
                return;
            }
#endif
            throw new NotImplementedException("Legacy preview is not yet implemented.");
        }
    }
}
