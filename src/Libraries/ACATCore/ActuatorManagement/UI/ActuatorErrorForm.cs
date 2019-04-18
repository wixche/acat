﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ActuatorErrorForm.cs" company="Intel Corporation">
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ACAT.Lib.Core.ActuatorManagement
{
    /// <summary>
    /// This form displays an actuator error
    /// </summary>
    public partial class ActuatorErrorForm : Form
    {
        /// <summary>
        /// Thickness of the line for the border
        /// </summary>
        private const int BorderThickness = 1;

        /// <summary>
        /// Initializes a new instance of teh class
        /// </summary>
        public ActuatorErrorForm()
        {
            InitializeComponent();

            ShowInTaskbar = false;
            Load += OnLoad;
            BorderPanel.Paint += BorderPanelOnPaint;
        }

        /// <summary>
        /// Get or sets the caption to use in the error form
        /// </summary>
        public String Caption { get; set; }

        /// <summary>
        /// Should the "Configure" button be enabled?
        /// </summary>
        public bool EnableConfigure { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public String Prompt { get; set; }

        /// <summary>
        /// Gets or sets the actuator that initiated the error
        /// </summary>
        public IActuator SourceActuator { get; set; }

        /// <summary>
        /// Dismisses the dialog
        /// </summary>
        public void Dismiss()
        {
            DialogResult = DialogResult.Yes;
        }

        /// <summary>
        /// Updates the form owth the caption and the prompt
        /// </summary>
        /// <param name="caption">caption to set</param>
        /// <param name="prompt">prompt to display</param>
        public void Update(String caption, String prompt)
        {
            Invoke(new MethodInvoker(delegate
            {
                labelPrompt.Text = prompt;
                labelCaption.Text = caption;
            }));
        }

        /// <summary>
        /// Paint handler.  Paint the border around the form
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void BorderPanelOnPaint(object sender, PaintEventArgs e)
        {
            if (BorderPanel.BorderStyle == BorderStyle.FixedSingle)
            {
                const int thickness = BorderThickness;
                const int halfThickness = thickness / 2;
                using (var pen = new Pen(Color.Black, thickness))
                {
                    e.Graphics.DrawRectangle(
                        pen,
                        new Rectangle(halfThickness,
                        halfThickness,
                        BorderPanel.ClientSize.Width - thickness,
                        BorderPanel.ClientSize.Height - thickness));
                }
            }
        }

        /// <summary>
        /// User pressed the Configure" button. Hides the form and
        /// display the configuration dialog
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void buttonConfigure_Click(object sender, EventArgs e)
        {
            Close();

            ActuatorManager.Instance.OnCalibrationAction(SourceActuator);
        }

        /// <summary>
        /// User pressed the OK button. Closes the form.
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handler for when the form loads
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="eventArgs">event args</param>
        private void OnLoad(object sender, EventArgs eventArgs)
        {
            CenterToScreen();

            TopMost = false;
            TopMost = true;
            labelCaption.Text = Caption;
            labelPrompt.Text = Prompt;

            buttonConfigure.Enabled = EnableConfigure;
        }
    }
}