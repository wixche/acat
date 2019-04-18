﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="VolumeSettingsAgent.cs" company="Intel Corporation">
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
using System.Windows.Forms;

namespace ACAT.Extensions.Default.FunctionalAgents.VolumeSettings
{
    /// <summary>
    /// Functional agent that allows the user to set the volume
    /// of text to speech (TTS).  Volume is normalized from 0 to 9 as
    /// different TTS engines have different ranges.  The value
    /// is then scaled by the TTS engine before setting it.
    /// </summary>
    [DescriptorAttribute("6D6F94CF-154B-4911-84CD-71CBA07424A3",
                        "Volume Settings",
                        "VolumeSettingsAgent",
                        "Set volume of speech engine")]
    internal class VolumeSettingsAgent : FunctionalAgentBase
    {
        /// <summary>
        /// The volume settings scanner object
        /// </summary>
        private VolumeSettingsScanner _volumeSettingsScanner;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public VolumeSettingsAgent()
        {
            Name = DescriptorAttribute.GetDescriptor(GetType()).Name;
        }

        /// <summary>
        /// Invoked when the Functional agent is activated.  This is
        /// the entry point.
        /// </summary>
        /// <returns>true on success</returns>
        public override bool Activate()
        {
            IsClosing = false;
            ExitCode = CompletionCode.None;
            IsActive = true;

            var form = Context.AppPanelManager.CreatePanel("VolumeSettingsScanner");
            if (form != null)
            {
                _volumeSettingsScanner = form as VolumeSettingsScanner;
                _volumeSettingsScanner.FormClosing += _volumeSettingsScanner_FormClosing;
                Context.AppPanelManager.ShowDialog(form as IPanel);
            }

            return true;
        }

        /// <summary>
        /// Invoked to check if a scanner button should be enabled.  Uses context
        /// to determine the 'enabled' state.
        /// </summary>
        /// <param name="arg">info about the scanner button</param>
        public override void CheckCommandEnabled(CommandEnabledArg arg)
        {
            _volumeSettingsScanner.CheckCommandEnabled(arg);
        }

        /// <summary>
        /// Invoked when the focus changes either in the active window or when the
        /// active window itself changes.
        /// </summary>
        /// <param name="monitorInfo">Info about focused element</param>
        /// <param name="handled">was this handled</param>
        public override void OnFocusChanged(WindowActivityMonitorInfo monitorInfo, ref bool handled)
        {
            if (IsClosing)
            {
                Log.Debug("IsClosing is true.  Will not handle the focus change");
                return;
            }

            Log.Debug("OnFocus: " + monitorInfo);

            base.OnFocusChanged(monitorInfo, ref handled);
            handled = true;
        }

        /// <summary>
        /// A request came in to close the agent. We MUST
        /// quit if this call is ever made
        /// </summary>
        /// <returns>true on success</returns>
        public override bool OnRequestClose()
        {
            quit();
            return true;
        }

        /// <summary>
        /// Invoked when there is a request to run a command. This
        /// could as a result of the user activating a button on the
        /// scanner and there is a command associated with the button
        /// </summary>
        /// <param name="command">command to run</param>
        /// <param name="commandArg">any optional arguments</param>
        /// <param name="handled">was this handled?</param>
        public override void OnRunCommand(String command, object commandArg, ref bool handled)
        {
            handled = true;

            switch (command)
            {
                default:
                    base.OnRunCommand(command, commandArg, ref handled);
                    break;
            }
        }

        /// <summary>
        /// Release resources
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void _volumeSettingsScanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsClosing = true;
            IsActive = false;

            Close();
        }

        /// <summary>
        /// Closes all the scanners
        /// </summary>
        private void closeScanner()
        {
            if (_volumeSettingsScanner != null)
            {
                Windows.CloseForm(_volumeSettingsScanner);
                _volumeSettingsScanner = null;
            }
        }

        /// <summary>
        /// Close scanners and exit
        /// </summary>
        private void quit()
        {
            IsClosing = true;
            IsActive = false;

            closeScanner();
            Close();
        }
    }
}