﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="IWordPredictor.cs" company="Intel Corporation">
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
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ACAT.Lib.Core.WordPredictionManagement
{
    /// <summary>
    /// Interface to Word prediction engines.  A word predictor predicts
    /// the next word using the previous n-words as context.  Some word predictors
    /// also support learning where the engine tunes its internal model for
    /// more accurate predictions based on the user.
    /// All WordPredictors should implement this interface.
    /// </summary>
    public interface IWordPredictor : IDisposable
    {
        /// <summary>
        /// Returns a descriptor which contains a user readable name, a
        /// short textual description and a unique GUID.
        /// </summary>
        IDescriptor Descriptor { get; }

        /// <summary>
        /// Whether puncutations should be a part of the prediction. For
        /// instance, some word predictors may return a question mark as a
        /// prediction if it detects the end of a sentence.
        /// </summary>
        bool FilterPunctuationsEnable { get; set; }

        /// <summary>
        /// How many previous words to use for prediction.
        /// </summary>
        int NGram { get; set; }

        /// <summary>
        /// Sets the number of predicted words to return.
        /// </summary>
        int PredictionWordCount { get; set; }

        /// <summary>
        /// Returns a flag indicating whether the word predictor supports
        /// dynamic learning.
        /// </summary>
        bool SupportsLearning { get; }

        /// <summary>
        /// Initialize the word predictor.  Language is optional,
        /// if not specified, use the current culture
        /// </summary>
        /// <param name="ci">Language for the model</param>
        /// <returns>true on success, false on failure</returns>
        bool Init(CultureInfo ci);

        /// <summary>
        /// Add the text to the word prediction models for better
        /// prediction.  The text would typically represent something
        /// the user typed.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool Learn(String text);

        /// <summary>
        /// Call this on a context switch to a document.  Creates
        /// a temporary model
        /// </summary>
        /// <param name="text">Text from the document</param>
        /// <returns>Handle.  Pass this to unload</returns>
        int LoadContext(String text);

        /// <summary>
        /// Reset to factory default settings
        /// </summary>
        /// <returns>true on success, false on failure</returns>
        bool LoadDefaultSettings();

        /// <summary>
        /// Load settings from a file maintained by the word predictor.
        /// </summary>
        /// <param name="configFileDirectory">Directory where the settings are stored</param>
        /// <returns>true on success, false on failure</returns>
        bool LoadSettings(String configFileDirectory);

        /// <summary>
        /// Returns a list of next word predictions based on the context
        /// from the previous words in the sentence.  The number of words
        /// returned is set by the PredictionWordCount property
        /// </summary>
        /// <param name="prevNWords">Previous words in the sentence</param>
        /// <param name="lastWord">current word (may be partially spelt out</param>
        /// <returns>A list of predicted words</returns>
        IEnumerable<String> Predict(String prevNWords, String lastWord);

        /// <summary>
        /// Save the word predictor settings to a file that is maintained
        /// by the word predictor.
        /// </summary>
        /// <param name="configFileDirectory">Directory where the settings are stored</param>
        /// <returns>true on success, false on failure</returns>
        bool SaveSettings(String configFileDirectory);

        /// <summary>
        /// Unload context
        /// </summary>
        /// <param name="contextHandle">Handle returned by load</param>
        void UnloadContext(int contextHandle);
    }
}