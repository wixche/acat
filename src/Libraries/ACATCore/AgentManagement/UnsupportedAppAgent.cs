﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="UnsupportedAppAgent.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;
using System;
using System.Collections.Generic;

namespace ACAT.Lib.Core.AgentManagement
{
    /// <summary>
    /// Application agent for all unsupported applications. This is the
    /// default agent used by the Agent Manager if it can't find an agent
    /// that supports the current foreground process.
    /// </summary>
    [DescriptorAttribute("B23F799A-2A08-4387-BF5D-D4F80F79951A",
                        "Unsupported App Agent",
                        "Application Agent for unsupported applications")]
    public class UnsupportedAppAgent : GenericAppAgentBase
    {
        /// <summary>
        /// Has the scanner been shown yet?
        /// </summary>
        protected bool scannerShown;

        /// <summary>
        /// Ignore ACAT. There is a dedicated agent for it
        /// </summary>
        private static String[] _ignoreAppList = { "acatapp" };

        /// <summary>
        /// Returns the processes supported by this agent.
        /// </summary>
        public override IEnumerable<AgentProcessInfo> ProcessesSupported
        {
            get { return new[] { new AgentProcessInfo("**genericappagent**") }; }
        }

        /// <summary>
        /// Checks if the command should be enabled depending on the
        /// current context
        /// </summary>
        /// <param name="arg">contains info about the widget</param>
        public override void CheckCommandEnabled(CommandEnabledArg arg)
        {
            switch (arg.Command)
            {
                case "CmdContextMenu":
                    arg.Enabled = false;
                    arg.Handled = true;
                    break;

                default:
                    arg.Handled = false;
                    break;
            }
        }

        /// <summary>
        /// No contextual menu supported
        /// </summary>
        /// <param name="monitorInfo"></param>
        public override void OnContextMenuRequest(WindowActivityMonitorInfo monitorInfo)
        {
        }

        /// <summary>
        /// Invoked when the focus in the fg window or if the active window
        /// itself chnages.  Displays the alphabet scanner as the default
        /// scanner
        /// </summary>
        /// <param name="monitorInfo">Info about the active window</param>
        /// <param name="handled">true if handled</param>
        public override void OnFocusChanged(WindowActivityMonitorInfo monitorInfo, ref bool handled)
        {
            Log.Debug();

            if (ignoreApp(monitorInfo.FgProcess.ProcessName))
            {
                handled = false;
                return;
            }

            base.OnFocusChanged(monitorInfo, ref handled);
            Log.Debug("IsNew: " + monitorInfo.IsNewWindow + ", scannerShown: " + scannerShown);
            if (monitorInfo.IsNewWindow || !scannerShown)
            {
                showPanel(this, new PanelRequestEventArgs(PanelClasses.Alphabet, monitorInfo));
                scannerShown = true;
            }
            handled = true;
        }

        /// <summary>
        /// Fg application changed
        /// </summary>
        public override void OnFocusLost()
        {
            scannerShown = true;
        }

        /// <summary>
        /// Returns true if the specified app should be ignored.
        /// </summary>
        /// <param name="appName">Name of the app</param>
        /// <returns>true if it should ignored</returns>
        private bool ignoreApp(String appName)
        {
            foreach (String app in _ignoreAppList)
            {
                if (String.Compare(appName, app, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}