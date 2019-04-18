﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="GenericAppAgent.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.AgentManagement;
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ACAT.Lib.Extension.AppAgents.Generic
{
    /// <summary>
    /// Application agent for processes that are not natively
    /// supported by ACAT in that, they don't have dedicated
    /// Application agents in ACAT.
    /// </summary>
    public class GenericAppAgent : GenericAppAgentBase
    {
        /// <summary>
        /// Has the scanner been shown yet?
        /// </summary>
        protected bool scannerShown;

        /// <summary>
        /// Name of the executing assembly
        /// </summary>
        private String _currentProcessName;

        /// <summary>
        /// Initializes a new instance of the class..
        /// </summary>
        public GenericAppAgent()
        {
            _currentProcessName = Process.GetCurrentProcess().ProcessName.ToLower();
        }

        /// <summary>
        /// Gets which processes this agent supported. Use the
        /// 'magic string' for generic app agents that AgentManager requires.
        /// </summary>
        public override IEnumerable<AgentProcessInfo> ProcessesSupported
        {
            get { return new[] { new AgentProcessInfo("**genericappagent**") }; }
        }

        /// <summary>
        /// Invoked to set the 'enabled' state of a widget.  This
        /// will depend on the current context.
        /// </summary>
        /// <param name="arg">contains info about the widget</param>
        public override void CheckCommandEnabled(CommandEnabledArg arg)
        {
            arg.Handled = true;
            switch (arg.Command)
            {
                case "CmdContextMenu":
                    arg.Enabled = false;
                    return;

                default:
                    arg.Handled = false;
                    break;
            }
        }

        /// <summary>
        /// Displays the contextual menu
        /// </summary>
        /// <param name="monitorInfo">Foreground window info</param>
        public override void OnContextMenuRequest(WindowActivityMonitorInfo monitorInfo)
        {
        }

        /// <summary>
        /// Invoked when the foreground window focus changes.  Display the
        /// alphabet scanner
        /// </summary>
        /// <param name="monitorInfo">Foreground window info</param>
        /// <param name="handled">set to true if handled</param>
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
        /// Focus shifted to a supported app. This agent is
        /// getting deactivated.
        /// </summary>
        public override void OnFocusLost()
        {
            scannerShown = false;
        }

        /// <summary>
        /// Returns true if the specified app is not supported by this agent
        /// </summary>
        /// <param name="appName">app name</param>
        /// <returns>true if not supported</returns>
        private bool ignoreApp(String appName)
        {
            return String.Compare(appName, _currentProcessName, true) == 0;
        }
    }
}