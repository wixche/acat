﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="NotepadAgentSettings.cs" company="Intel Corporation">
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
using ACAT.Lib.Core.PreferencesManagement;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

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

namespace ACAT.Lib.Core.Extensions.Base.AppAgents.NotepadAgent
{
    /// <summary>
    /// Settings for the Microsoft Agent
    /// </summary>
    [Serializable]
    public class  NotepadAgentSettings : AppAgentsPreferencesBase
    {
        /// <summary>
        /// Name of the settings file
        /// </summary>
        [NonSerialized, XmlIgnore]
        public static String PreferencesFilePath;

        /// <summary>
        /// Dock with the Alphabet scanner when window is snapped
        /// </summary>
        [BoolDescriptor("Dock with the Alphabet scanner when window is snapped", true)]
        public bool SnapWindowDockAlphabetScanner = true;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public NotepadAgentSettings()
        {
            AutoSwitchScannerEnable = true;
        }

        /// <summary>
        /// Load settings
        /// </summary>
        /// <returns>settings object</returns>
        public static NotepadAgentSettings Load()
        {
            return PreferencesBase.Load<NotepadAgentSettings>(PreferencesFilePath);
        }

        /// <summary>
        /// Save settings to the preferences file (PreferencesFilePath)
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Save()
        {
            return Save(this, PreferencesFilePath);
        }
    }
}