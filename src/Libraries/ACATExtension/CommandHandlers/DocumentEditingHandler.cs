﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="DocumentEditingHandler.cs" company="Intel Corporation">
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
using ACAT.Lib.Core.PanelManagement.CommandDispatcher;
using ACAT.Lib.Core.Utility;
using System;
using System.Windows.Forms;

namespace ACAT.Lib.Extension.CommandHandlers
{
    /// <summary>
    /// Takes care of document editing operations such as
    /// selecting text, clipboard operations, deleting
    /// words, characters etc.
    /// </summary>
    public class DocumentEditingHandler : RunCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="cmd">The command to be executed</param>
        public DocumentEditingHandler(String cmd)
            : base(cmd)
        {
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="handled">set to true if the command was handled</param>
        /// <returns>true on success</returns>
        public override bool Execute(ref bool handled)
        {
            bool retVal = true;

            handled = true;

            try
            {
                using (AgentContext context = Context.AppAgentMgr.ActiveContext())
                {
                    switch (Command)
                    {
                        case "CmdUndo":
                            context.TextAgent().Undo();
                            break;

                        case "CmdRedo":
                            context.TextAgent().Redo();
                            break;

                        case "CmdSelectModeToggle":
                            context.TextAgent().SetSelectMode(!context.TextAgent().GetSelectMode());
                            break;

                        case "CmdFind":
                            Context.AppAgentMgr.RunCommand("CmdFind", ref handled);
                            break;

                        case "CmdSelectAll":
                            context.TextAgent().SelectAll();
                            break;

                        case "CmdDeletePrevChar":
                            Context.AppAgentMgr.Keyboard.Send(Keys.Back);
                            break;

                        case "CmdDeleteNextChar":
                            Context.AppAgentMgr.Keyboard.Send(Keys.Delete);
                            break;

                        case "CmdDeletePrevWord":
                            Dispatcher.Scanner.TextController.SmartDeletePrevWord();
                            break;

                        case "CmdCut":
                            context.TextAgent().Cut();
                            turnOffSelectMode();  // TODO move to cursor scanner
                            break;

                        case "CmdCopy":
                            context.TextAgent().Copy();
                            turnOffSelectMode();
                            break;

                        case "CmdPaste":
                            context.TextAgent().Paste();
                            turnOffSelectMode();
                            break;

                        case "CmdUndoLastEditChange":
                            Dispatcher.Scanner.TextController.UndoLastEditChange();
                            break;

                        default:
                            handled = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// Turns off the select mode.
        /// </summary>
        private void turnOffSelectMode()
        {
            KeyStateTracker.ClearAll();

            try
            {
                using (AgentContext context = Context.AppAgentMgr.ActiveContext())
                {
                    context.TextAgent().SetSelectMode(false);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}