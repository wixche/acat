﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="AnimationWidgetAddedEventArgs.cs" company="Intel Corporation">
//
// Copyright (c) 2013-2017 Intel Corporation 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
////////////////////////////////////////////////////////////////////////////

using System;

namespace ACAT.Lib.Core.AnimationManagement
{
    /// <summary>
    /// Event args for the event that is raised when an animation
    /// widget is added to the animation sequence. This gives the
    /// app a chance to do its own initialization.
    /// </summary>
    public class AnimationWidgetAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializaes the object
        /// </summary>
        /// <param name="widget">the animation widget</param>
        public AnimationWidgetAddedEventArgs(AnimationWidget widget)
        {
            AnimWidget = widget;
        }

        /// <summary>
        /// Gets the animation widget object that was added.
        /// </summary>
        public AnimationWidget AnimWidget { get; private set; }
    }
}