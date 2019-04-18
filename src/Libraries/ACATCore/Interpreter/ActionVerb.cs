﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ActionVerb.cs" company="Intel Corporation">
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
using System.Collections.Generic;

namespace ACAT.Lib.Core.Interpreter
{
    /// <summary>
    /// An action verb consists of a function name and arguments.
    /// This class encapsulates the attributes of an action verb.
    /// E.g. if the action verb is "transition(TopLevelRotation)",
    /// action name would be "transition" and the argument would be
    /// "TopLevelRotation"

    /// </summary>
    public class ActionVerb
    {
        /// <summary>
        /// Initiates the Action verb class
        /// </summary>
        public ActionVerb()
        {
            ArgList = new List<string>();
        }

        /// <summary>
        /// The name of the function
        ///  eg if the action verb is "transition(TopLevelRotation)",
        ///  action name would be "transition"
        /// </summary>
        public String Action { get; set; }

        /// <summary>
        /// List of arguments for the function
        /// eg if the action verb is "transition(TopLevelRotation)",
        /// arglist would contain the string "TopLevelRotation"
        /// </summary>
        public List<String> ArgList { get; set; }
    }
}