﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="NullTTSEngine.cs" company="Intel Corporation">
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
using ACAT.Lib.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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

namespace ACAT.Lib.Core.TTSManagement
{
    /// <summary>
    /// Represents a 'no-op' text-to-speech engine.  Has no TTS
    /// functionality. Useful if there is no active TTS engine.
    /// </summary>
    [DescriptorAttribute("A98DA439-A6A9-48EF-AC8D-3D3588363341",
                        "Null Text-to-speech Engine",
                        "Text-to-speech disabled")]
    public class NullTTSEngine : ExtensionInvoker, ITTSEngine
    {
        private static int _nextBookmark = 1;

        private readonly StatusFlags _currentStatus;

        /// <summary>
        /// Pitch of the voice
        /// </summary>
        private readonly TTSValue _speechPitch = new TTSValue(0, 0, 0);

        /// <summary>
        /// Speech rate
        /// </summary>
        private readonly TTSValue _speechRate = new TTSValue(0, 0, 0);

        /// <summary>
        /// Speech volume
        /// </summary>
        private readonly TTSValue _speechVolume = new TTSValue(0, 0, 0);

        /// <summary>
        /// Has this object been disposed
        /// </summary>
        private bool _disposed;

        private bool _muted;

        /// <summary>
        /// Constructor
        /// </summary>
        public NullTTSEngine()
        {
            _currentStatus = StatusFlags.None;
            EvtVoiceChanged = null;
            EvtStatusChanged = null;
        }

#pragma warning disable

        /// <summary>
        /// Triggered when bookmark reached after async text to speech
        /// </summary>
        public event TTSBookmarkReached EvtBookmarkReached;

        /// <summary>
        /// Raised whenever one of the speech properties changes
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
        /// Gets the ACAT descriptor for this engine
        /// </summary>
        public IDescriptor Descriptor
        {
            get { return DescriptorAttribute.GetDescriptor(GetType()); }
        }

        /// <summary>
        /// Returns the current status of the speech engine
        /// </summary>
        public StatusFlags Status
        {
            get { return _currentStatus; }
        }

        /// <summary>
        /// Gets or sets the voice to use
        /// </summary>
        public String Voice
        {
            get { return String.Empty; }

            set { }
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
        /// Pitch of speech.  Value is engine-dependent
        /// </summary>
        public TTSValue GetPitch()
        {
            return _speechPitch;
        }

        /// <summary>
        /// Rate of speech.  Value is engine-dependent
        /// </summary>
        public TTSValue GetRate()
        {
            return _speechRate;
        }

        /// <summary>
        /// Returns a list of voices supported by the speech engine
        /// </summary>
        /// <returns>List of names of vlices</returns>
        public List<String> GetVoices()
        {
            return new List<String>();
        }

        /// <summary>
        /// Volume of speech. Value is engine-dependent
        /// </summary>
        public TTSValue GetVolume()
        {
            return _speechVolume;
        }

        /// <summary>
        /// Call this first.  Initializes the speech engine
        /// </summary>
        /// <returns></returns>
        public bool Init(CultureInfo ci)
        {
            return true;
        }

        /// <summary>
        /// Is engine muted
        /// </summary>
        /// <returns>mute state</returns>
        public bool IsMuted()
        {
            return _muted;
        }

        /// <summary>
        /// Mute the engine
        /// </summary>
        public void Mute()
        {
            _muted = true;
        }

        /// <summary>
        /// Pauses the speech
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        public bool Pause()
        {
            return true;
        }

        /// <summary>
        /// Restores default settings
        /// </summary>
        public void RestoreDefaults()
        {
            SetVolume(0);
            SetPitch(0);
            SetRate(0);
        }

        /// <summary>
        /// Resumes paused speech
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        public bool Resume()
        {
            return true;
        }

        /// <summary>
        /// Save settings
        /// </summary>
        /// <returns>true</returns>
        public bool Save()
        {
            return true;
        }

        /// <summary>
        /// Sets pitch value
        /// </summary>
        /// <param name="pitch">pitch value</param>
        /// <returns>true</returns>
        public bool SetPitch(int pitch)
        {
            _speechPitch.Value = pitch;

            notifyPropertyChanged();

            return true;
        }

        /// <summary>
        /// Sets rate of speech
        /// </summary>
        /// <param name="rate">value</param>
        /// <returns>true</returns>
        public bool SetRate(int rate)
        {
            _speechRate.Value = rate;

            notifyPropertyChanged();

            return true;
        }

        /// <summary>
        /// Sets volume level
        /// </summary>
        /// <param name="volume">level</param>
        /// <returns>true</returns>
        public bool SetVolume(int volume)
        {
            _speechVolume.Value = volume;

            notifyPropertyChanged();

            return true;
        }

        /// <summary>
        /// Convert the string to speech.ag is specified)
        /// </summary>
        /// <param name="text">Text to convert.  Can have multiple sentences</param>
        /// <returns>true</returns>
        public bool Speak(String text)
        {
            return true;
        }

        /// <summary>
        /// Perform TTS asynchronously
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="bookmark">bookmark</param>
        /// <returns>true</returns>
        public bool SpeakAsync(String text, out int bookmark)
        {
            bookmark = _nextBookmark++;
            return true;
        }

        /// <summary>
        /// Stops the speech. All items are
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        public bool Stop()
        {
            return true;
        }

        /// <summary>
        /// Unmute the engine
        /// </summary>
        public void UnMute()
        {
            _muted = false;
        }

        /// <summary>
        /// Waits until speech finishes or timeout expires
        /// </summary>
        /// <param name="timeout">Timeout to wait in milliseconds</param>
        /// <returns>true on success, false on failure</returns>
        public bool WaitUntilDone(int timeout)
        {
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
                }

                // Release unmanaged resources.
            }

            _disposed = true;
        }

        /// <summary>
        /// Notifies subscribers that some property changed
        /// in the engine
        /// </summary>
        private void notifyPropertyChanged()
        {
            if (EvtPropertyChanged != null)
            {
                EvtPropertyChanged(this, new EventArgs());
            }
        }
    }
}