using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VnManager.Extensions
{
    public class TextBoxExtensions
    {
        #region NumericOnly
        /// <summary>
        /// TextBox Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsNumericOnlyProperty = DependencyProperty.RegisterAttached(
            "IsNumericOnly",
            typeof(bool),
            typeof(TextBoxExtensions),
            new UIPropertyMetadata(false, OnIsNumericOnlyChanged));

        /// <summary>
        /// Gets the IsNumericOnly property.  This dependency property indicates the text box only allows numeric or not.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to get the property from</param>
        /// <returns>The value of the StatusBarContent property</returns>
        public static bool GetIsNumericOnly(DependencyObject d)
        {
            return (bool)d.GetValue(IsNumericOnlyProperty);
        }

        /// <summary>
        /// Sets the IsNumericOnly property.  This dependency property indicates the text box only allows numeric or not.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetIsNumericOnly(DependencyObject d, bool value)
        {
            d.SetValue(IsNumericOnlyProperty, value);
        }

        /// <summary>
        /// Handles changes to the IsNumericOnly property.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> that fired the event</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnIsNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool isNumericOnly = (bool)e.NewValue;

            TextBox textBox = (TextBox)d;

            if (isNumericOnly)
            {
                textBox.PreviewTextInput += BlockNonDigitCharacters;
                textBox.PreviewKeyDown += ReviewKeyDown;
            }
            else
            {
                textBox.PreviewTextInput -= BlockNonDigitCharacters;
                textBox.PreviewKeyDown -= ReviewKeyDown;
            }
        }

        /// <summary>
        /// Disallows non-digit character.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="TextCompositionEventArgs"/> that contains the event data.</param>
        private static void BlockNonDigitCharacters(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Disallows a space key.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
        private static void ReviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // Disallow the space key, which doesn't raise a PreviewTextInput event.
                e.Handled = true;
            }
        }
        #endregion
    }
}
