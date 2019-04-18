﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="WordPadAgentBase.cs" company="Intel Corporation">
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

using ACAT.ACATResources;
using ACAT.Lib.Core.AgentManagement;
using ACAT.Lib.Core.Extensions;
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACAT.Lib.Extension.AppAgents.Wordpad
{
    /// <summary>
    /// Base class for the Application agent for WordPad.
    /// </summary>
    public class WordpadAgentBase : GenericAppAgentBase
    {
        /// <summary>
        /// If set to true, the agent will autoswitch the
        /// scanners depending on which element has focus.
        /// Eg: Alphabet scanner if an edit text window has focus,
        /// the contextual menu if the main document has focus
        /// </summary>
        protected bool autoSwitchScanners = true;

        /// <summary>
        /// Snap window to alphabet scanner
        /// </summary>
        protected bool snapWindowDockAlphabetScanner;

        /// <summary>
        /// Name of the WordPad process
        /// </summary>
        private const String WordPadProcessName = "wordpad";

        private readonly String[] _supportedCommands =
        {
            "NewFile",
            "OpenFile",
            "SaveFile",
            "SaveFileAs",
            "CmdFind",
            "CmdContextMenu",
            "CmdSwitchApps"
        };

        /// <summary>
        /// Title of the contextual menu
        /// </summary>
        private readonly String ContextMenuTitle = R.GetString("WordPad");

        /// <summary>
        /// Word prediction context handle used to add contents
        /// of the wordpad window for temporary learning.
        /// </summary>
        private int _wordPredictionContext;

        /// <summary>
        /// Returns list of processes supported by this agent
        /// </summary>
        public override IEnumerable<AgentProcessInfo> ProcessesSupported
        {
            get { return new[] { new AgentProcessInfo(WordPadProcessName) }; }
        }

        /// <summary>
        /// Invoked to set the 'enabled' state of a widget.  This
        /// will depend on the current context.
        /// </summary>
        /// <param name="arg">contains info about the widget</param>
        public override void CheckCommandEnabled(CommandEnabledArg arg)
        {
            checkCommandEnabled(_supportedCommands, arg);
        }

        /// <summary>
        /// Displays the contextual menu
        /// </summary>
        /// <param name="monitorInfo">Foreground window info</param>
        public override void OnContextMenuRequest(WindowActivityMonitorInfo monitorInfo)
        {
            showPanel(this, new PanelRequestEventArgs("WordPadContextMenu", ContextMenuTitle, monitorInfo));
        }

        /// <summary>
        /// Invoked when the foreground window focus changes. Display the
        /// scanner depending on the context. Also, if this is a new window that has
        /// come into focus, add its contents to the word prediction temporary batch model for more
        /// contextual prediction of words
        /// </summary>
        /// <param name="monitorInfo">Foreground window info</param>
        /// <param name="handled">set to true if handled</param>
        public override void OnFocusChanged(WindowActivityMonitorInfo monitorInfo, ref bool handled)
        {
            Log.Debug();

            base.OnFocusChanged(monitorInfo, ref handled);

            Log.Debug(monitorInfo.FocusedElement.Current.ClassName + ", " +
                        monitorInfo.FocusedElement.Current.ControlType.ProgrammaticName);

            if (autoSwitchScanners)
            {
                displayScanner(monitorInfo, ref handled);
            }
            else
            {
                if (monitorInfo.IsNewWindow)
                {
                    addWordPredictionContext();

                    showPanel(this, new PanelRequestEventArgs(PanelClasses.Alphabet, monitorInfo));

                    handled = true;
                }
            }
        }

        /// <summary>
        /// Focus shifted to another app.  This agent is
        /// getting deactivated.
        /// </summary>
        public override void OnFocusLost()
        {
            Context.AppWordPredictionManager.ActiveWordPredictor.UnloadContext(_wordPredictionContext);
        }

        /// <summary>
        /// Invoked to run a command
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="commandArg">Optional arguments for the command</param>
        /// <param name="handled">set this to true if handled</param>
        public override void OnRunCommand(String command, object commandArg, ref bool handled)
        {
            handled = true;
            switch (command)
            {
                case "SwitchAppWindow":
                    DialogUtils.ShowTaskSwitcher(WordPadProcessName);
                    break;

                case "WordPadLectureManager":
                    if (TextControlAgent != null)
                    {
                        String text = TextControlAgent.GetText();
                        if (String.IsNullOrEmpty(text.Trim()))
                        {
                            DialogUtils.ShowTimedDialog(PanelManager.Instance.GetCurrentPanel() as Form,
                                                        R.GetString("LectureManager"), R.GetString("DocumentIsEmpty"));
                            break;
                        }

                        if (DialogUtils.ConfirmScanner(R.GetString("LoadThisDocIntoLM")))
                        {
#pragma warning disable 4014
                            launchLectureManager();
#pragma warning restore 4014
                        }
                    }

                    break;

                case "CmdSnapMaxDockWindowToggle":
                    if (snapWindowDockAlphabetScanner)
                    {
                        Windows.ToggleForegroundWindowMaximizeDock(Context.AppPanelManager.GetCurrentForm() as Form,
                            Context.AppWindowPosition, true);
                    }
                    else
                    {
                        Windows.ToggleSnapForegroundWindow(Context.AppWindowPosition, Common.AppPreferences.WindowSnapSizePercent);
                    }
                    break;

                default:
                    base.OnRunCommand(command, commandArg, ref handled);
                    break;
            }
        }

        /// <summary>
        /// Addes text from the WordPad window to the word prediction context
        /// </summary>
        private void addWordPredictionContext()
        {
            _wordPredictionContext = Context.AppWordPredictionManager.ActiveWordPredictor.LoadContext(TextControlAgent.GetText());
        }

        /// <summary>
        /// Displays scanner according to the window element in the
        /// WordPad window that currently has focus
        /// </summary>
        /// <param name="monitorInfo">Info about currently focused element</param>
        /// <param name="handled">true if handled</param>
        private void displayScanner(WindowActivityMonitorInfo monitorInfo, ref bool handled)
        {
            String panel;

            // Is a menu active?
            if (monitorInfo.FocusedElement.Current.ClassName == "NetUIHWND" &&
                Context.AppAgentMgr.EnableContextualMenusForMenus &&
                (monitorInfo.FocusedElement.Current.ControlType.ProgrammaticName == "ControlType.Pane" ||
                monitorInfo.FocusedElement.Current.ControlType.ProgrammaticName == "ControlType.Window"))
            {
                panel = PanelClasses.MenuContextMenu;
            }
            else
            {
                if (monitorInfo.IsNewWindow)
                {
                    addWordPredictionContext();
                }

                panel = PanelClasses.Alphabet;
            }

            showPanel(this, new PanelRequestEventArgs(panel, monitorInfo));

            handled = true;
        }

        /// <summary>
        /// Parses the title of the notepad window and extracts the
        /// file name from it
        /// </summary>
        /// <returns>filename</returns>
        private String getFileNameFromWindow()
        {
            WindowActivityMonitorInfo info = WindowActivityMonitor.GetForegroundWindowInfo();
            if (info.FgProcess.ProcessName.Equals("wordpad", StringComparison.InvariantCultureIgnoreCase))
            {
                String[] titleParts = info.Title.Split('-');
                if (titleParts.Length > 0)
                {
                    return titleParts[0];
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Launches lecture manager with text from the wordpad window. This
        /// function returns only after lecture manager has exited
        /// </summary>
        /// <returns>task</returns>
        private async Task launchLectureManager()
        {
            IApplicationAgent agent = Context.AppAgentMgr.GetAgentByCategory("LectureManagerAgent");
            if (agent != null)
            {
                IExtension extension = agent;
                extension.GetInvoker().SetValue("LoadFromFile", false);
                String fileName = getFileNameFromWindow();
                if (!String.IsNullOrEmpty(fileName))
                {
                    extension.GetInvoker().SetValue("LectureFile", fileName);
                }

                extension.GetInvoker().SetValue("LectureText", TextControlAgent.GetText());

                await Context.AppAgentMgr.ActivateAgent(agent as IFunctionalAgent);
            }
        }
    }
}