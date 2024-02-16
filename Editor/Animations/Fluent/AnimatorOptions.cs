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
using Chocopoi.DressingFramework;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations.Fluent
{
    internal class AnimatorOptions
    {
        public enum WriteDefaultsMode
        {
            DoNothing = 0,
            On = 1,
            Off = 2
        }

        public Transform rootTransform;
        public Context context;
        public WriteDefaultsMode writeDefaultsMode;

        public AnimatorOptions WithRootTransform(Transform rootTrans)
        {
            rootTransform = rootTrans;
            return this;
        }

        public AnimatorOptions WithContext(Context ctx)
        {
            context = ctx;
            return this;
        }

        public AnimatorOptions WithWriteDefaultsMode(WriteDefaultsMode newMode)
        {
            writeDefaultsMode = newMode;
            return this;
        }

        public static WriteDefaultsMode DetectWriteDefaultsMode(AnimatorController controller)
        {
            var stack = new Stack<AnimatorStateMachine>();

            foreach (var layer in controller.layers)
            {
                stack.Push(layer.stateMachine);
            }

            var writeDefaultsOn = false;
            var writeDefaultsOff = false;

            while (stack.Count > 0)
            {
                var stateMachine = stack.Pop();
                foreach (var state in stateMachine.states)
                {
                    if (state.state.writeDefaultValues)
                    {
                        writeDefaultsOn = true;
                    }
                    else
                    {
                        writeDefaultsOff = true;
                    }
                }

                foreach (var childAnimatorMachine in stateMachine.stateMachines)
                {
                    stack.Push(childAnimatorMachine.stateMachine);
                }
            }

            if (writeDefaultsOn && writeDefaultsOff)
            {
                // the user's write defaults is messed up, do nothing
                return WriteDefaultsMode.DoNothing;
            }
            else
            {
                return writeDefaultsOn ? WriteDefaultsMode.On : WriteDefaultsMode.Off;
            }
        }
    }
}
