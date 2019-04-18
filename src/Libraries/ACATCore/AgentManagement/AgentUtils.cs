﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="AgentUtils.cs" company="Intel Corporation">
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
using System.Windows.Automation;
using System.Windows.Forms;

namespace ACAT.Lib.Core.AgentManagement
{
    /// <summary>
    /// Contains useful utility functions
    /// </summary>
    public class AgentUtils
    {
        /// <summary>
        /// Finds a descendent of the focused element that has the specified
        /// className, controlType and automationID
        /// </summary>
        /// <param name="focusedElement"></param>
        /// <param name="className">class name </param>
        /// <param name="controlType">controlType</param>
        /// <param name="automationId">automation id </param>
        /// <param name="name">optional, name of the element to search for</param>
        /// <returns>automation element if found null otherwise</returns>
        public static AutomationElement FindElementByAutomationId(AutomationElement focusedElement,
                                                                    String className,
                                                                    object controlType,
                                                                    String automationId,
                                                                    String name = "")
        {
            var controlTypeProperty = new PropertyCondition(AutomationElement.ControlTypeProperty, controlType);
            var automationIdProperty = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
            var classNameProperty = new PropertyCondition(AutomationElement.ClassNameProperty, className);
            var findControl = new AndCondition(controlTypeProperty, automationIdProperty, classNameProperty);

            var retVal = focusedElement.FindFirst(TreeScope.Descendants, findControl);

            if (retVal != null && !String.IsNullOrEmpty(name))
            {
                retVal = ((String.Compare(retVal.Current.Name, name, true) == 0) ? retVal : null);
            }

            return retVal;
        }

