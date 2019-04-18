﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="AlphabetScannerCommon.cs" company="Intel Corporation">
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

using ACAT.ACATResources;
using ACAT.Lib.Core.AgentManagement;
using ACAT.Lib.Core.Audit;
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.PanelManagement.CommandDispatcher;
using ACAT.Lib.Core.Utility;
using ACAT.Lib.Core.WidgetManagement;
using ACAT.Lib.Core.Widgets;
using ACAT.Lib.Extension.CommandHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;

namespace ACAT.Lib.Extension
{
    /// <summary>
    /// This is a helper class exclusively for Alphabet scanners.
    /// It does a lot of the heavy lifting required for word predictions
    /// for instance.  This eases coding for Alphabet scanners.
    /// Create an object of this type in the Alphabet scanner class and call
    /// the functions here when needed
    /// </summary>
    public class AlphabetScannerCommon : IDisposable
    {
        /// <summary>
        /// The parent alphabet scanner form
        /// </summary>
        private readonly Form _form;

        /// <summary>
        /// The scanner panel interface that represents the alphabet
        /// scanner form
        /// </summary>
        private readonly IScannerPanel _scannerPanel;

        /// <summary>
        /// The widget that holds the word currently being typed
        /// </summary>
        private CurrentWordWidget _currentWordWidget;

        /// <summary>
        /// Has this object been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Widget that represents the alphabet scanner
        /// </summary>
        private Widget _rootWidget;

        /// <summary>
        /// The ScannerCommon object for the alphabet scanner
        /// </summary>
        private ScannerCommon _scannerCommon;

        /// <summary>
        /// The scanner helper object for the alphabet scanner
        /// </summary>
        private ScannerHelper _scannerHelper;

        /// <summary>
        /// Widget that holds the prediction word list
        /// </summary>
        private WordListWidget _wordListWidgetWidget;

        /// <summary>
        /// Constructor. Initialize the various controls and
        /// display the UI
        /// </summary>
        public AlphabetScannerCommon(IScannerPanel scannerPanel)
        {
            _scannerPanel = scannerPanel;
            _form = scannerPanel.Form;
            Dispatcher = new CmdDispatcher(this, scannerPanel);
            _scannerCommon = new ScannerCommon(_scannerPanel);
        }

        /// <summary>
        /// For the event raised to format the string that will
        /// be displayed for predicited words
        /// </summary>
        /// <param name="index">index number of the word in the list</param>
        /// <param name="word">the predicted word</param>
        /// <returns>formatted string</returns>
        public delegate String FormatPredictionWord(int index, String word);

        /// <summary>
        /// Raised to format the predicted word
        /// </summary>
        public event FormatPredictionWord EvtFormatPreditionWord;

        /// <summary>
        /// Gets the commanddispatcher object to execute commands.
        /// Call this in the CommandDisatcher property in the alphabet scanner
        /// </summary>
        public RunCommandDispatcher CommandDispatcher
        {
            get { return Dispatcher; }
        }

        /// <summary>
        /// Gets the command dispatcher object. Call this in the
        /// Dispatcher getter in the Alphabet scanner.
        /// </summary>
        public CmdDispatcher Dispatcher { get; private set; }

        /// <summary>
        /// Gets the PanelClass of the scanner. Call this in
        /// the PanelClass getter in the Alphabet scanner.
        /// </summary>
        public String PanelClass { get; private set; }

        /// <summary>
        /// Gets the PanelCommon object
        /// </summary>
        public IPanelCommon PanelCommon { get { return _scannerCommon; } }

        /// <summary>
        /// Gets the current setting of preview mode.
        /// Call this in the PreviewMode property getter in
        ///  the Alphabet scanner.
        /// </summary>
        public bool PreviewMode
        {
            get { return _scannerCommon.PreviewMode; }

            set { _scannerCommon.PreviewMode = value; }
        }

        /// <summary>
        /// Gets the ScannerCommon object.  Call this in
        /// the ScannerCommon getter in the Alphabet scanner.
        /// </summary>
        public ScannerCommon ScannerCommon
        {
            get { return _scannerCommon; }
        }

