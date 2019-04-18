﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="AuditEventActiveWindowChange.cs" company="Intel Corporation">
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

namespace ACAT.Lib.Core.Audit
{
    /// <summary>
    /// Represents log entry for auditing context switch to
    /// another window.
    /// </summary>
    public class AuditEventActiveWindowChange : AuditEventBase
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public AuditEventActiveWindowChange()
            : base("ActiveWindowChange")
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="processName">name of the new process</param>
        /// <param name="windowTitle">title of the new windw</param>
        public AuditEventActiveWindowChange(string processName, string windowTitle)
            : base("ActiveWindowChange")
        {
            ProcessName = processName;
            WindowTitle = windowTitle;
        }

        /// <summary>
        /// Gets name of the process
        /// </summary>
        public String ProcessName { get; set; }

        /// <summary>
        /// Gets title of the window
        /// </summary>
        public String WindowTitle { get; set; }

        /// <summary>z
        /// Converts to string
        /// </summary>
        /// <returns>string representation</returns>
        protected override string toString()
        {
            return formatEventString(ProcessName, WindowTitle);
        }
    }
}