﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="TextboxWidget" company="Intel Corporation">
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

using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;
using ACAT.Lib.Core.WidgetManagement;
using System.Drawing;
using System.Windows.Forms;

namespace ACAT.Lib.Core.Widgets
{
    /// <summary>
    /// A wrapper widget class a TextBox .NET control.  Scales
    /// the font size of the control depending on the scalefactor.
    /// </summary>
    public class TextBoxWidget : ButtonWidgetBase
    {
        /// <summary>
        /// Has this been disposed?
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Font to use
        /// </summary>
        private Font _font;

        /// <summary>
        /// the Font family
        /// </summary>
        private FontFamily _fontFamily;

        /// <summary>
        /// Font size before the widget is scaled up/down
        /// </summary>
        private float _originalFontSize;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="uiControl">the inner .NET Control for the widget</param>
        public TextBoxWidget(Control uiControl)
            : base(uiControl)
        {
        }

        /// <summary>
        /// Sets the scale factor to scale the widget up/down.
        /// Resizes the font proportionately
        /// </summary>
        /// <param name="newScaleFactor">the scalefactor</param>
        public override void SetScaleFactor(float newScaleFactor)
        {
            base.SetScaleFactor(newScaleFactor);

            _font = new Font(_fontFamily, _originalFontSize * newScaleFactor, _font.Style);
            UIControl.Font = _font;
        }

        /// <summary>
        /// Sets the button attribute.  Extracts font information from
        /// the button attribute and sets the font on the Uicontrol.
        /// </summary>
        /// <param name="attribute"></param>
        public override void SetWidgetAttribute(WidgetAttribute attribute)
        {
            base.SetWidgetAttribute(attribute);
            _fontFamily = Fonts.Instance.GetFontFamily(new[]
                                                        { widgetAttribute.FontName,
                                                            CoreGlobals.AppPreferences.FontName,
                                                            "Arial" });

            if (_fontFamily != null)
            {
                _font = new Font(_fontFamily,
                                    widgetAttribute.FontSize, widgetAttribute
                                    .FontBold ? FontStyle.Bold : FontStyle.Regular);
                UIControl.Font = _font;
            }

            _originalFontSize = _font.Size;
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        /// <param name="disposing">true to dispose managed resources</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    Log.Debug();

                    if (disposing)
                    {
                        // release managed resources
                        unInit();
                    }

                    // Release the native unmanaged resources

                    _disposed = true;
                }
                finally
                {
                    // Call Dispose on your base class.
                    base.Dispose(disposing);
                }
            }
        }

        /// <summary>
        /// Releases resources
        /// </summary>
        private void unInit()
        {
            if (_font != null)
            {
                _font.Dispose();
            }
        }
    }
}