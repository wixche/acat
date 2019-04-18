﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ActuatorEx.cs" company="Intel Corporation">
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
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace ACAT.Lib.Core.ActuatorManagement
{
    /// <summary>
    /// This is a wrapper class for the Actuator.  It has helper functions
    /// to handler user interaction for calibration.  It displays the calibration
    /// form while the actuator is calibrating. This class can be extended to add
    /// addtional functions.
    /// Since actuator initialization and calibration are asynchronous, this class
    /// handles them synchronously, so to the caller, the functions appear to act
    /// in a synchronous manner.
    /// </summary>
    internal class ActuatorEx
    {
        /// <summary>
        /// The inner acutator object
        /// </summary>
        public IActuator SourceActuator;

        /// <summary>
        /// Event is set when the background task is done
        /// </summary>
        private readonly ManualResetEvent _bgTaskDoneEvent = new ManualResetEvent(true);

        /// <summary>
        /// This event is set when the calibration has concluded
        /// </summary>
        private readonly ManualResetEvent _calibrationDoneEvent = new ManualResetEvent(true);

        /// <summary>
        /// Has the calibartion form been created
        /// </summary>
        private readonly ManualResetEvent _formCreatedEvent = new ManualResetEvent(false);

        /// <summary>
        /// Background worker
        /// </summary>
        private Worker _bgWorker;

        /// <summary>
        /// The calibration form object
        /// </summary>
        private CalibrationForm calibrationForm;

        /// <summary>
        /// Has initialization completed?
        /// </summary>
        private ManualResetEvent initDoneEvent = new ManualResetEvent(false);

        /// <summary>
        /// Initializes an instance of the class
        /// </summary>
        /// <param name="src">The inner actuator object</param>
        public ActuatorEx(IActuator src)
        {
            SourceActuator = src;
        }

        /// <summary>
        /// Gets/sets whether an error ocurred during initialization
        /// </summary>
        public bool InitError { get; set; }

        /// <summary>
        /// Closes calibration form
        /// </summary>
        public void CancelCalibration()
        {
            closeCalibrationForm();

            _calibrationDoneEvent.Set();
        }

        /// <summary>
        /// Initializes the actuator and waits for init
        /// to complete
        /// </summary>
        public void Init()
        {
            SourceActuator.Init();

            Log.Debug("Before Wait");

            WaitForInit();

            Log.Debug("After Wait");
        }

        /// <summary>
        /// Call this function to indicate end of calibration.  If error message
        /// is not empty, assumes there was an error and displays
        /// the error dialog
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <param name="enableConfigure">should the config button be enabled</param>
        public void OnEndCalibration(String errorMessage = "", bool enableConfigure = true)
        {
            Log.Debug();

            if (!String.IsNullOrEmpty((errorMessage)))
            {
                hideCalibrationForm();

                OnError(errorMessage, enableConfigure);
            }

            Log.Debug("Calling closeCalibrationForm()");

            closeCalibrationForm();
            _calibrationDoneEvent.Set();
        }

        /// <summary>
        /// Invoked to indicate there was an error.  Displays the
        /// error form as a dialog
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="enableConfigure">should the config button be enabled</param>
        public void OnError(String message, bool enableConfigure = true)
        {
            var errorForm = new ActuatorErrorForm
            {
                EnableConfigure = enableConfigure,
                Caption = "ERROR",
                SourceActuator = this.SourceActuator,
                Prompt = message
            };
            errorForm.ShowDialog();
        }

        /// <summary>
        /// Invoked when initaliation is complete.  Sets the event
        /// </summary>
        /// <param name="success">was init successful?</param>
        public void OnInitDone(bool success = true)
        {
            InitError = !success;
            initDoneEvent.Set();
        }

        /// <summary>
        /// Invoked to request start of calibration
        /// </summary>
        public void RequestCalibration()
        {
            _calibrationDoneEvent.Reset();
        }

        /// <summary>
        /// Starts calibration
        /// </summary>
        public void StartCalibration()
        {
            SourceActuator.StartCalibration();
        }

        /// <summary>
        /// Displays the calibartion form if it is not already
        /// displayed. Updates the caption, prompt on the form
        /// </summary>
        /// <param name="position">where to display the form?</param>
        /// <param name="caption">caption of the form</param>
        /// <param name="prompt">any message to display?</param>
        /// <param name="timeout">calibration timeout</param>
        /// <param name="enableConfigure">should calibration button be enabled?</param>
        public void UpdateCalibrationStatus(Windows.WindowPosition position, String caption, String prompt, int timeout, bool enableConfigure, String buttonText)
        {
            if (calibrationForm == null)
            {
                showCalibrationForm(position, caption, prompt, timeout, enableConfigure, buttonText);
            }
            else if (calibrationForm.Visible)
            {
                calibrationForm.Update(caption, prompt);
            }
        }

        /// <summary>
        /// Waits for async calibration to end.
        /// </summary>
        public void WaitForCalibration()
        {
            Log.Debug("Waiting for calibration");

            _calibrationDoneEvent.WaitOne();

            Log.Debug("Calibration event is done");

            Log.Debug("Waiting for calibration configure dialog");

            _bgTaskDoneEvent.WaitOne();

            Log.Debug("Calibration configure dialog event is done");
        }

        /// <summary>
        /// Waits for async init to end
        /// </summary>
        public void WaitForInit()
        {
            Log.Debug("Waiting for initdone");
            initDoneEvent.WaitOne();
            Log.Debug("initdone is done");
        }

        /// <summary>
        /// The work handler function for the background worker.  Displays
        /// the calibration form as a dialog
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as Worker;
            calibrationForm = new CalibrationForm
            {
                Caption = worker.Caption,
                Prompt = worker.Prompt,
                SourceActuator = SourceActuator,
                Timeout = worker.Timeout,
                EnableConfigure = worker.EnableConfigure,
                ButtonText = worker.ButtonText
            };

            Windows.SetWindowPosition(calibrationForm, worker.Position);
            _formCreatedEvent.Set();
            if (calibrationForm != null)
            {
                Log.Debug("Calling calibrationform.ShowDialog");
                DialogResult result = calibrationForm.ShowDialog();
                Log.Debug("Result: " + result);

                // user closed the calibration form
                if (result == DialogResult.Cancel)
                {
                    SourceActuator.OnCalibrationCanceled();
                    _calibrationDoneEvent.Set();
                }
            }
            else
            {
                Log.Debug("calibration form IS NULL!!!");
            }

            if (calibrationForm != null)
            {
                calibrationForm.Dispose();
                calibrationForm = null;
            }
        }

        /// <summary>
        /// Background worker completed
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void bgWorker_RunCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log.Debug();
            _bgTaskDoneEvent.Set();
        }

        /// <summary>
        /// Closes the calibration form
        /// </summary>
        private void closeCalibrationForm()
        {
            try
            {
                if (calibrationForm != null)
                {
                    Log.Debug("Closing calibration form, calling Dismiss()");
                    calibrationForm.Dismiss();
                    calibrationForm = null;
                }

                if (_bgWorker != null)
                {
                    _bgWorker.CancelAsync();
                    _bgWorker.Dispose();
                    _bgWorker = null;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Hides the calibration form
        /// </summary>
        private void hideCalibrationForm()
        {
            if (calibrationForm != null)
            {
                calibrationForm.Hide();
            }
        }

        /// <summary>
        /// Kicks off the background worker to display the calibration form
        /// </summary>
        /// <param name="position">where to display the form?</param>
        /// <param name="caption">caption of the form</param>
        /// <param name="prompt">any message to display?</param>
        /// <param name="timeout">calibration timeout</param>
        /// <param name="enableConfigure">should calibration button be enabled?</param>
        private void showCalibrationForm(Windows.WindowPosition position, String caption, String prompt, int timeout, bool enableConfigure, String buttonText)
        {
            _bgTaskDoneEvent.Reset();
            _bgWorker = new Worker
            {
                Prompt = prompt,
                Timeout = timeout,
                Caption = caption,
                EnableConfigure = enableConfigure,
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = false,
                Position = position,
                ButtonText = buttonText
            };

            _bgWorker.RunWorkerCompleted += bgWorker_RunCompleted;
            _bgWorker.DoWork += bgWorker_DoWork;
            _formCreatedEvent.Reset();
            _bgWorker.RunWorkerAsync();

            _formCreatedEvent.WaitOne();
        }

        /// <summary>
        /// Extends the background worker class to store
        /// properties for the calibration form which is displayed
        /// by the background worker
        /// </summary>
        private class Worker : BackgroundWorker
        {
            public String ButtonText { get; set; }

            public String Caption { get; set; }

            public bool EnableConfigure { get; set; }

            public Windows.WindowPosition Position { get; set; }

            public String Prompt { get; set; }

            public int Timeout { get; set; }
        }
    }
}