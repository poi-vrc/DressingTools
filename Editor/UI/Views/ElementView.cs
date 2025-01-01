/*
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

        public ElementView()
        {
            RegisterCallback<AttachToPanelEvent>(evt => Load?.Invoke());
            RegisterCallback<DetachFromPanelEvent>(evt => Unload?.Invoke());
        }

        public abstract void Repaint();

        public virtual void RaiseForceUpdateViewEvent()
        {
            ForceUpdateView?.Invoke();
        }

        public T Q<T>(string name = null, params string[] classes) where T : VisualElement
        {
            return UQueryExtensions.Query<T>(this, name, classes).Build().First();
        }

        public VisualElement Q(string name = null, params string[] classes)
        {
            return UQueryExtensions.Query<VisualElement>(this, name, classes).Build().First();
        }

        public T Q<T>(string name = null, string className = null) where T : VisualElement
        {
            return UQueryExtensions.Query<T>(this, name, className).Build().First();
        }

        public VisualElement Q(string name = null, string className = null)
        {
            return UQueryExtensions.Query<VisualElement>(this, name, className).Build().First();
        }

        /// <summary>
        /// Binds the foldout with the container together
        /// </summary>
        /// <param name="foldoutName">Foldout name</param>
        /// <param name="containerName">Container name</param>
        public void BindFoldoutHeaderWithContainer(string foldoutName, string containerName)
        {
            var foldout = Q<Foldout>(foldoutName);
            var container = Q<VisualElement>(containerName);
            foldout.RegisterValueChangedCallback((ChangeEvent<bool> evt) => container.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
        }

        public void BindFoldoutHeaderAndContainerWithPrefix(string prefix)
        {
            BindFoldoutHeaderWithContainer($"{prefix}-foldout", $"{prefix}-container");
        }

        /// <summary>
        /// Creates a HelpBox element (Legacy code, please use Unity 2022 HelpBox instead)
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="msgType">Message type</param>
        /// <returns>Element</returns>
        public static VisualElement CreateHelpBox(string msg, MessageType msgType)
        {
            // Unity 2019.4 not yet have helpbox in uxml, but now 2022 has it.
            // return new IMGUIContainer(() => EditorGUILayout.HelpBox(msg, msgType));
            // TODO: remove this legacy code and replace everywhere using this with the below snippet.
            var helpBoxMsgType = msgType switch
            {
                MessageType.Info => HelpBoxMessageType.Info,
                MessageType.Warning => HelpBoxMessageType.Warning,
                MessageType.Error => HelpBoxMessageType.Error,
                _ => HelpBoxMessageType.None,
            };
            var helpBox = new HelpBox
            {
                text = msg,
                messageType = helpBoxMsgType
            };
            return helpBox;
        }
    }
}
