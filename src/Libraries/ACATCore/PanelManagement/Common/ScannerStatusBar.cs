﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ScannerStatusBar.cs" company="Intel Corporation">
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

namespace ACAT.Lib.Core.PanelManagement
{
    /// <summary>
    /// Represents the Status Bar in the scanner form that
    /// will display the states of keys such as Ctrl, Alt, Shift.
    /// For instance, these could Label objects that displays whether
    /// Ctrl is currently pressed or not. Or they could be a StatusStrip
    /// labels.
    /// </summary>
    public class ScannerStatusBar
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ScannerStatusBar()
        {
            AltStatus = null;
            CtrlStatus = null;
            FuncStatus = null;
            ShiftStatus = null;
        }

        /// <summary>
        /// Gets or sets control that will display the state of
        /// the Alt key
        /// </summary>
        public object AltStatus { get; set; }

        /// <summary>
        /// Gets or sets control that will display the state of
        /// the Ctrl key
        /// </summary>
        public object CtrlStatus { get; set; }

        /// <summary>
        /// Gets or sets control that will display the state of
        /// the the Function keys
        /// </summary>
        public object FuncStatus { get; set; }

        /// <summary>
        /// Gets or sets control that will display the state of
        /// the Shift key
        /// </summary>
        public object ShiftStatus { get; set; }
    }
}