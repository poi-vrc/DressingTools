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

using UnityEngine;

namespace Chocopoi.DressingTools.Components.Animations
{
    /// <summary>
    /// Parameter slot component. It means SmartControls using the same parameter slot are assigned with a specific value.
    /// Only the SmartControl that the parameter matches with will be enabled.
    /// </summary>
    [AddComponentMenu("DressingTools/DT Parameter Slot")]
    internal class DTParameterSlot : DTBaseComponent
    {
        public enum ParameterValueType
        {
            Int = 0,
            Float = 1
        }

        public string ParameterName { get => m_ParameterName; set => m_ParameterName = value; }
        public ParameterValueType ValueType { get => m_ValueType; set => m_ValueType = value; }
        public float ParameterDefaultValue { get => m_ParameterDefaultValue; set => m_ParameterDefaultValue = value; }
        public bool NetworkSynced { get => m_NetworkSynced; set => m_NetworkSynced = value; }
        public bool Saved { get => m_Saved; set => m_Saved = value; }

        [SerializeField] private string m_ParameterName;
        [SerializeField] private ParameterValueType m_ValueType;
        [SerializeField] private float m_ParameterDefaultValue;
        [SerializeField] private bool m_NetworkSynced;
        [SerializeField] private bool m_Saved;

        public DTParameterSlot()
        {
            m_ParameterName = "";
            m_ValueType = ParameterValueType.Int;
            m_ParameterDefaultValue = 0.0f;
            m_NetworkSynced = true;
            m_Saved = true;
        }
    }
}