        /// <summary>
        /// Gets the synch object. Call this in the
        /// SynchObj getter in the Alphabet scanner.
        /// </summary>
        public SyncLock SyncObj
        {
            get { return _scannerCommon.SyncObj; }
        }

        /// <summary>
        /// Gets the text controller object. Call this in
        /// the TextController getter in the Alphabet scanner.
        /// </summary>
        public ITextController TextController
        {
            get { return _scannerCommon.TextController; }
        }

        /// <summary>
        /// Gets or sets how many predicted words to display
        /// </summary>
        public int WordPredictionWordCountMax { get; set; }

        /// <summary>
        /// Autocompletes by selecting the first word in the
        /// word prediciton list
        /// </summary>
        public void AutocompleteWithFirstWord()
        {
            List<Widget> list = new List<Widget>();
            _rootWidget.Finder.FindAllChildren(typeof(WordListItemWidget), list);

            if (list.Any())
            {
                autoComplete(list[0] as WordListItemWidget);
            }
        }

        /// <summary>
        /// Checks if the command in arg needs to be enabled or not
        /// depending on the context.  Call this in the CheckCommandEnabled
        /// function in the Alphabet scanner.
        /// </summary>
        /// <param name="arg">widget info</param>
        /// <returns>true on success</returns>
        public bool CheckCommandEnabled(CommandEnabledArg arg)
        {
            return _scannerHelper.CheckCommandEnabled(arg);
        }

        /// <summary>
        /// Call this in the Dispose() function of the Alphabet scanner
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Call this in the Initialize function of the Alphabet scanner
        /// </summary>
        /// <param name="startupArg">startup arguments</param>
        /// <returns>true on success</returns>
        public bool Initialize(StartupArg startupArg)
        {
            PanelClass = startupArg.PanelClass;

            _scannerHelper = new ScannerHelper(_scannerPanel, startupArg);

            if (!_scannerCommon.Initialize(startupArg))
            {
                Log.Debug("Could not initialize form " + _form.Name);
                return false;
            }

            _rootWidget = PanelCommon.RootWidget;

            return true;
        }

        /// <summary>
        /// Call this in the form's OnClientChanged handler.
        /// Client size changed.  Resize the form to maintain aspect ratio
        /// If the app is DPI aware, the form doesn't scale properly.  The
        /// vertical gets squeezed.  Let's store the design time aspect
        /// ratio and then use it later in the OnLoad to restore the aspect
        /// ratio.
        /// </summary>
        /// <param name="e">event args</param>
        public void OnClientSizeChanged()
        {
            _scannerCommon.OnClientSizeChanged();
        }

        /// <summary>
        /// Release resources and stop threads/timers. Call this in the OnClosing()
        /// function in the Alphabet scanner.
        /// </summary>
        public void OnClosing(object sender, FormClosingEventArgs e)
        {
            Log.Debug();

            KeyStateTracker.EvtKeyStateChanged -= KeyStateTracker_EvtKeyStateChanged;

            _scannerCommon.OnClosing();
            _scannerCommon.Dispose();
        }

        /// <summary>
        /// Invoked when the focus changes either in the active window or when the
        /// active window itself changes. Call this from the OnFocusChanged function
        /// in the Alphabet scanner
        /// </summary>
        /// <param name="inf">Info about focused element</param>
        public void OnFocusChanged(WindowActivityMonitorInfo info)
        {
            _scannerCommon.OnFocusChanged(info);
        }

        /// <summary>
        /// Scanner is closing. Call this in the OnFormClosing
        /// function in the Alphabet scanner.
        /// </summary>
        /// <param name="e">event args</param>
        public void OnFormClosing(FormClosingEventArgs e)
        {
            _scannerHelper.OnFormClosing(e);
            _scannerCommon.OnFormClosing(e);
            Context.AppAgentMgr.EvtTextChanged -= AppAgent_EvtTextChanged;
        }

