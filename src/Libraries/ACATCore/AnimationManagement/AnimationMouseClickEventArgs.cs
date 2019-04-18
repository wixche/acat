﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="AnimationMouseClickEventArgs.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.Interpreter;
using System;

namespace ACAT.Lib.Core.AnimationManagement
{
    /// <summary>
    /// Event args for the mouse click event
    /// </summary>
    public class AnimationMouseClickEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="animationWidget">animation widget</param>
        /// <param name="onMouseClick">the code for handling the click</param>
        public AnimationMouseClickEventArgs(AnimationWidget animationWidget, PCode onMouseClick)
        {
            AnimationWidget = animationWidget;
            OnMouseClick = onMouseClick;
        }

        /// <summary>
        /// Gets the animation widget which rasied the event
        /// </summary>
        public AnimationWidget AnimationWidget { get; private set; }

        /// <summary>
        /// Gets the PCode associated with the mouse click event
        /// </summary>
        public PCode OnMouseClick { get; private set; }
    }
}