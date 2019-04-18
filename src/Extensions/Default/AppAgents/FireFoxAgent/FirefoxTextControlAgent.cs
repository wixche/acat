﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="FirefoxTextControlAgent.cs" company="Intel Corporation">
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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Automation;
using ACAT.Lib.Core.AgentManagement.TextInterface;

#region SupressStyleCopWarnings

[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1126:PrefixCallsCorrectly",
        Scope = "namespace",
        Justification = "Not needed. ACAT naming conventions takes care of this")]
[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1101:PrefixLocalCallsWithThis",
        Scope = "namespace",
        Justification = "Not needed. ACAT naming conventions takes care of this")]
[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1121:UseBuiltInTypeAlias",
        Scope = "namespace",
        Justification = "Since they are just aliases, it doesn't really matter")]
[module: SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1200:UsingDirectivesMustBePlacedWithinNamespace",
        Scope = "namespace",
        Justification = "ACAT guidelines")]
[module: SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1309:FieldNamesMustNotBeginWithUnderscore",
        Scope = "namespace",
        Justification = "ACAT guidelines. Private fields begin with an underscore")]
[module: SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Scope = "namespace",
        Justification = "ACAT guidelines. Private/Protected methods begin with lowercase")]

#endregion SupressStyleCopWarnings

namespace ACAT.Lib.Core.Extensions.Base.AppAgents.FireFoxAgent
{
    /// <summary>
    /// Text control agent for the firefox browser.  We want to disallow
    /// expanding abbreviations and also disallow auto-correct
    /// </summary>
    public class FireFoxTextControlAgent : EditTextControlAgent
    {
        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="handle">Handle to the browser window</param>
        /// <param name="editControlElement">currently focused element</param>
        /// <param name="handled">set to true if handled</param>
        public FireFoxTextControlAgent(IntPtr handle, AutomationElement editControlElement, ref bool handled)
            : base(handle, editControlElement, ref handled)
        {
        }

        /// <summary>
        /// Disallow expansion of abbreviations
        /// </summary>
        /// <returns>always returns false</returns>
        public override bool ExpandAbbreviations()
        {
            return false;
        }

        /// <summary>
        /// Returns whether spell check is supported by this agent. Return
        /// true so ACAT"s spellchecker doesn't kick in.
        /// </summary>
        /// <returns>always returns true</returns>
        public override bool SupportsSpellCheck()
        {
            return true;
        }
    }
}