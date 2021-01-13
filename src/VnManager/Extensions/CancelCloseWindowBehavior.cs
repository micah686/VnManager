using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace VnManager.Extensions
{
    /// <summary>
    /// Class for modifying the behavior of the X close button
    /// </summary>
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
