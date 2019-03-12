using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public class FocusGroupState
    {
        public static readonly DependencyProperty IsFocusActiveProperty = DependencyProperty.RegisterAttached(
            "IsFocusActive",
            typeof(bool),
            typeof(FocusGroupState),
            new UIPropertyMetadata(IsFocusActiveChanged)
        );
        
        public static void SetIsFocusActive(UIElement element, Boolean value)
        {
            element.SetValue(IsFocusActiveProperty, value);
        }

        public static bool GetIsFocusActive(UIElement element)
        {
            return (bool)element.GetValue(IsFocusActiveProperty);
        }

        private static void IsFocusActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }
    }

    public class FocusActiveGroup : Behavior<UIElement>
    {
        public static readonly List<UIElement> UIElements = new List<UIElement>();

        protected override void OnAttached()
        {
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseRightButtonDown += OnMouseRightButtonDown;
            UIElements.Add(AssociatedObject);

            if (AssociatedObject.IsFocused)
            {
                SetIsFocusActiveTrue(AssociatedObject);
            }
            else
            {
                SetIsFocusActiveFalse(AssociatedObject);
            }
        }
        
        protected override void OnDetaching()
        {
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseRightButtonDown -= OnMouseRightButtonDown;
            SetIsFocusActiveFalse(AssociatedObject);
            UIElements.Remove(AssociatedObject);
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            SetIsFocusActiveTrue(AssociatedObject);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            SetIsFocusActiveFalse(AssociatedObject);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetIsFocusActiveTrue(AssociatedObject);
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetIsFocusActiveTrue(AssociatedObject);
        }

        private void SetIsFocusActiveTrue(UIElement element)
        {
            foreach (var otherUiElement in UIElements.Where(p => p != element))
            {
                FocusGroupState.SetIsFocusActive(otherUiElement, false);
            }
            FocusGroupState.SetIsFocusActive(element, true);
        }

        private void SetIsFocusActiveFalse(UIElement element)
        {
            FocusGroupState.SetIsFocusActive(element, false);
        }
    }
}