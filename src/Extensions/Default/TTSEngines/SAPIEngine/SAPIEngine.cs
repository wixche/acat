﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="SAPIEngine.cs" company="Intel Corporation">
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

using ACAT.Lib.Core.Extensions;
using ACAT.Lib.Core.PreferencesManagement;
using ACAT.Lib.Core.TTSManagement;
using ACAT.Lib.Core.UserManagement;
using ACAT.Lib.Core.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Speech.Synthesis;

namespace ACAT.Extensions.Default.TTSEngines.SAPIEngine
{
    /// <summary>
    /// Converts text to speech by sending the text string to the
    /// Microsoft Speech Synthesizer
    /// </summary>
    [DescriptorAttribute("B7AB6188-AE23-40E3-9E6A-F8AA8A81E2BF",
                        "Speech Synthesizer TTS Engine",
                        "Text to Speech based on the Microsoft Speech Synthesizer")]
    public class SAPIEngine : ExtensionInvoker, ITTSEngine, ISupportsPreferences
    {
        /// <summary>
        /// Settings for this engine
        /// </summary>
        internal static SAPISettings SAPISettings;

        /// <summary>
        /// Max value of rate of speech
        /// </summary>
        private const int MaxRate = 10;

        /// <summary>
        /// Max value of speech volume
        /// </summary>
        private const int MaxVolume = 100;

        /// <summary>
        /// Minimum value of rate of speech
        /// </summary>
        private const int MinRate = -10;

        /// <summary>
        /// Minimum value of speech volume
        /// </summary>
        private const int MinVolume = 0;

        /// <summary>
        /// Name of the settings file
        /// </summary>
        internal const String SettingsFileName = "SAPIEngineSettings.xml";

        /// <summary>
        /// Has this object been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Is speech muted?
        /// </summary>
        private bool _muted;

        /// <summary>
        /// Next value of the bookmark to generate
        /// </summary>
        private int _nextBookmark = 1;

        /// <summary>
        /// Volume level before Mute was set
        /// </summary>
        private int _preMuteVolumeLevel;

        /// <summary>
        /// The alternate pronunciations file
        /// </summary>
        private Pronunciations _pronunciations;

        /// <summary>
        /// The Microsoft speech synthesizer object
        /// </summary>
        private SpeechSynthesizer _speechSynthesizer;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public SAPIEngine()
        {
            SAPISettings.PreferencesFilePath = UserManager.GetFullPath(SettingsFileName);
            SAPISettings = SAPISettings.Load();

            Synthesizer.SetOutputToDefaultAudioDevice();
            Synthesizer.BookmarkReached += speechSynthesizer_BookmarkReached;
        }

#pragma warning disable

        /// <summary>
        /// Triggered when bookmark reached after async text to speech
        /// </summary>
        public event TTSBookmarkReached EvtBookmarkReached;

        /// <summary>
        /// Raised when one of the properties changes
        /// </summary>
        public event EventHandler EvtPropertyChanged;

        /// <summary>
        /// Triggered when the status of the speech engine changes
        /// </summary>
        public event TTSStatusChanged EvtStatusChanged;

        /// <summary>
        /// Triggered when voice is changed
        /// </summary>
        public event TTSVoiceChanged EvtVoiceChanged;

#pragma warning enable

        /// <summary>
        /// Gets or sets the culture info for the voice to use
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets the descriptor for this class
        /// </summary>
        public IDescriptor Descriptor
        {
            get { return DescriptorAttribute.GetDescriptor(GetType()); }
        }

        /// <summary>
        /// Returns the current status of the speech engine, whether
        /// it is currently speaking or paused or something else
        /// </summary>
        public StatusFlags Status
        {
            get
            {
                switch (Synthesizer.State)
                {
                    case SynthesizerState.Speaking:
                        return StatusFlags.Speaking;

                    case SynthesizerState.Paused:
                        return StatusFlags.Paused;

                    default:
                        return StatusFlags.None;
                }
            }
        }

        /// <summary>
        /// Gets whether this supports a custom settings dialog
        /// </summary>
        public virtual bool SupportsPreferencesDialog
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the voice to use. Not supported
        /// </summary>
        public String Voice
        {
            get { return String.Empty; }

            set { }
        }