        /// <summary>
        /// The form has loaded.  Start the animation sequence. Call this from
        /// the OnLoad function in the Alphabet scanner
        /// </summary>
        public void OnLoad(object sender, EventArgs e)
        {
            _scannerCommon.OnLoad();

            subscribeToEvents();

            _currentWordWidget = (CurrentWordWidget)_rootWidget.Finder.FindChild(typeof(CurrentWordWidget));
            _wordListWidgetWidget = (WordListWidget)_rootWidget.Finder.FindChild(typeof(WordListWidget));
            KeyStateTracker.EvtKeyStateChanged += KeyStateTracker_EvtKeyStateChanged;

            if (!_scannerCommon.PreviewMode)
            {
                PanelCommon.AnimationManager.Start(_rootWidget);
            }
            else
            {
                _rootWidget.HighlightOff();
            }

            if (WordPredictionWordCountMax <= 0 && _wordListWidgetWidget != null)
            {
                WordPredictionWordCountMax = _wordListWidgetWidget.Children.Count();
            }

            refreshWordPredictionsAndSetCurrentWord();
        }

        /// <summary>
        /// Pause the application. Call this in the OnPause
        /// function in the Alphabet scanner.
        /// </summary>
        public void OnPause()
        {
            Log.Debug();

            _scannerCommon.OnPause();
        }

        /// <summary>
        /// Resumes the application. Call this in the OnResume
        /// function in the Alphabet scanner.
        /// </summary>
        public void OnResume()
        {
            Log.Debug();

            _scannerCommon.OnResume();

            refreshWordPredictionsAndSetCurrentWord();

            if (!_scannerCommon.DialogMode)
            {
                _scannerCommon.KeepTalkWindowActive = false;
            }
        }

        /// <summary>
        /// Triggered when a widget is actuated. Call this in the OnWidgetActuated
        /// function in the Alphabet scanner.
        /// </summary>
        /// <param name="widget">widget that was actuated</param>
        /// <param name="handled">set to true if handled</param>
        public virtual void OnWidgetActuated(Widget widget, ref bool handled)
        {
            if (widget is WordListItemWidget)
            {
                _form.Invoke(new MethodInvoker(delegate
                {
                    autoComplete(widget as WordListItemWidget);
                }));
                handled = true;
            }
            else
            {
                handled = false;
            }
        }

        /// <summary>
        /// Resizes form to fit the height of the desktop if it
        /// exceeds the size
        /// </summary>
        /// <param name="form">the form</param>
        public void ResizeToFitDesktop(Form form)
        {
            int desktopHeight = Screen.PrimaryScreen.WorkingArea.Height;
            if (form.Height > desktopHeight)
            {
                float ratio = ((float)desktopHeight / form.Height);

                form.Top = 0;
                form.Height = desktopHeight;
                form.Width = (int)((float)form.Width * ratio);
            }
        }

        /// <summary>
        /// Restores default size and position. Call this in the RestoreDefaults
        /// function in the Alphabet scanner.
        /// </summary>
        public void RestoreDefaults()
        {
            _scannerCommon.PositionSizeController.RestoreDefaults();
        }

        /// <summary>
        /// Saves the current scale setting
        /// </summary>
        public void SaveScaleSetting()
        {
            _scannerCommon.PositionSizeController.SaveScaleSetting(ACATPreferences.Load());
        }

        /// <summary>
        /// Saves current size/position settings. Call this in the SaveSettings
        /// function in the Alphabet scanner.
        /// </summary>
        public void SaveSettings()
        {
            _scannerCommon.PositionSizeController.SaveSettings(ACATPreferences.Load());
        }

        /// <summary>
        /// Sets the default scale factor
        /// </summary>
        public void ScaleDefault()
        {
            _scannerCommon.PositionSizeController.ScaleDefault();
        }

        /// <summary>
        /// Zooms out the scanner. Call this in the ScaleDown
        /// function in the Alphabet scanner.
        /// </summary>
        public void ScaleDown()
        {
            _scannerCommon.PositionSizeController.ScaleDown();
        }

