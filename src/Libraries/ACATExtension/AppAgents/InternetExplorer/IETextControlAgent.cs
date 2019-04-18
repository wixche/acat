﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="IETextControlAgent.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.AgentManagement.TextInterface;
using ACAT.Lib.Core.Utility;
using System;
using System.Windows.Automation;

namespace ACAT.Lib.Extension.AppAgents.InternetExplorer
{
    /// <summary>
    /// Text control agent for the IE browser.  Disable
    /// spellcheck and abbreviation expansion.
    /// </summary>
    public class IETextControlAgent : EditTextControlAgent
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="handle">Handle to the IE window</param>
        /// <param name="editControlElement">the currently focused element</param>
        /// <param name="handled">set to true if handle</param>
        public IETextControlAgent(IntPtr handle, AutomationElement editControlElement, ref bool handled)
            : base(handle, editControlElement, ref handled)
        {
            Log.Debug();
        }

        /// <summary>
        /// Don't expand abbreviations
        /// </summary>
        /// <returns>false</returns>
        public override bool ExpandAbbreviations()
        {
            return false;
        }

        /// <summary>
        /// Disable spellchecking
        /// </summary>
        /// <returns>true</returns>
        public override bool SupportsSpellCheck()
        {
            return true;
        }
    }
}