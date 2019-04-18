﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="IRunCommandHandler.cs" company="Intel Corporation">
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

using System;

namespace ACAT.Lib.Core.PanelManagement.CommandDispatcher
{
    /// <summary>
    /// Interface for commands that will be executed
    /// </summary>
    public interface IRunCommandHandler
    {
        /// <summary>
        /// Gets or sets the command verb to execute
        /// </summary>
        String Command { get; set; }

        /// <summary>
        /// Command to run after the command executing the "Command"
        /// </summary>
        PostExitCommand Status { get; set; }

        /// <summary>
        /// Executes the command. Handled is set to
        /// true if the command was handled. False otherwise.
        /// If false, the command is passed on to next handler in the
        /// command chain
        /// </summary>
        /// <param name="handled">true if handled</param>
        /// <returns>true on success</returns>
        bool Execute(ref bool handled);
    }
}