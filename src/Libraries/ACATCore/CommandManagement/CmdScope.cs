﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="CmdScope.cs" company="Intel Corporation">
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

namespace ACAT.Lib.Core.CommandManagement
{
    /// <summary>
    /// Defines the scope of a command, i.e. where is the
    /// command valid?  ScopeTypes defines the various scopes
    /// </summary>
    [Serializable]
    public class CmdScope
    {
        /// <summary>
        /// Initializes an instance of the class
        /// </summary>
        public CmdScope()
        {
            ScopeType = ScopeTypes.NotSpecified;
            ScopeValue = String.Empty;
            Enabled = false;
        }

        /// <summary>
        /// Initializes an instance of the class
        /// </summary>
        /// <param name="scopeType">Scope Type</param>
        /// <param name="scopeValue">Value for the scope</param>
        /// <param name="enabled">Is this enabled or not</param>
        public CmdScope(ScopeTypes scopeType, String scopeValue = null, bool enabled = true)
        {
            ScopeType = scopeType;
            ScopeValue = scopeValue;
            Enabled = enabled;
        }

        /// <summary>
        /// Types of scopes
        /// </summary>
        public enum ScopeTypes
        {
            /// <summary>
            /// Unknown
            /// </summary>
            NotSpecified,

            /// <summary>
            /// Valid anywhere in ACAT - in scanners, 
            /// menus, dialogs 
            /// </summary>
            Global,

            /// <summary>
            /// Valid only for the specified PanelCategory i.e.
            /// Scanner, Menu, Dialog etc.  The ScopeValue contains
            /// the value "Scanner", "Menu" etc.  The value should be 
            /// one of the valid values of the PanelCategory enum
            /// </summary>
            PanelCategory,

            /// <summary>
            /// Valid only for the specified PanelClass eg AlphabetScanner,
            /// ScopeValue should contain the PanelClass
            /// </summary>
            PanelClass,

            /// <summary>
            /// Valid only for a specific panelinstance i.e., a specific 
            /// combination of the panel class and the animation file.  In 
            /// this case, the ScopeValue should contain the ConfigId
            /// (which is a GUID) from PanelConfigMap.xml
            /// </summary>
            PanelInstance
        }

        /// <summary>
        /// Gets or sets whether this is enabled or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the scope type
        /// </summary>
        public ScopeTypes ScopeType { get; set; }

        /// <summary>
        /// Gets or sets the scope value
        /// </summary>
        public String ScopeValue { get; set; }
    }
}