﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ACATAgent.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.Utility;
using ACAT.Lib.Extension.AppAgents.ACATApp;

namespace ACAT.Extensions.Default.AppAgents.ACATApp
{
    /// <summary>
    /// Application agent for the ACAT application.
    /// </summary>
    [DescriptorAttribute("066A06E9-7178-4058-A6BC-CFA803A67088",
                            "ACAT Agent",
                            "Application Agent for the executing assembly")]
    internal class ACATAgent : ACATAgentBase
    {
    }
}