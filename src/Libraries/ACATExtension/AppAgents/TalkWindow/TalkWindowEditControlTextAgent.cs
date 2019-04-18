﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="TalkWindowEditControlTextAgent.cs" company="Intel Corporation">
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
using ACAT.Lib.Core.PanelManagement;
using System;
using System.Windows.Automation;

namespace ACAT.Lib.Extension.AppAgents.TalkWindow
{
    /// <summary>
    /// The text control agent for the talk window
    /// </summary>
    public class TalkWindowEditControlTextAgent : EditTextControlAgent
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="handle">Handle to the talk window</param>
        /// <param name="editControlElement">Automation element of the text box </param>
        /// <param name="handled">set to true if handled</param>
        public TalkWindowEditControlTextAgent(IntPtr handle, AutomationElement editControlElement, ref bool handled)
            : base(handle, editControlElement, ref handled)
        {
        }

        /// <summary>
        /// Copies text from the talk window to the clipboard
        /// </summary>
        /// <returns>true on success</returns>
        public override bool Copy()
        {
            Context.AppTalkWindowManager.Copy();
            return true;
        }

        /// <summary>
        /// Cuts text in the Talk window
        /// </summary>
        /// <returns>true on success</returns>
        public override bool Cut()
        {
            Context.AppTalkWindowManager.Cut();
            return true;
        }

        /// <summary>
        /// Pastes text from clipboard to the talk window
        /// </summary>
        /// <returns>true on success</returns>
        public override bool Paste()
        {
            Context.AppTalkWindowManager.Paste();
            return true;
        }

        /// <summary>
        /// Selects all text in the Talk window
        /// </summary>
        /// <returns>true on success</returns>
        public override bool SelectAll()
        {
            Context.AppTalkWindowManager.SelectAll();
            return true;
        }
    }
}