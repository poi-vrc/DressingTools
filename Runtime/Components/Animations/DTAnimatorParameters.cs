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

namespace Chocopoi.DressingTools.Components.Animations
{
    /// <summary>
    /// DT Animator Parameters
    /// 
    /// A component to set network synced, saved configuration of animator parameters.
    /// </summary>
    [AddComponentMenu("DressingTools/DT Animator Parameters")]
    internal class DTAnimatorParameters : DTBaseComponent
    {
        [Serializable]
        public class ParameterConfig
        {
            public string ParameterName { get => m_ParameterName; set => m_ParameterName = value; }
            public float ParameterDefaultValue { get => m_ParameterDefaultValue; set => m_ParameterDefaultValue = value; }
            public bool NetworkSynced { get => m_NetworkSynced; set => m_NetworkSynced = value; }
            public bool Saved { get => m_Saved; set => m_Saved = value; }

            [SerializeField] private string m_ParameterName;
            [SerializeField] private float m_ParameterDefaultValue;
            [SerializeField] private bool m_NetworkSynced;
            [SerializeField] private bool m_Saved;

            public ParameterConfig()
            {
                m_ParameterName = "";
                m_ParameterDefaultValue = 0.0f;
                m_NetworkSynced = true;
                m_Saved = true;
            }
        }

        public List<ParameterConfig> Configs { get => m_Configs; set => m_Configs = value; }

        [SerializeField] private List<ParameterConfig> m_Configs;

        public DTAnimatorParameters()
        {
            m_Configs = new List<ParameterConfig>();
        }
    }
}
