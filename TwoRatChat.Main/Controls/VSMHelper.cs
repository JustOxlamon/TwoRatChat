using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TwoRatChat.Main.Controls {
    public class VSMHelper {
        public static void SetState( UIElement element, string value ) {
            element.SetValue(StateProperty, value);
        }
        public static string GetState( UIElement element ) {
            return (string)element.GetValue(StateProperty);
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
            "State", typeof(string), typeof(VSMHelper), new UIPropertyMetadata(null, StateChanged));

        internal static void StateChanged( DependencyObject target, DependencyPropertyChangedEventArgs args ) {
            if (args.NewValue != null)
                VisualStateManager.GoToElementState((FrameworkElement)target, args.NewValue.ToString(), true);
        }
    }
}