        /// <summary>
        /// Checks if the control identified by className, controlType and
        /// automationID is the focused element or an ancestor of the focusedElement
        /// </summary>
        /// <param name="focusedElement">control that has focus</param>
        /// <param name="className">class name of the ancestor</param>
        /// <param name="controlType">controlType of the ancestor</param>
        /// <param name="automationId">automation id of the ancestor</param>
        /// <returns>true if it is</returns>
        public static AutomationElement GetElementOrAncestorByAutomationId(AutomationElement focusedElement,
                                                                            String className,
                                                                            String controlType,
                                                                            String automationId)
        {
            var walker = TreeWalker.ControlViewWalker;

            if (String.Compare(focusedElement.Current.ClassName, className, true) == 0 &&
                String.Compare(focusedElement.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                String.Compare(focusedElement.Current.AutomationId, automationId, true) == 0)
            {
                return focusedElement;
            }

            var parent = walker.GetParent(focusedElement);

            while (parent != null)
            {
                Log.Debug("parent.ClassName: " + parent.Current.ClassName + ", parent.ControlType: " +
                            parent.Current.ControlType.ProgrammaticName + ", parent.AutoId: " + parent.Current.AutomationId);

                if (String.Compare(parent.Current.ClassName, className, true) == 0 &&
                    String.Compare(parent.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                    String.Compare(parent.Current.AutomationId, automationId, true) == 0)
                {
                    return parent;
                }

                parent = walker.GetParent(parent);
            }

            return null;
        }

        /// <summary>
        /// Returns the automation element of the control that is a sibling
        /// of the focusedElement.  The control isidentified by
        /// className, controlType and  automationIid
        /// </summary>
        /// <param name="focusedElement">control in focus</param>
        /// <param name="className">class name of sibling</param>
        /// <param name="controlType">control type of sibling </param>
        /// <param name="automationId">automation id of sibling</param>
        /// <returns>automation element if found, null otherwise</returns>
        public static AutomationElement GetSiblingByAutomationId(AutomationElement focusedElement,
                                                                String className,
                                                                String controlType,
                                                                String automationId)
        {
            var walker = TreeWalker.ControlViewWalker;

            var parent = walker.GetParent(focusedElement);
            if (parent == null)
            {
                return null;
            }

            var child = walker.GetFirstChild(parent);
            while (child != null)
            {
                if (String.Compare(child.Current.ClassName, className, true) == 0 &&
                    String.Compare(child.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                    String.Compare(child.Current.AutomationId, automationId, true) == 0)
                {
                    return child;
                }

                child = walker.GetNextSibling(child);
            }

            return null;
        }

        /// <summary>
        /// Inserts the specified string value into the control
        /// referenced by the element
        /// </summary>
        /// <param name="element">the element into which to insert text</param>
        /// <param name="value">text to insert</param>
        public static void InsertTextIntoElement(AutomationElement element, string value)
        {
            if (element == null || value == null)
            {
                return;
            }

            try
            {
                if (!element.Current.IsEnabled || !element.Current.IsKeyboardFocusable)
                {
                    Log.Debug("Control not enabled or keyboard focusable. AutomationID " + element.Current.AutomationId);
                    return;
                }

                object valuePattern;
                if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                {
                    element.SetFocus();
                    SendKeys.SendWait(value);
                }
                else
                {
                    element.SetFocus();
                    ((ValuePattern)valuePattern).SetValue(value);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
        }

        /// <summary>
        /// Checks if the control identified by className and controlType is
        /// an ancestor of the focusedElement
        /// </summary>
        /// <param name="focusedElement">control that has focus</param>
        /// <param name="className">class name of the ancestor</param>
        /// <param name="controlType">controlType of the ancestor</param>
        /// <returns>true if it is</returns>
        public static bool IsAncestor(AutomationElement focusedElement, String className, String controlType)
        {
            var walker = TreeWalker.ControlViewWalker;

            var parent = walker.GetParent(focusedElement);
            while (parent != null)
            {
                if (String.Compare(parent.Current.ClassName, className, true) == 0 &&
                    String.Compare(parent.Current.ControlType.ProgrammaticName, controlType, true) == 0)
                {
                    return true;
                }

                parent = walker.GetParent(parent);
            }

            return false;
        }

        /// <summary>
        /// Checks if the control identified by className, controlType and
        /// automationID is an ancestor of the focusedElement
        /// </summary>
        /// <param name="focusedElement">control that has focus</param>
        /// <param name="className">class name of the ancestor</param>
        /// <param name="controlType">controlType of the ancestor</param>
        /// <param name="automationId">automation id of the ancestor</param>
        /// <returns>true if it is</returns>
        public static bool IsAncestorByAutomationId(AutomationElement focusedElement, String className, String controlType, String automationId)
        {
            var walker = TreeWalker.ControlViewWalker;

            var parent = walker.GetParent(focusedElement);
            while (parent != null)
            {
                if (String.Compare(parent.Current.ClassName, className, true) == 0 &&
                    String.Compare(parent.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                    String.Compare(parent.Current.AutomationId, automationId, true) == 0)
                {
                    return true;
                }

                parent = walker.GetParent(parent);
            }

            return false;
        }

        /// <summary>
        /// Checks if the control identified by className, controlType and
        /// name is an ancestor of the focusedElement
        /// </summary>
        /// <param name="focusedElement">control that has focus</param>
        /// <param name="className">class name of the ancestor</param>
        /// <param name="controlType">controlType of the ancestor</param>
        /// <param name="name">name of the ancestor</param>
        /// <returns>true if it is</returns>
        public static bool IsAncestorByName(AutomationElement focusedElement, String className, String controlType, String name)
        {
            var walker = TreeWalker.ControlViewWalker;

            var parent = walker.GetParent(focusedElement);
            while (parent != null)
            {
                if (String.Compare(parent.Current.ClassName, className, true) == 0 &&
                    String.Compare(parent.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                    String.Compare(parent.Current.Name, name, true) == 0)
                {
                    return true;
                }

                parent = walker.GetParent(parent);
            }

            return false;
        }

        /// <summary>
        /// Checks if the focusedElement has the specified className, controlType
        /// and automationID
        /// </summary>
        /// <param name="focusedElement">control that has focus</param>
        /// <param name="className">class name of the ancestor</param>
        /// <param name="controlType">controlType of the ancestor</param>
        /// <param name="automationId">automation id of the ancestor</param>
        /// <param name="name">(optional) name of the element</param>
        /// <returns>true if it is</returns>
        public static bool IsElementByAutomationId(AutomationElement focusedElement,
                                                            String className,
                                                            String controlType,
                                                            String automationId,
                                                            String name = "")
        {
            bool retVal = (String.Compare(focusedElement.Current.ClassName, className, true) == 0 &&
                    String.Compare(focusedElement.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                    String.Compare(focusedElement.Current.AutomationId, automationId, true) == 0);

            if (!String.IsNullOrEmpty(name))
            {
                retVal = (String.Compare(focusedElement.Current.Name, name, true) == 0);
            }

            return retVal;
        }

        /// <summary>
        /// Checks if the focused element has the specified className and controlType
        /// or if one of its ancestors has the specified className and controlType
        /// </summary>
        /// <param name="focusedElement">element that has focus</param>
        /// <param name="className">class name of the element or ancestor</param>
        /// <param name="controlType">controlType of the element or ancestor</param>
        /// <returns>true if it is</returns>
        public static bool IsElementOrAncestor(AutomationElement focusedElement, String className, String controlType)
        {
            var walker = TreeWalker.ControlViewWalker;

            if (String.Compare(focusedElement.Current.ClassName, className, true) == 0 &&
                String.Compare(focusedElement.Current.ControlType.ProgrammaticName, controlType, true) == 0)
            {
                return true;
            }

            var parent = walker.GetParent(focusedElement);
            while (parent != null)
            {
                if (String.Compare(parent.Current.ClassName, className, true) == 0 &&
                    String.Compare(parent.Current.ControlType.ProgrammaticName, controlType, true) == 0)
                {
                    return true;
                }

                parent = walker.GetParent(parent);
            }

            return false;
        }

        /// <summary>
        /// Checks if the focusedElement or one of its ancestors has
        /// the specified className, controlType and
        /// automationID
        /// </summary>
        /// <param name="focusedElement">control that has focus</param>
        /// <param name="className">class name of the ancestor</param>
        /// <param name="controlType">controlType of the ancestor</param>
        /// <param name="automationId">automation id of the ancestor</param>
        /// <returns>true if it is</returns>
        public static bool IsElementOrAncestorByAutomationId(AutomationElement focusedElement,
                                                            String className,
                                                            String controlType,
                                                            String automationId)
        {
            var walker = TreeWalker.ControlViewWalker;

            if (String.Compare(focusedElement.Current.ClassName, className, true) == 0 &&
                String.Compare(focusedElement.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                String.Compare(focusedElement.Current.AutomationId, automationId, true) == 0)
            {
                return true;
            }

            var parent = walker.GetParent(focusedElement);

            while (parent != null)
            {
                Log.Debug("parent.ClassName: " + parent.Current.ClassName + ", parent.COntrolType: " +
                    parent.Current.ControlType.ProgrammaticName + ", parent.AutoId: " + parent.Current.AutomationId);

                if (String.Compare(parent.Current.ClassName, className, true) == 0 &&
                    String.Compare(parent.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                    String.Compare(parent.Current.AutomationId, automationId, true) == 0)
                {
                    return true;
                }

                parent = walker.GetParent(parent);
            }

            return false;
        }

        /// <summary>
        /// Checks if the focused element has the specified className, controlType and name
        /// or if one of its ancestors has the specified className, controlType
        /// and name
        /// </summary>
        /// <param name="focusedElement">control that has focus</param>
        /// <param name="className">class name of the ancestor</param>
        /// <param name="controlType">controlType of the ancestor</param>
        /// <param name="name">name of the ancestor</param>
        /// <returns>true if it is</returns>
        public static bool IsElementOrAncestorByName(AutomationElement focusedElement, String className, String controlType, String name)
        {
            var walker = TreeWalker.ControlViewWalker;

            if (String.Compare(focusedElement.Current.ClassName, className, true) == 0 &&
                String.Compare(focusedElement.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                String.Compare(focusedElement.Current.Name, name, true) == 0)
            {
                return true;
            }

            var parent = walker.GetParent(focusedElement);
            while (parent != null)
            {
                if (String.Compare(parent.Current.ClassName, className, true) == 0 &&
                    String.Compare(parent.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                    String.Compare(parent.Current.Name, name, true) == 0)
                {
                    return true;
                }

                parent = walker.GetParent(parent);
            }

            return false;
        }

        /// <summary>
        /// Checks if the control identified by className, controlType and
        /// automationIid is a sibling of the focusedElement.
        /// </summary>
        /// <param name="focusedElement">control in focus</param>
        /// <param name="className">class name of sibling</param>
        /// <param name="controlType">control type of sibling </param>
        /// <param name="automationId">automation id of sibling</param>
        /// <returns>true if it is</returns>
        public static bool IsSiblingByAutomationId(AutomationElement focusedElement, String className, String controlType, String automationId)
        {
            var walker = TreeWalker.ControlViewWalker;

            var parent = walker.GetParent(focusedElement);
            if (parent != null)
            {
                var child = walker.GetFirstChild(parent);
                while (child != null)
                {
                    if (String.Compare(child.Current.ClassName, className, true) == 0 &&
                            String.Compare(child.Current.ControlType.ProgrammaticName, controlType, true) == 0 &&
                            String.Compare(child.Current.AutomationId, automationId, true) == 0)
                    {
                        return true;
                    }

                    child = walker.GetNextSibling(child);
                }
            }
            return false;
        }
    }
}