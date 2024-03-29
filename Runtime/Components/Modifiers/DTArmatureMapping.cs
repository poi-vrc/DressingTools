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

using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Modifiers
{
    /// <summary>
    /// Armature mapping component. It is intentionally marked as internal and disabled the add component menu for now.
    /// </summary>
    [AddComponentMenu("")]
    internal class DTArmatureMapping : DTBaseComponent
    {
        public enum DresserTypes
        {
            Default = 0
        }

        public enum MappingMode
        {
            Auto = 0,
            Override = 1,
            Manual = 2,
        }

        public class Tag
        {
            public enum TagType
            {
                DoNothing = 0,
                IgnoreTransform = 1,
                CopyDynamics = 2,
            }

            public TagType Type { get => m_Type; set => m_Type = value; }
            public Transform SourceTransform { get => m_SourceTransform; set => m_SourceTransform = value; }
            public string TargetPath { get => m_TargetPath; set => m_TargetPath = value; }

            [SerializeField] private TagType m_Type;
            [SerializeField] private Transform m_SourceTransform;
            [SerializeField] private string m_TargetPath;

            public Tag()
            {
                m_Type = TagType.IgnoreTransform;
                m_SourceTransform = null;
                m_TargetPath = "";
            }

            public override string ToString()
            {
                return $"{m_Type}: {m_SourceTransform.name} -> {m_TargetPath}";
            }
        }

        public class AMDresserDefaultConfig
        {
            public enum DynamicsOptions
            {
                Auto = 0,
                RemoveDynamicsAndUseParentConstraint = 1,
                KeepDynamicsAndUseParentConstraintIfNecessary = 2,
                IgnoreTransform = 3,
                CopyDynamics = 4,
                IgnoreAll = 5,
            }

            public DynamicsOptions DynamicsOption { get => m_DynamicsOption; set => m_DynamicsOption = value; }

            [SerializeField] private DynamicsOptions m_DynamicsOption;

            public AMDresserDefaultConfig()
            {
                m_DynamicsOption = DynamicsOptions.Auto;
            }
        }

        public DresserTypes DresserType { get => m_DresserType; set => m_DresserType = value; }
        public AMDresserDefaultConfig DresserDefaultConfig { get => m_DresserDefaultConfig; set => m_DresserDefaultConfig = value; }
        public MappingMode Mode { get => m_Mode; set => m_Mode = value; }
        public List<DTObjectMapping.Mapping> Mappings { get => m_Mappings; set => m_Mappings = value; }
        public List<Tag> Tags { get => m_Tags; set => m_Tags = value; }
        public Transform SourceArmature { get => m_SourceArmature; set => m_SourceArmature = value; }
        public string TargetArmaturePath { get => m_TargetArmaturePath; set => m_TargetArmaturePath = value; }
        public bool GroupBones { get => m_GroupBones; set => m_GroupBones = value; }
        public string Prefix { get => m_Prefix; set => m_Prefix = value; }
        public string Suffix { get => m_Suffix; set => m_Suffix = value; }
        public bool PreventDuplicateNames { get => m_PreventDuplicateNames; set => m_PreventDuplicateNames = value; }

        [SerializeField] private DresserTypes m_DresserType;
        [SerializeField] private AMDresserDefaultConfig m_DresserDefaultConfig;
        [SerializeField] private MappingMode m_Mode;
        [SerializeField] private List<DTObjectMapping.Mapping> m_Mappings;
        [SerializeField] private List<Tag> m_Tags;
        [SerializeField] private Transform m_SourceArmature;
        [SerializeField] private string m_TargetArmaturePath;
        [SerializeField] private bool m_GroupBones;
        [SerializeField] private string m_Prefix;
        [SerializeField] private string m_Suffix;
        [SerializeField] private bool m_PreventDuplicateNames;

        public DTArmatureMapping()
        {
            m_DresserType = DresserTypes.Default;
            m_DresserDefaultConfig = new AMDresserDefaultConfig();
            m_Mode = MappingMode.Auto;
            m_Mappings = new List<DTObjectMapping.Mapping>();
            m_Tags = new List<Tag>();
            m_SourceArmature = null;
            m_TargetArmaturePath = "";
            m_GroupBones = true;
        }
    }
}
