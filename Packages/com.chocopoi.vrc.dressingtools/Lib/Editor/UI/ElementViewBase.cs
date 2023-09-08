/*
 * File: IMGUIViewBase.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 12:21:25 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Lib.UI
{
    [ExcludeFromCodeCoverage]
    public abstract class ElementViewBase : VisualElement, IEditorView
    {
        public event Action ForceUpdateView;
        public event Action Load;
        public event Action Unload;

        public virtual void RaiseForceUpdateViewEvent()
        {
            ForceUpdateView?.Invoke();
        }

        public virtual void OnEnable()
        {
            Load?.Invoke();
        }

        public virtual void OnDisable()
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
    }
}
