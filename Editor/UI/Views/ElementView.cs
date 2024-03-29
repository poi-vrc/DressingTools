﻿/*
 * Copyright (c) 2023 chocopoi
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
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    /// <summary>
    /// Unity UIElements view base
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ElementView : VisualElement, IEditorView
    {
        public event Action ForceUpdateView;
        public event Action Load;
        public event Action Unload;

        public abstract void Repaint();

        public virtual void RaiseForceUpdateViewEvent()
        {
            ForceUpdateView?.Invoke();
        }

        public virtual void OnEnable()
        {
            RaiseLoadEvent();
        }

        public virtual void OnDisable()
        {
            RaiseUnloadEvent();
        }

        protected void RaiseLoadEvent()
        {
            Load?.Invoke();
        }

        protected void RaiseUnloadEvent()
        {
            Unload?.Invoke();
        }

        public UQueryBuilder<T> Q<T>() where T : VisualElement
        {
            return UQueryExtensions.Query<T>(this);
        }

        public UQueryBuilder<T> Q<T>(string name, string className) where T : VisualElement
        {
            return UQueryExtensions.Query<T>(this, name, className);
        }

        public UQueryBuilder<T> Q<T>(string name, params string[] classes) where T : VisualElement
        {
            return UQueryExtensions.Query<T>(this, name, classes);
        }

        /// <summary>
        /// Binds the foldout with the container together
        /// </summary>
        /// <param name="foldoutName">Foldout name</param>
        /// <param name="containerName">Container name</param>
        public void BindFoldoutHeaderWithContainer(string foldoutName, string containerName)
        {
            var foldout = Q<Foldout>(foldoutName).First();
            var container = Q<VisualElement>(containerName).First();
            foldout.RegisterValueChangedCallback((ChangeEvent<bool> evt) => container.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
        }

        public void BindFoldoutHeaderAndContainerWithPrefix(string prefix)
        {
            BindFoldoutHeaderWithContainer($"{prefix}-foldout", $"{prefix}-container");
        }

        /// <summary>
        /// Creates a HelpBox element (Unity 2019.4 not yet have helpbox in UXML yet)
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="msgType">Message type</param>
        /// <returns>Element</returns>
        public VisualElement CreateHelpBox(string msg, MessageType msgType)
        {
            // unity 2019.4 not yet have helpbox in uxml
            return new IMGUIContainer(() => EditorGUILayout.HelpBox(msg, msgType));
        }
    }
}
