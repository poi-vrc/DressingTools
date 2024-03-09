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
    internal partial class DTSmartControl
    {
        public class PropertyGroupBuilder
        {
            private readonly PropertyGroup _propGp;

            internal PropertyGroupBuilder(PropertyGroup propGp)
            {
                _propGp = propGp;
            }

            public PropertyGroupBuilder SearchIn(Transform transform)
            {
                _propGp.SearchTransform = transform;
                return this;
            }

            public PropertyGroupBuilder WithSelectedObjects(params GameObject[] gameObjects)
            {
                _propGp.SelectionType = PropertyGroup.PropertySelectionType.Normal;
                _propGp.GameObjects.Clear();
                _propGp.GameObjects.AddRange(gameObjects);
                return this;
            }

            public PropertyGroupBuilder WithIgnoredObjects(params GameObject[] gameObjects)
            {
                _propGp.SelectionType = PropertyGroup.PropertySelectionType.Inverted;
                _propGp.GameObjects.Clear();
                _propGp.GameObjects.AddRange(gameObjects);
                return this;
            }

            public PropertyGroupBuilder WithAvatarWide()
            {
                return WithAvatarWideAndIgnore();
            }

            public PropertyGroupBuilder WithAvatarWideAndIgnore(params GameObject[] gameObjects)
            {
                _propGp.SelectionType = PropertyGroup.PropertySelectionType.AvatarWide;
                _propGp.GameObjects.Clear();
                _propGp.GameObjects.AddRange(gameObjects);
                return this;
            }

            public PropertyGroupBuilder ChangeProperty(string name, float value)
            {
                _propGp.PropertyValues.Add(new PropertyGroup.PropertyValue()
                {
                    Name = name,
                    Value = value,
                    FromValue = value,
                    ToValue = value,
                    ValueObjectReference = null
                });
                return this;
            }

            public PropertyGroupBuilder ChangeProperty(string name, float fromValue, float toValue)
            {
                _propGp.PropertyValues.Add(new PropertyGroup.PropertyValue()
                {
                    Name = name,
                    Value = toValue,
                    FromValue = fromValue,
                    ToValue = toValue,
                    ValueObjectReference = null
                });
                return this;
            }

            public PropertyGroupBuilder ChangeProperty(string name, Object value)
            {
                _propGp.PropertyValues.Add(new PropertyGroup.PropertyValue()
                {
                    Name = name,
                    Value = 0.0f,
                    FromValue = 0.0f,
                    ToValue = 0.0f,
                    ValueObjectReference = value
                });
                return this;
            }

            public PropertyGroup Build()
            {
                return _propGp;
            }
        }

        public class BinarySmartControlBuilder
        {
            private readonly DTSmartControl _control;

            internal BinarySmartControlBuilder(DTSmartControl control)
            {
                _control = control;
                _control.ControlType = SmartControlControlType.Binary;
            }

            public BinarySmartControlBuilder Toggle(GameObject gameObject, bool enabled)
            {
                return Toggle(gameObject.transform, enabled);
            }

            public BinarySmartControlBuilder Toggle(Component component, bool enabled)
            {
                _control.ObjectToggles.Add(new ObjectToggle()
                {
                    Target = component,
                    Enabled = enabled
                });
                return this;
            }

            // TODO
            internal BinarySmartControlBuilder CrossControlValueOnEnable(DTSmartControl control, float value)
            {
                _control.CrossControlActions.ValueActions.ValuesOnEnable.Add(new SmartControlCrossControlActions.ControlValueActions.ControlValue()
                {
                    Control = control,
                    Value = value
                });
                return this;
            }

            // TODO
            internal BinarySmartControlBuilder CrossControlValueOnDisable(DTSmartControl control, float value)
            {
                _control.CrossControlActions.ValueActions.ValuesOnDisable.Add(new SmartControlCrossControlActions.ControlValueActions.ControlValue()
                {
                    Control = control,
                    Value = value
                });
                return this;
            }

            public BinarySmartControlBuilder AddPropertyGroup(PropertyGroupBuilder propGpBuilder)
            {
                _control.PropertyGroups.Add(propGpBuilder.Build());
                return this;
            }

            public DTSmartControl Build()
            {
                return _control;
            }
        }

        public class MotionTimeSmartControlBuilder
        {
            private readonly DTSmartControl _control;

            internal MotionTimeSmartControlBuilder(DTSmartControl control)
            {
                _control = control;
                _control.ControlType = SmartControlControlType.MotionTime;
            }

            public MotionTimeSmartControlBuilder AddPropertyGroup(PropertyGroupBuilder propGpBuilder)
            {
                _control.PropertyGroups.Add(propGpBuilder.Build());
                return this;
            }

            public DTSmartControl Build()
            {
                return _control;
            }
        }

        public DTSmartControl WithDriverType(SmartControlDriverType driverType)
        {
            DriverType = driverType;
            return this;
        }

        public DTSmartControl WithAnimationConfig(SmartControlAnimatorConfig animatorConfig)
        {
            AnimatorConfig = animatorConfig;
            return this;
        }

        public BinarySmartControlBuilder AsBinary()
        {
            return new BinarySmartControlBuilder(this);
        }

        public MotionTimeSmartControlBuilder AsMotionTime()
        {
            return new MotionTimeSmartControlBuilder(this);
        }

        public PropertyGroupBuilder NewPropertyGroup()
        {
            return new PropertyGroupBuilder(new PropertyGroup());
        }
    }
}
