﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="WordPadAgentSettings.cs" company="Intel Corporation">
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
using System.Xml.Serialization;

namespace ACAT.Lib.Core.Extensions.Base.AppAgents.WordPadAgent
{
    /// <summary>
    /// Settings for the WordPad Agent
    /// </summary>
    [Serializable]
    public class WordPadAgentSettings : AppAgentsPreferencesBase
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
        public WordPadAgentSettings()
        {
            AutoSwitchScannerEnable = true;
        }

        /// <summary>
        /// Load settings
        /// </summary>
        /// <returns>settings object</returns>
        public static WordPadAgentSettings Load()
        {
            return PreferencesBase.Load<WordPadAgentSettings>(PreferencesFilePath);
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