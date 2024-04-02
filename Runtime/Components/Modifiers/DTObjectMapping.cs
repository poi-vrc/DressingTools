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
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Modifiers
{
    /// <summary>
    /// Object mapping component. It is intentionally marked as internal and disabled the add component menu for now.
    /// </summary>
    [AddComponentMenu("")]
    internal class DTObjectMapping : DTBaseComponent
    {
        [Serializable]
        public class Mapping
        {
            /// <summary>
            /// Mapping type
            /// </summary>
            public enum MappingType
            {
                /// <summary>
                /// Do nothing
                /// </summary>
                DoNothing = 0,

                /// <summary>
                /// Move source object to target avatar path
                /// </summary>
                MoveToBone = 1,

                /// <summary>
                /// Bind using ParentConstraint
                /// </summary>
                ParentConstraint = 2,

                /// <summary>
                /// Move and ignore transform if dynamics exist
                /// </summary>
                IgnoreTransform = 3,
            }

            /// <summary>
            /// Mapping type
            /// </summary>
            public MappingType Type { get => m_Type; set => m_Type = value; }

            /// <summary>
            /// Source transform
            /// </summary>
            public Transform SourceTransform { get => m_SourceTransform; set => m_SourceTransform = value; }

            /// <summary>
            /// Target path from avatar root
            /// </summary>
            public string TargetPath { get => m_TargetPath; set => m_TargetPath = value; }

            [SerializeField] private MappingType m_Type;
            [SerializeField] private Transform m_SourceTransform;
            [SerializeField] private string m_TargetPath;

            public Mapping()
            {
                m_Type = MappingType.DoNothing;
                m_SourceTransform = null;
                m_TargetPath = "";
            }

            /// <summary>
            /// Check if equals to another mapping
            /// </summary>
            /// <param name="mapping">Another mapping</param>
            /// <returns></returns>
            public bool Equals(Mapping mapping)
            {
                return Type == mapping.Type && SourceTransform == mapping.SourceTransform && TargetPath == mapping.TargetPath;
            }

            /// <summary>
            /// Returns a string representable form
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("{0}: {1} -> {2}", Type, SourceTransform != null ? SourceTransform.name : null, TargetPath);
            }
        }

        /// <summary>
        /// Object mappings
        /// </summary>
        public List<Mapping> Mappings { get => m_Mappings; set => m_Mappings = value; }

        /// <summary>
        /// Group objects
        /// </summary>
        public bool GroupObjects { get => m_GroupObjects; set => m_GroupObjects = value; }
        public string Prefix { get => m_Prefix; set => m_Prefix = value; }
        public string Suffix { get => m_Suffix; set => m_Suffix = value; }
        public bool PreventDuplicateNames { get => m_PreventDuplicateNames; set => m_PreventDuplicateNames = value; }

        [SerializeField] private List<Mapping> m_Mappings;
        [SerializeField] private bool m_GroupObjects;
        [SerializeField] private string m_Prefix;
        [SerializeField] private string m_Suffix;
        [SerializeField] private bool m_PreventDuplicateNames;

        public DTObjectMapping()
        {
            m_Mappings = new List<Mapping>();
            m_GroupObjects = true;
            m_Prefix = "";
            m_Suffix = "";
            m_PreventDuplicateNames = true;
        }
    }
}
