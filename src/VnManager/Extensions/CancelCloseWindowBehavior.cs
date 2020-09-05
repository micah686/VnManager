using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace VnManager.Extensions
{
    public  class CancelCloseWindowBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty CancelCloseProperty =
            DependencyProperty.Register("CancelClose", typeof(bool),
                typeof(CancelCloseWindowBehavior), new FrameworkPropertyMetadata(false));

        public bool CancelClose
        {
            get => (bool)GetValue(CancelCloseProperty);
            set => SetValue(CancelCloseProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.Closing += (sender, args) => args.Cancel = CancelClose;
        }
    }
}