        /// <summary>
        /// Gets the microsoft speech synthesizer
        /// object
        /// </summary>
        private SpeechSynthesizer Synthesizer
        {
            get { return _speechSynthesizer ?? (_speechSynthesizer = new SpeechSynthesizer {Rate = 0, Volume = 100}); }
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns the default preferences (factory settings)
        /// </summary>
        /// <returns>Default preferences object</returns>
        public IPreferences GetDefaultPreferences()
        {
            return SAPISettings.LoadDefaults<SAPISettings>();
        }

        /// <summary>
        /// Pitch of speech.  Value is engine-dependent. SAPI
        /// doesn't support it
        /// </summary>
        public TTSValue GetPitch()
        {
            return new TTSValue(0, 0, 0);
        }

        /// <summary>
        /// Returns the preferences object
        /// </summary>
        /// <returns>The preferences object</returns>
        public IPreferences GetPreferences()
        {
            return SAPISettings;
        }

        /// <summary>
        /// Rate of speech.  Value is engine-dependent
        /// </summary>
        public TTSValue GetRate()
        {
            return new TTSValue(MinRate, MaxRate, Synthesizer.Rate);
        }

        /// <summary>
        /// Returns a list of voices supported by the speech engine
        /// </summary>
        /// <returns>List of names of vlices</returns>
        public List<String> GetVoices()
        {
            ICollection collection = Synthesizer.GetInstalledVoices();

            var voiceList = new List<String>();
            foreach (InstalledVoice installedVoice in collection)
            {
                voiceList.Add(installedVoice.VoiceInfo.Name);
            }

            return voiceList;
        }

        /// <summary>
        /// Volume of speech. Value is engine-dependent
        /// </summary>
        public TTSValue GetVolume()
        {
            return new TTSValue(MinVolume,
                                MaxVolume,
                                IsMuted() ? _preMuteVolumeLevel : Synthesizer.Volume);
        }

        /// <summary>
        /// Call this first.  Initializes the speech engine
        /// </summary>
        /// <returns>true on success</returns>
        public bool Init(CultureInfo ci)
        {
            var ins = Synthesizer.GetInstalledVoices();

            foreach (InstalledVoice iv in ins)
            {
                Log.Debug("Found installed voice: " + iv.VoiceInfo.Name +
                            "Gender " + iv.VoiceInfo.Gender +
                            ", age: " + iv.VoiceInfo.Age +
                            ", culture: " + iv.VoiceInfo.Culture.Name);
            }

            loadPronunciations(ci);

            setSpeechSynthSettings();

            return true;
        }

        /// <summary>
        /// Returns true if muted
        /// </summary>
        /// <returns>true if muted</returns>
        public bool IsMuted()
        {
            return _muted;
        }

        /// <summary>
        /// Mutes the speech engine (volume set to 0)
        /// </summary>
        public void Mute()
        {
            if (!IsMuted())
            {
                _preMuteVolumeLevel = GetVolume().Value;
                _muted = true;
                Synthesizer.Volume = 0;
                notifyPropertyChanged();
            }
        }

        /// <summary>
        /// Pauses the speech
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        public bool Pause()
        {
            if (Synthesizer.State == SynthesizerState.Speaking)
            {
                Synthesizer.Pause();
            }

            return true;
        }

        /// <summary>
        /// Restores default settings
        /// </summary>
        public void RestoreDefaults()
        {
            var settings = new SAPISettings();

            Synthesizer.Rate = settings.Rate;
            Synthesizer.Volume = settings.Volume;
        }

        /// <summary>
        /// Resumes paused speech
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        public bool Resume()
        {
            Synthesizer.Resume();
            return true;
        }

        /// <summary>
        /// Saves settings
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        public bool Save()
        {
            SAPISettings.Volume = Synthesizer.Volume;
            SAPISettings.Rate = Synthesizer.Rate;

            SAPISettings.Save();

            return true;
        }

        /// <summary>
        /// Sets pitch. SAPI doesn't support it.
        /// </summary>
        /// <param name="pitch">value</param>
        /// <returns>false</returns>
        public bool SetPitch(int pitch)
        {
            return false;
        }

        /// <summary>
        /// Sets the rate of speech. Value is engine dependent
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public bool SetRate(int rate)
        {
            Synthesizer.Rate = rate;

            notifyPropertyChanged();

            return true;
        }

        /// <summary>
        /// Sets the volume level. Value is synth specific
        /// </summary>
        /// <param name="volume">volume level</param>
        /// <returns>true on success</returns>
        public bool SetVolume(int volume)
        {
            if (volume == 0)
            {
                Mute();
            }
            else
            {
                _muted = false;
                Synthesizer.Volume = volume;
                notifyPropertyChanged();
            }

            return true;
        }

        /// <summary>
        /// Shows the preferences dialog
        /// </summary>
        /// <returns>true on success</returns>
        public virtual bool ShowPreferencesDialog()
        {
            var form = new SettingsForm();
            form.ShowDialog();

            return true;
        }

        /// <summary>
        /// Convert the string to speech by sending it to the
        /// speech synthesizer.  Conversion is done asynchronously.
        /// So the function returns immediately.
        /// </summary>
        /// <param name="text">Text to convert.  Can have multiple sentences</param>
        /// <returns>true on success</returns>
        public bool Speak(String text)
        {
            Log.Debug("Entering...text=" + text);

            try
            {
                if (!IsMuted())
                {
                    Synthesizer.SpeakAsync(replaceWithAltPronunciations(text));
                }
            }
            catch (Exception ex)
            {
                Log.Debug("exception caught! ex=" + ex.Message);
            }

            return true;
        }

        /// <summary>
        /// Sends the string to the speech syncth to speak
        /// async. Returns a bookmark. When the bookmark is
        /// reached, an event raised.
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <param name="bookmark">returns bookmark</param>
        /// <returns>true on success</returns>
        public bool SpeakAsync(String text, out int bookmark)
        {
            bool retVal = true;

            bookmark = _nextBookmark++;
            try
            {
                if (!IsMuted())
                {
                    var promptBuilder = new PromptBuilder();
                    promptBuilder.AppendText(replaceWithAltPronunciations(text));
                    promptBuilder.AppendBookmark(bookmark.ToString());

                    Synthesizer.SpeakAsync(promptBuilder);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// Stops the speech. Cancels all async operations in
        /// progress
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        public bool Stop()
        {
            if (Synthesizer.State == SynthesizerState.Speaking)
            {
                Synthesizer.SpeakAsyncCancelAll();
            }

            return true;
        }

        /// <summary>
        /// Unmutes the speech engine. Restore previous
        /// volume
        /// </summary>
        public void UnMute()
        {
            if (IsMuted())
            {
                _muted = false;
                Synthesizer.Volume = _preMuteVolumeLevel;
                notifyPropertyChanged();
            }
        }

        /// <summary>
        /// Waits until speech finishes or timeout expires
        /// </summary>
        /// <param name="timeout">Timeout to wait in milliseconds</param>
        /// <returns>true on success, false on failure</returns>
        public bool WaitUntilDone(int timeout)
        {
            // Synthesizer.WaitUntilDone(timeout);

            return true;
        }

        /// <summary>
        /// Disposer. Release resources and cleanup.
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
                    disposeSpeechSynthesizer();
                }

                // Release unmanaged resources.
            }

            _disposed = true;
        }

        /// <summary>
        /// Disposes the speech synth object
        /// </summary>
        private void disposeSpeechSynthesizer()
        {
            if (_speechSynthesizer != null)
            {
                _speechSynthesizer.BookmarkReached -= speechSynthesizer_BookmarkReached;
                _speechSynthesizer.Dispose();
                _speechSynthesizer = null;
            }
        }

        /// <summary>
        /// Reads alternate pronunciations from the pronunciations file
        /// </summary>
        /// <returns>true on success</returns>
        private bool loadPronunciations(CultureInfo ci)
        {
            if (_pronunciations != null)
            {
                _pronunciations.Dispose();
            }

            _pronunciations = new Pronunciations();

            return _pronunciations.Load(ci, SAPISettings.PronunciationsFile);
        }

        /// <summary>
        /// Notifies subscribes that the bookmark was reached
        /// </summary>
        /// <param name="bookmark">the bookmark</param>
        private void notifyBookmarkReached(int bookmark)
        {
            if (EvtBookmarkReached != null)
            {
                EvtBookmarkReached(this, new TTSBookmarkReachedEventArgs(bookmark));
            }
        }

        /// <summary>
        /// Notifies that property changed
        /// </summary>
        private void notifyPropertyChanged()
        {
            if (EvtPropertyChanged != null)
            {
                EvtPropertyChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Parses the input string and replaces all the words
        /// that have alternate pronunciations
        /// </summary>
        /// <param name="inputString">input string</param>
        /// <returns>string with alternate pronunciations</returns>
        private String replaceWithAltPronunciations(String inputString)
        {
            return SAPISettings.UseAlternatePronunciations ?
                    _pronunciations.ReplaceWithAlternatePronunciations(inputString) :
                    inputString;
        }

        /// <summary>
        /// Sets volume and pitch rate
        /// </summary>
        private void setSpeechSynthSettings()
        {
            try
            {
                SetVolume(SAPISettings.Volume);
                SetRate(SAPISettings.Rate);

                if (!String.IsNullOrEmpty(SAPISettings.Voice))
                {
                    _speechSynthesizer.SelectVoice(SAPISettings.Voice);
                }
                else
                {
                    _speechSynthesizer.SelectVoiceByHints(SAPISettings.Gender, VoiceAge.NotSet, 0,
                        CultureInfo.DefaultThreadCurrentUICulture);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error setting TTS settings " + ex);
            }
            
        }

        /// <summary>
        /// Event handler for bookmark reached. Completion of
        /// an async text to speech
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void speechSynthesizer_BookmarkReached(object sender, BookmarkReachedEventArgs e)
        {
            try
            {
                int bookmark = Convert.ToInt32(e.Bookmark);
                notifyBookmarkReached(bookmark);
            }
            catch (Exception ex)
            {
                Log.Debug("Invalid bookmark " + e.Bookmark + ", exception: " + ex);
            }
        }
    }
}