        /// <summary>
        /// Zooms in the scanner. Call this in the ScaleUp function in the Alphabet scanner.
        /// </summary>
        public void ScaleUp()
        {
            _scannerCommon.PositionSizeController.ScaleUp();
        }

        /// <summary>
        /// Window proc. Call this in the WndProc
        /// function in the Alphabet scanner.
        /// </summary>
        /// <param name="m">windows message</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public bool WndProc(ref Message m)
        {
            return _scannerCommon.HandleWndProc(m);
        }

        /// <summary>
        /// Disposer. Releases resources and cleanup.
        /// </summary>
        /// <param name="disposing">true to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                Log.Debug();

                if (disposing)
                {
                    // dispose all managed resources.
                }

                // Release unmanaged resources.
            }

            _disposed = true;
        }

        /// <summary>
        /// Something changed in the output text window.  Sest the
        /// word in the "CurrentWordWidget" box and do a fresh
        /// word prediction
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void AppAgent_EvtTextChanged(object sender, EventArgs e)
        {
            Log.Debug();
            try
            {
                if (_form.Visible)
                {
                    refreshWordPredictionsAndSetCurrentWord();
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }

            Log.Debug("returning");
        }

        private void autoComplete(WordListItemWidget wordListWidget)
        {
            Log.Debug("wordListItemName : " + wordListWidget.Name + ", value: " + wordListWidget.Value);

            var wordSelected = wordListWidget.Value.Trim();

            if (!String.IsNullOrEmpty(wordSelected))
            {
                KeyStateTracker.ClearAlt();
                KeyStateTracker.ClearCtrl();

                _scannerCommon.AutoCompleteWord(wordSelected);
                AuditLog.Audit(new AuditEventAutoComplete(wordListWidget.Name));
            }
        }

        /// <summary>
        /// Checks if the word is in the list
        /// </summary>
        /// <param name="list">list of words</param>
        /// <param name="count">how many words?</param>
        /// <param name="word">word to check</param>
        /// <returns>true if it does</returns>
        private bool contains(IEnumerable<String> list, int count, String word)
        {
            for (int ii = 0; ii < count && ii < list.Count(); ii++)
            {
                if (String.Compare(word, list.ElementAt(ii), true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Formats the prediction word with the index
        /// </summary>
        /// <param name="index">index of the word in the list</param>
        /// <param name="word">the word itself</param>
        /// <returns>formatted word</returns>
        private String formatWord(int index, String word)
        {
            string text;
            if (EvtFormatPreditionWord != null)
            {
                text = EvtFormatPreditionWord(index, word);
            }
            else
            {
                text = (index % 10) + ": " + word;
            }

            return text;
        }

        /// <summary>
        /// Some modifier key was pressed. Handled it.
        /// </summary>
        private void KeyStateTracker_EvtKeyStateChanged()
        {
            try
            {
                // turn off select mode.  If select mode is on,
                // as the user moves the cursor, ACAT selects
                // text in the target window
                if (!KeyStateTracker.IsShiftOn())
                {
                    using (AgentContext context = Context.AppAgentMgr.ActiveContext())
                    {
                        context.TextAgent().SetSelectMode(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        /// <summary>
        /// Fetch new set of words into the prediction list box
        /// </summary>
        private void refreshWordPredictionsAndSetCurrentWord()
        {
            // we try it twice in case there is an AgentContext exception
            // the first time around
            if (!tryRefreshWordPredictionsAndSetCurrentWord())
            {
                Log.Debug("AgentContextException.  Retrying refreshing word prediction");
                tryRefreshWordPredictionsAndSetCurrentWord();
            }
        }

        /// <summary>
        /// Subscribes to all the events triggered by the
        /// widgets and the interpreter
        /// </summary>
        private void subscribeToEvents()
        {
            if (_scannerCommon.PreviewMode)
            {
                return;
            }

            subscribeToTalkManagerEvents();
        }

        /// <summary>
        /// Subscribes to events triggered by the Output manager
        /// </summary>
        private void subscribeToTalkManagerEvents()
        {
            Context.AppAgentMgr.EvtTextChanged += AppAgent_EvtTextChanged;
        }

        /// <summary>
        /// Refreshes the word prediction list and also updates the
        /// current word box with the word that is currently being typed.
        /// </summary>
        /// <returns>true onf success</returns>
        private bool tryRefreshWordPredictionsAndSetCurrentWord()
        {
            bool retVal = true;

            if (_wordListWidgetWidget == null)
            {
                return true;
            }

            try
            {
                String wordAtCaret;
                String nwords;
                var charAtCaret = '\0';

                using (var agentContext = Context.AppAgentMgr.ActiveContext())
                {
                    // we need the word at the cursor and also the previous n-words
                    // in the current sentence
                    int caretPos = agentContext.TextAgent().GetCaretPos();
                    Log.Debug("Perform WordPrediction at caretPos " + caretPos);
                    agentContext.TextAgent().GetPrefixAndWordAtCaret(out nwords, out wordAtCaret);
                    Log.Debug("wordAtCaret: [" + wordAtCaret + "]");

                    agentContext.TextAgent().GetCharAtCaret(out charAtCaret);
                    Log.Debug("charAtCaret: [" + charAtCaret + "]");
                }

                if (String.IsNullOrEmpty(nwords) && String.IsNullOrEmpty(wordAtCaret))
                {
                    if (_currentWordWidget != null)
                    {
                        _currentWordWidget.SetCurrentWord(String.Empty);
                    }

                    _wordListWidgetWidget.ClearEntries();

                    if (Common.AppPreferences.SeedWordPredictionOnNewSentence)
                    {
                        nwords = " ";
                        wordAtCaret = String.Empty;
                    }
                    else
                    {
                        return retVal;
                    }
                }

                Log.Debug("wordatcaret length: " + wordAtCaret.Length);

                //Log.Debug("Text: [" + text + "], caretPos: " + caretPos.ToString());

                Log.Debug("Prefix: [" + nwords + "]");
                Log.Debug("CurrentWord: [" + wordAtCaret + "]");

                // do the word prediction
                if (wordAtCaret.Length > 1 && Char.IsPunctuation(wordAtCaret[0]))
                {
                    wordAtCaret = wordAtCaret.Substring(1);
                }

                var predictedWordList = Context.AppWordPredictionManager.ActiveWordPredictor.Predict(nwords, wordAtCaret);

                Log.Debug("predictedWordList count: " + predictedWordList.Count());

                // check if the current word is a possessive word. If not, we need to create
                // a possessive version of the word and add it as the last word
                // in the predicton list.
                var possessiveWord = String.Empty;

                if (!String.IsNullOrEmpty(wordAtCaret) &&
                    R.IsCurrentCultureEnglish() &&
                    Context.AppAgentMgr.CurrentEditingMode == EditingMode.Edit &&
                    !wordAtCaret.EndsWith("'s", StringComparison.InvariantCultureIgnoreCase) &&
                    (charAtCaret == '\0' || charAtCaret == 0x0D || charAtCaret == 0x0A ||
                    TextUtils.IsPunctuationOrWhiteSpace(charAtCaret) ||
                    TextUtils.IsTerminatorOrWhiteSpace(charAtCaret)))
                {
                    possessiveWord = wordAtCaret + "'s";
                }

                int ii = 0;

                foreach (var word in predictedWordList)
                {
                    if (ii >= predictedWordList.Count() || ii >= WordPredictionWordCountMax)
                    {
                        break;
                    }

                    Log.Debug("setting Word for " + ii + ":  " + word);

                    var text = formatWord(ii + 1, word);

                    _wordListWidgetWidget.Children.ElementAt(ii).SetText(text);
                    _wordListWidgetWidget.Children.ElementAt(ii).Value = word;
                    ii++;
                }

                // if there is a possessive form of the word, add
                // it to the list.
                if (!String.IsNullOrEmpty(possessiveWord) &&
                    !contains(predictedWordList, ii, possessiveWord))
                {
                    if (ii == 0)
                    {
                        ii++;
                    }

                    int index = ii - 1;

                    var text = formatWord(index + 1, possessiveWord);

                    _wordListWidgetWidget.Children.ElementAt(index).SetText(text);
                    _wordListWidgetWidget.Children.ElementAt(index).Value = possessiveWord;
                }

                // if we had less than max words, clear out the remaining word list items
                _wordListWidgetWidget.ClearEntries(ii);

                if (_currentWordWidget != null)
                {
                    Log.Debug("Calling SetCurrentWord with : [" + wordAtCaret + "]");
                    _currentWordWidget.SetCurrentWord(wordAtCaret);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
                retVal = false;
            }

            Log.Debug("Returning");
            return retVal;
        }

        /// <summary>
        /// Command dispatcher to handle commands
        /// </summary>
        public class CmdDispatcher : DefaultCommandDispatcher
        {
            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            /// <param name="alphabetScannerCommon">parent object</param>
            /// <param name="panel">the alphabet scanner panel</param>
            public CmdDispatcher(AlphabetScannerCommon alphabetScannerCommon, IScannerPanel panel)
                : base(panel)
            {
                Commands.Add(new CommandHandler(alphabetScannerCommon, "CmdMouseScanner"));
                Commands.Add(new CommandHandler(alphabetScannerCommon, "CmdCursorScanner"));
                Commands.Add(new CommandHandler(alphabetScannerCommon, "CmdPunctuationScanner"));
                Commands.Add(new CommandHandler(alphabetScannerCommon, "CmdWindowPosSizeMenu"));
                Commands.Add(new CommandHandler(alphabetScannerCommon, "CmdNumberScanner"));
                Commands.Add(new CommandHandler(alphabetScannerCommon, "CmdFunctionKeyScanner"));
                Commands.Add(new CommandHandler(alphabetScannerCommon, "CmdAutocompleteWithFirstWord"));
            }
        }

        /// <summary>
        /// Command handler to show other scanners such as Cursor, Punctuations etc.
        /// </summary>
        private class CommandHandler : CreateAndShowScanner
        {
            /// <summary>
            /// The parent object
            /// </summary>
            private readonly AlphabetScannerCommon _alphabetScannerCommon;

            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            /// <param name="alphabetScannerCommon">parent object</param>
            /// <param name="cmd">command to execute</param>
            public CommandHandler(AlphabetScannerCommon alphabetScannerCommon, String cmd)
                : base(cmd)
            {
                _alphabetScannerCommon = alphabetScannerCommon;
            }

            /// <summary>
            /// Executes the command
            /// </summary>
            /// <param name="handled">set to true if handled</param>
            /// <returns>true on success</returns>
            public override bool Execute(ref bool handled)
            {
                handled = true;

                Form form = Dispatcher.Scanner.Form;

                switch (Command)
                {
                    case "CmdAutocompleteWithFirstWord":
                        _alphabetScannerCommon.AutocompleteWithFirstWord();
                        break;

                    case "CmdPunctuationScanner":
                    case "CmdCursorScanner":
                    case "CmdMouseScanner":
                    case "CmdNumberScanner":
                    case "CmdFunctionKeyScanner":
                        // don't close the talk window
                        _alphabetScannerCommon._scannerCommon.KeepTalkWindowActive = true;
                        base.Execute(ref handled);
                        break;

                    case "CmdWindowPosSizeMenu":
                        {
                            Context.AppTalkWindowManager.CloseTalkWindow();
                            var panel = Context.AppPanelManager.CreatePanel("WindowPosSizeMenu", R.GetString("Window")) as IPanel;
                            if (panel != null)
                            {
                                Context.AppPanelManager.Show(form as IPanel, panel);
                            }
                        }
                        break;

                    default:
                        handled = false;
                        break;
                }

                return true;
            }
        }
    }
}