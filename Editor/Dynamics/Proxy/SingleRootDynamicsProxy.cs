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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools.Dynamics.Proxy
{
    /// <summary>
    /// Abstract single root dynamics proxy base class
    /// </summary>
    internal abstract class SingleRootDynamicsProxy : DynamicsProxy
    {
        private class SingleTransformCollection : ICollection<Transform>
        {
            public delegate void ValueSetDelegate(Transform value);
            public delegate Transform ValueGetDelegate();

            public Transform Value { get => _valueGetFunc(); set => _valueSetFunc(value); }
            public int Count => Value != null ? 1 : 0;
            public bool IsReadOnly => false;

            private readonly ValueGetDelegate _valueGetFunc;
            private readonly ValueSetDelegate _valueSetFunc;

            public SingleTransformCollection(ValueGetDelegate valueGetFunc, ValueSetDelegate valueSetFunc)
            {
                _valueGetFunc = valueGetFunc;
                _valueSetFunc = valueSetFunc;
            }

            public void Add(Transform item)
            {
                if (Value == null)
                {
                    Value = item;
                }
            }

            public void Clear()
            {
                Value = null;
            }

            public bool Contains(Transform item) => Value == item;
            public void CopyTo(Transform[] array, int arrayIndex) => array[arrayIndex] = Value;
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<Transform> GetEnumerator()
            {
                yield return Value;
            }
            public bool Remove(Transform item)
            {
                if (Value == item)
                {
                    Value = null;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// The single root transform that this dynamics is controlling
        /// </summary>
        public abstract Transform RootTransform { get; set; }
        public sealed override ICollection<Transform> RootTransforms
        {
            get => _transform;
            set => _transform.Value = value.First();
        }

        private readonly SingleTransformCollection _transform;

        public SingleRootDynamicsProxy()
        {
            _transform = new SingleTransformCollection(() => RootTransform, (Transform value) => RootTransform = value);
        }
    }
}
