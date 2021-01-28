// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System.Windows;
using System.Windows.Input;

namespace VnManager.Extensions
{
    /// <summary>
    /// XAML extension for reading the mouseover state.
    /// This also gets around the ReadOnly OneWayToSource binding issue
    /// </summary>
    public static class MouseObserver
    {
        public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
            "Observe",
            typeof(bool),
            typeof(MouseObserver),
            new FrameworkPropertyMetadata(OnObserveChanged));

        public static readonly DependencyProperty ObservedMouseOverProperty = DependencyProperty.RegisterAttached(
            "ObservedMouseOver",
            typeof(bool),
            typeof(MouseObserver));


        public static bool GetObserve(DependencyObject frameworkElement)
        {
            if (frameworkElement == null)
            {
                return false;
            }
            return (bool)frameworkElement.GetValue(ObserveProperty);
        }

        public static void SetObserve(DependencyObject frameworkElement, bool observe)
        {
            frameworkElement?.SetValue(ObserveProperty, observe);
        }

        public static bool GetObservedMouseOver(DependencyObject frameworkElement)
        {
            if (frameworkElement == null)
            {
                return false;
            }
            return (bool)frameworkElement.GetValue(ObservedMouseOverProperty);
        }

        public static void SetObservedMouseOver(DependencyObject frameworkElement, bool observedMouseOver)
        {
            frameworkElement?.SetValue(ObservedMouseOverProperty, observedMouseOver);
        }

        private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;
            if ((bool)e.NewValue)
            {
                frameworkElement.MouseEnter += OnFrameworkElementMouseOverChanged;
                frameworkElement.MouseLeave += OnFrameworkElementMouseOverChanged;
                UpdateObservedMouseOverForFrameworkElement(frameworkElement);
            }
            else
            {
                frameworkElement.MouseEnter -= OnFrameworkElementMouseOverChanged;
                frameworkElement.MouseLeave -= OnFrameworkElementMouseOverChanged;
            }
        }

        private static void OnFrameworkElementMouseOverChanged(object sender, MouseEventArgs e)
        {
            UpdateObservedMouseOverForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedMouseOverForFrameworkElement(DependencyObject dependencyObject)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;
            frameworkElement.SetCurrentValue(ObservedMouseOverProperty, frameworkElement.IsMouseOver);
        }
    }
}
