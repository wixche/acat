﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="FileOperationConfirmScanner.cs" company="Intel Corporation">
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
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.PanelManagement.CommandDispatcher;
using ACAT.Lib.Core.Utility;
using ACAT.Lib.Extension;
using System;
using System.IO;

namespace ACAT.Extensions.Default.FunctionalAgents.FileBrowserAgent
{
    /// <summary>
    /// Displays a menu with options to confirm whether the
    /// user should open the file or delete it
    /// </summary>
    [DescriptorAttribute("472AA253-2FB4-4FFC-A763-42C1717D5DF4",
                        "FileOperationConfirmScanner",
                        "File Browser Operation Confirm Scanner")]
    public partial class FileOperationConfirmScanner : MenuPanel
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="panelClass">the panel name</param>
        /// <param name="panelTitle">title of the form</param>
        public FileOperationConfirmScanner(String panelClass, String panelTitle)
            : base(panelClass, panelTitle)
        {
            commandDispatcher.Commands.Add(new CommandHandler(this, "CancelFileOperation"));
            commandDispatcher.Commands.Add(new CommandHandler(this, "OpenFile"));
            commandDispatcher.Commands.Add(new CommandHandler(this, "DeleteFile"));
        }

        /// <summary>
        /// Gets or sets a value whether the user wants to delete
        /// </summary>
        public bool DeleteFile { get; set; }

        /// <summary>
        /// Gets or sets the FileInfo object associated with the file
        /// </summary>
        public FileInfo FInfo { get; set; }

        /// <summary>
        /// Gets or sets whether the user wants to open
        /// </summary>
        public bool OpenFile { get; set; }

        /// <summary>
        /// Gets confirmation whether the user wants to delete the file
        /// and closes the form
        /// </summary>
        private void handleDeleteFile()
        {
            if (FInfo != null)
            {
                if (!DialogUtils.ConfirmScanner(PanelManager.Instance.GetCurrentForm(),
                                                String.Format(R.GetString("DeleteFileQ"), FInfo.Name)))
                {
                    return;
                }
            }

            DeleteFile = true;
            Windows.CloseForm(this);
        }

        /// <summary>
        /// Gets confirmation whether the user wants to open the file
        /// and closes the form
        /// </summary>
        private void handleOpenFile()
        {
            if (FInfo != null)
            {
                if (!DialogUtils.ConfirmScanner(PanelManager.Instance.GetCurrentForm(),
                                                String.Format(R.GetString("OpenFileQ"), FInfo.Name)))
                {
                    return;
                }
            }

            OpenFile = true;
            Windows.CloseForm(this);
        }

        /// <summary>
        /// Handles commands associated with the items in the
        /// contextual menu
        /// </summary>
        private class CommandHandler : RunCommandHandler
        {
            private readonly FileOperationConfirmScanner _menu;

            /// <summary>
            /// Initializes a new instance of the class
            /// </summary>
            /// <param name="contextualMenu">the contextual menu</param>
            /// <param name="cmd">command to execute</param>
            public CommandHandler(FileOperationConfirmScanner contextualMenu, String cmd)
                : base(cmd)
            {
                _menu = contextualMenu;
            }

            /// <summary>
            /// Executes a command
            /// </summary>
            /// <param name="handled">true if it was handled</param>
            /// <returns>true on success</returns>
            public override bool Execute(ref bool handled)
            {
                handled = true;

                switch (Command)
                {
                    case "CancelFileOperation":
                        _menu.commandDispatcher.Dispatch("CmdGoBack", ref handled);
                        break;

                    case "OpenFile":
                        _menu.handleOpenFile();
                        break;

                    case "DeleteFile":
                        _menu.handleDeleteFile();
                        break;

                    default:
                        handled = false;
                        break;
                }

                return true;
            }
        }
    }
}