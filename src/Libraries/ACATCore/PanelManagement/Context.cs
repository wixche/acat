﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="Context.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.AbbreviationsManagement;
using ACAT.Lib.Core.ActuatorManagement;
using ACAT.Lib.Core.AgentManagement;
using ACAT.Lib.Core.CommandManagement;
using ACAT.Lib.Core.SpellCheckManagement;
using ACAT.Lib.Core.TalkWindowManagement;
using ACAT.Lib.Core.ThemeManagement;
using ACAT.Lib.Core.TTSManagement;
using ACAT.Lib.Core.Utility;
using ACAT.Lib.Core.WidgetManagement;
using ACAT.Lib.Core.WordPredictionManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ACAT.Lib.Core.PanelManagement
{
    /// <summary>
    /// Encapsulates system wide global shared objects. Creates
    /// instances of all the managers in ACAT.  Since all managers
    /// are singletons, access to them can be made through this class.
    /// Handles initialization of all the different managers in ACAT
    /// such as the TTSManager, WordPrediction manager etc.
    /// There is a sequence to the initialization:
    ///     PreInit()
    ///     Init()
    ///     PostInit()
    ///
    /// </summary>
    public class Context
    {
        private static readonly AbbreviationsManager _abbreviationsManager;
        private static readonly ActuatorManager _actuatorManager;
        private static readonly AgentManager _agentManager;
        private static readonly AutomationEventManager _automationEventManager;
        private static readonly CommandManager _commandManager;
        private static readonly List<String> _extensionDirs = new List<String>();
        private static readonly PanelManager _panelManager;
        private static readonly SpellCheckManager _spellCheckManager;
        private static readonly ThemeManager _themeManager;
        private static readonly TTSManager _ttsManager;
        private static readonly WordPredictionManager _wordPredictionManager;

        /// <summary>
        /// Error message if there was an error during initialization
        /// </summary>
        private static String _completionStatus = String.Empty;

        /// <summary>
        /// Was there a warning (non-fatal) during initialization
        /// </summary>
        private static bool _initWarning;

        /// <summary>
        /// Did a fatal error occur during initialization?
        /// </summary>
        private static bool _isFatal;

        /// <summary>
        /// Which modules should be initialized
        /// </summary>
        private static StartupFlags _startupFlags = StartupFlags.All;

        /// <summary>
        /// The Talk window manager
        /// </summary>
        private static TalkWindowManager _talkManager;

        /// <summary>
        /// Initializes the singleton instance of the class
        /// </summary>
        static Context()
        {
            AppQuit = false;
            AppWindowPosition = Windows.WindowPosition.MiddleRight;

            //Initialize all the manager singleton objects
            _abbreviationsManager = AbbreviationsManager.Instance;
            _actuatorManager = ActuatorManager.Instance;
            _agentManager = AgentManager.Instance;
            _panelManager = PanelManager.Instance;
            _ttsManager = TTSManager.Instance;
            _automationEventManager = AutomationEventManager.Instance;
            _wordPredictionManager = WordPredictionManager.Instance;
            _spellCheckManager = SpellCheckManager.Instance;
            _themeManager = ThemeManager.Instance;
            _commandManager = CommandManager.Instance;
        }

        /// <summary>
        /// Raised when the culture changes
        /// </summary>
        public static event CultureChanged EvtCultureChanged;

        /// <summary>
        /// Which modules to activate?
        /// </summary>
        [Flags]
        public enum StartupFlags
        {
            Minimal = 0,
            WordPrediction = 1,
            WindowsActivityMonitor = 2,
            TextToSpeech = 4,
            SpellChecker = 16,
            Abbreviations = 32,
            AgentManager = 64,
            All = 0xffff
        }

        /// <summary>
        /// Gets the single Abbreviations Manager object
        /// </summary>
        public static AbbreviationsManager AppAbbreviationsManager
        {
            get { return _abbreviationsManager; }
        }

        /// <summary>
        /// Gets the ACAT ActuatorManager object
        /// </summary>
        public static ActuatorManager AppActuatorManager
        {
            get { return _actuatorManager; }
        }

        /// <summary>
        /// Gets the ACAT Application Agent Manager object
        /// </summary>
        public static AgentManager AppAgentMgr
        {
            get { return _agentManager; }
        }

        /// <summary>
        /// Gets the ACAT Automation Event Manager object
        /// </summary>
        public static AutomationEventManager AppAutomationEventManger
        {
            get { return _automationEventManager; }
        }

        public static CommandManager AppCommandManager
        {
            get { return _commandManager; }
        }

        /// <summary>
        /// Gets the ACAT PanelManager object
        /// </summary>
        public static PanelManager AppPanelManager
        {
            get { return _panelManager; }
        }

        /// <summary>
        /// Gets or sets whether the application should quit
        /// </summary>
        public static bool AppQuit { get; set; }

        /// <summary>
        /// Gets the ACAT Spellcheck Manager
        /// </summary>
        public static SpellCheckManager AppSpellCheckManager
        {
            get { return _spellCheckManager; }
        }

        /// <summary>
        /// Gets the ACAT TalkWindowManager object
        /// </summary>
        public static TalkWindowManager AppTalkWindowManager
        {
            get { return _talkManager; }
        }

        /// <summary>
        /// Gets the ACAT Theme Manager object
        /// </summary>
        public static ThemeManager AppThemeManager
        {
            get { return _themeManager; }
        }

        /// <summary>
        /// Gets the ACAT Text to speech Manager object
        /// </summary>
        public static TTSManager AppTTSManager
        {
            get { return _ttsManager; }
        }

        /// <summary>
        /// Gets or sets the current scanner position
        /// </summary>
        public static Windows.WindowPosition AppWindowPosition { get; set; }

        /// <summary>
        /// Gets the ACAT WordPredictionManager object
        /// </summary>
        public static WordPredictionManager AppWordPredictionManager
        {
            get { return _wordPredictionManager; }
        }

        /// <summary>
        /// Gets the list of extension directories
        /// </summary>
        public static IEnumerable<String> ExtensionDirs
        {
            get
            {
                if (!_extensionDirs.Any())
                {
                    getExtensionDirs();
                }

                return _extensionDirs;
            }
        }

        /// <summary>
        /// Gets or sets whether the talk windows should be displayed
        /// when the application is launched
        /// </summary>
        public static bool ShowTalkWindowOnStartup { get; set; }

        /// <summary>
        /// Changes the culture to the specified culture
        /// </summary>
        /// <param name="culture">culture to change to</param>
        public static void ChangeCulture(CultureInfo cultureInfo)
        {
            var culture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            ResourceUtils.InstallLanguageForUser();

            if (EvtCultureChanged != null)
            {
                WindowActivityMonitor.Pause();
                EvtCultureChanged(cultureInfo, new CultureChangedEventArg(cultureInfo));
                WindowActivityMonitor.Resume();
            }
        }

        /// <summary>
        /// Disposes allocated resources
        /// </summary>
        public static void Dispose()
        {
            if (AppActuatorManager != null)
            {
                AppActuatorManager.Dispose();
            }

            if (AppPanelManager != null)
            {
                AppPanelManager.Dispose();
            }

            if (AppWordPredictionManager != null)
            {
                AppWordPredictionManager.Dispose();
            }

            if (AppTTSManager != null)
            {
                AppTTSManager.Dispose();
            }

            if (AppSpellCheckManager != null)
            {
                AppSpellCheckManager.Dispose();
            }

            if (AppAbbreviationsManager != null)
            {
                AppAbbreviationsManager.Dispose();
            }

            if (AppAgentMgr != null)
            {
                AppAgentMgr.Dispose();
            }

            if (AppAutomationEventManger != null)
            {
                AppAutomationEventManger.Dispose();
            }

            WindowActivityMonitor.Dispose();
        }

        /// <summary>
        /// Returns the string that contains the completion status
        /// (the error message for example if init failed)
        /// </summary>
        /// <returns>Status string</returns>
        public static String GetInitCompletionStatus()
        {
            return _completionStatus;
        }

        /// <summary>
        /// Performs initialization.  Depending on the startup
        /// flags, only loads those modules that have been specified.
        /// </summary>
        /// <param name="startup">Which modules to init?</param>
        /// <returns></returns>
        public static bool Init(StartupFlags startup = StartupFlags.All)
        {
            getExtensionDirs();

            _startupFlags = startup;

            AppWindowPosition = CoreGlobals.AppPreferences.ScannerPosition;

            AppCommandManager.Init();

            bool retVal = createThemeManager();

            if (retVal)
            {
                retVal = createPanelManager();
            }

            if (retVal)
            {
                retVal = initWidgetManager();
            }

            if (retVal)
            {
                retVal = createWordPredictionManager();
            }

            if (retVal)
            {
                retVal = createTTSManager();
            }

            if (retVal)
            {
                retVal = createSpellCheckManager();
            }

            if (retVal)
            {
                retVal = createAbbreviationsManager();
            }

            createTalkWindowManager();

            if (retVal)
            {
                retVal = createAgentManager();
            }

            if (retVal)
            {
                retVal = createActuatorManager();
            }

            if (_initWarning)
            {
                retVal = false;
            }

            Log.Debug("Returning " + retVal + " from context init");
            return retVal;
        }

        /// <summary>
        /// Returns whether initialization error was fatal
        /// </summary>
        /// <returns></returns>
        public static bool IsInitFatal()
        {
            return _isFatal;
        }

        /// <summary>
        /// Call this AFTER the Init function
        /// </summary>
        public static bool PostInit()
        {
            if (isEnabled(StartupFlags.AgentManager))
            {
                AppAgentMgr.PostInit();
            }

            if (isEnabled(StartupFlags.WindowsActivityMonitor))
            {
                WindowActivityMonitor.GetActiveWindow();
                WindowActivityMonitor.Start();
            }

            return true;
        }

        /// <summary>
        /// Call this BEFORE calling the Init() function.
        /// </summary>
        /// <returns>true on success</returns>
        public static bool PreInit()
        {
            return true;
        }

        /// <summary>
        /// Creates the singleton instance of the Abbreviations manager
        /// </summary>
        /// <returns>true on success</returns>
        private static bool createAbbreviationsManager()
        {
            bool retVal = true;

            if (isEnabled(StartupFlags.Abbreviations))
            {
                retVal = AppAbbreviationsManager.Init();

                if (!retVal)
                {
                    setCompletionStatus("Abbreviations load error.  Abbreviations will be disabled", false);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Creates the singleton instance of the Actuator manager
        /// </summary>
        /// <returns>true on success</returns>
        private static bool createActuatorManager()
        {
            bool retVal = AppActuatorManager.LoadExtensions(ExtensionDirs);

            if (retVal)
            {
                retVal = AppActuatorManager.Init(ExtensionDirs);
            }

            if (!retVal)
            {
                setCompletionStatus("Error initializing actuator manager");
            }

            return retVal;
        }

        /// <summary>
        /// Initializes the singleton instance of the Agent manager
        /// </summary>
        /// <returns></returns>
        private static bool createAgentManager()
        {
            bool retVal = true;

            if (isEnabled(StartupFlags.AgentManager))
            {
                retVal = AppAgentMgr.LoadExtensions(ExtensionDirs);
                if (retVal)
                {
                    retVal = AppAgentMgr.Init(ExtensionDirs);
                }

                if (!retVal)
                {
                    setCompletionStatus("Error initializing the Agent manager");
                }
            }

            return retVal;
        }

        /// <summary>
        /// Creates the singleton instance of the Screen manager
        /// </summary>
        /// <returns>true on success</returns>
        private static bool createPanelManager()
        {
            bool retVal = AppPanelManager.Init(ExtensionDirs);
            if (!retVal)
            {
                setCompletionStatus("Error initializing Panel Manager");
            }

            return retVal;
        }

        /// <summary>
        /// Creates the singleton instance of the SpellCheck manager
        /// </summary>
        /// <returns>true on success</returns>
        private static bool createSpellCheckManager()
        {
            bool retVal = true;

            if (isEnabled(StartupFlags.SpellChecker))
            {
                retVal = AppSpellCheckManager.LoadExtensions(ExtensionDirs);
                if (retVal)
                {
                    retVal = AppSpellCheckManager.Init(ExtensionDirs);
                }

                if (retVal)
                {
                    retVal = AppSpellCheckManager.SetActiveSpellChecker();
                    if (!retVal)
                    {
                        setCompletionStatus("Error setting active spell checker");
                    }
                }
                else
                {
                    setCompletionStatus("Error initializing SpellCheck Manager");
                }
            }

            return retVal;
        }

        /// <summary>
        /// Creates the talk window manager
        /// </summary>
        private static void createTalkWindowManager()
        {
            _talkManager = TalkWindowManager.Instance;
        }

        /// <summary>
        /// Creates the singleton instance of the Theme manager
        /// </summary>
        /// <returns>true on success</returns>
        private static bool createThemeManager()
        {
            bool retVal = AppThemeManager.Init();
            if (retVal)
            {
                AppThemeManager.SetActiveTheme(CoreGlobals.AppPreferences.Theme);
            }
            else
            {
                setCompletionStatus("Error initializing Theme Manager");
            }

            return retVal;
        }

        /// <summary>
        /// Creates the singleton instance of the Text-to-Speech manager
        /// </summary>
        /// <returns>true on success</returns>
        private static bool createTTSManager()
        {
            bool retVal = true;

            if (isEnabled(StartupFlags.TextToSpeech) && CoreGlobals.AppPreferences.EnableTextToSpeech)
            {
                retVal = AppTTSManager.LoadExtensions(ExtensionDirs);
                if (retVal)
                {
                    retVal = AppTTSManager.Init(ExtensionDirs);
                }

                if (retVal)
                {
                    retVal = AppTTSManager.SetActiveEngine();
                }

                if (!retVal)
                {
                    setCompletionStatus("Error initializing text to speech.  TTS will be disabled", false);
                    retVal = true;
                    _initWarning = true;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Creates the singleton instance of the Word Prediction manager
        /// </summary>
        /// <returns>true on success</returns>
        private static bool createWordPredictionManager()
        {
            bool retVal = true;

            if (isEnabled(StartupFlags.WordPrediction))
            {
                retVal = AppWordPredictionManager.LoadExtensions(ExtensionDirs);

                if (retVal)
                {
                    retVal = AppWordPredictionManager.Init(ExtensionDirs);
                }

                if (retVal)
                {
                    retVal = AppWordPredictionManager.SetActiveWordPredictor();
                    if (!retVal)
                    {
                        setCompletionStatus("Error setting active word prediction engine");
                    }
                }
                else
                {
                    setCompletionStatus("Error initializing Word Predition engine");
                }
            }

            return retVal;
        }

        /// <summary>
        /// Reads the config setting for extension directories,
        /// resolves them into full directory paths, checks if they
        /// are valid and stores the resultant set of valid directories
        /// in _extensionDirs
        /// </summary>
        private static void getExtensionDirs()
        {
            _extensionDirs.Clear();
            var dirs = CoreGlobals.AppPreferences.Extensions.Split(',');
            var extensionDirRootPath = FileUtils.GetExtensionDir();
            if (Directory.Exists(extensionDirRootPath))
            {
                foreach (var str in dirs)
                {
                    var dir = str.Trim();
                    if (Path.IsPathRooted(dir))
                    {
                        _extensionDirs.Add(dir);
                    }
                    else
                    {
                        var fullPath = FileUtils.GetExtensionDir(dir);
                        if (Directory.Exists(fullPath))
                        {
                            var attr = File.GetAttributes(fullPath);
                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                Log.Debug("Adding Extensiondir " + fullPath);
                                _extensionDirs.Add(fullPath);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize the widget manager
        /// </summary>
        /// <returns>true on success</returns>
        private static bool initWidgetManager()
        {
            bool retVal = WidgetManager.Init(ExtensionDirs);
            if (!retVal)
            {
                setCompletionStatus("Error initializing Widget Manager");
            }

            return retVal;
        }

        /// <summary>
        /// Checks if the specified flag is in startup
        /// </summary>
        /// <param name="flag">Flag to check for</param>
        /// <returns>true if it is</returns>
        private static bool isEnabled(StartupFlags flag)
        {
            return (_startupFlags & flag) == flag;
        }

        /// <summary>
        /// Sets the completion status string. This could be an
        /// error message for instance
        /// </summary>
        /// <param name="status">status string</param>
        /// <param name="fatal">is the error fatal?</param>
        private static void setCompletionStatus(String status, bool fatal = true)
        {
            _completionStatus = (fatal) ? "Fatal error. " + status : status;
            _isFatal = fatal;
        }
    }
}