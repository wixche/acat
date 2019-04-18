﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="AgentProcessInfo.cs" company="Intel Corporation">
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

namespace ACAT.Lib.Core.AgentManagement
{
    /// <summary>
    /// Represents process information used to identify which processes
    /// an agent supports
    /// </summary>
    public class AgentProcessInfo
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="name">name of the process</param>
        public AgentProcessInfo(String name)
        {
            Name = name;
            ExecutablePath = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="name">name of the process</param>
        /// <param name="exe">full path to the exe</param>
        public AgentProcessInfo(String name, String exe)
        {
            Name = name;
            ExecutablePath = exe;
        }

        /// <summary>
        /// Optional.  Path to the executable.  (What if there
        /// is another app called notepad?  Use this to ensure
        /// that it is the Windows notepad.
        /// </summary>
        public String ExecutablePath { get; set; }

        /// <summary>
        /// Name of the process. Eg. winword, notepad etc
        /// </summary>
        public String Name { get; set; }
    }
}