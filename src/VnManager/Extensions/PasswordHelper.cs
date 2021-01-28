// Copyright (c) micah686. All Rights Reserved.
// Licensed under the MIT License.  See the LICENSE file in the project root for license information.

using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace VnManager.Extensions
{
    /// <summary>
    /// XAML extension class that allows for binding a SecureString to a PasswordBox
    /// These methods have the DebuggerHidden attribute. This will need to be disabled in order to step into the method
    /// </summary>
    [DebuggerStepThrough]
    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
            typeof(SecureString), typeof(PasswordHelper),
            new FrameworkPropertyMetadata(null, OnPasswordPropertyChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty =
           DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
           typeof(PasswordHelper));


        [DebuggerHidden]
        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp?.SetValue(AttachProperty, value);
        }

        [DebuggerHidden]
        public static bool GetAttach(DependencyObject dp)
        {
            if (dp == null)
            {
                return false;
            }
            return (bool)dp.GetValue(AttachProperty);
        }

        [DebuggerHidden]
        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp?.GetValue(PasswordProperty);
        }

        [DebuggerHidden]
        public static void SetPassword(DependencyObject dp, string value)
        {
            SecureString val= new NetworkCredential("", value).SecurePassword;
            dp?.SetValue(PasswordProperty, val);
        }

        [DebuggerHidden]
        private static bool GetIsUpdating(DependencyObject dp)
        {
            if (dp == null)
            {
                return false;
            }
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        [DebuggerHidden]
        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        [DebuggerHidden]
        private static void OnPasswordPropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is PasswordBox passwordBox))
            {
                return;
            }
            passwordBox.PasswordChanged -= PasswordChanged;

            if (!GetIsUpdating(passwordBox))
            {
                var secureStr = Marshal.SecureStringToBSTR((SecureString)e.NewValue);
                passwordBox.Password = Marshal.PtrToStringBSTR(secureStr);
            }

            passwordBox.PasswordChanged += PasswordChanged;
        }

        [DebuggerHidden]
        private static void Attach(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            if ((bool)e.OldValue)
            {
                passwordBox.PasswordChanged -= PasswordChanged;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        [DebuggerHidden]
        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            if (passwordBox == null)
            {
                return;
            }
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }
    }
}
