using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiteDB;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public static class BsonValueForeground
    {
        public static PropertyMetadata ValuePropertyMetadata = new PropertyMetadata(
            default(BsonValue),
            OnValueChanged, 
            null);
        
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            name: "Value", 
            propertyType: 
            typeof(BsonValue), 
            ownerType: typeof(BsonValueForeground), 
            defaultMetadata: ValuePropertyMetadata);
        
        public static void SetValue(DependencyObject element, BsonValue value)
        {
            element.SetValue(ValueProperty, value);
        }

        public static BsonValue GetValue(DependencyObject element)
        {
            return (BsonValue) element.GetValue(ValueProperty);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bsonValue = e.NewValue as BsonValue;
            if (d is TextBlock textBlock)
            {
                var ctrlForeground = GetBsonValueForeground(bsonValue);
                textBlock.SetValue(TextBlock.ForegroundProperty, ctrlForeground);
            }
            else if (d is Control ctrl)
            {
                var ctrlForeground = GetBsonValueForeground(bsonValue);
                ctrl.SetValue(Control.ForegroundProperty, ctrlForeground);
            }
            
            ThemeManager.CurrentThemeChanged += (sender, args) =>
            {
                OnValueChanged(d, e);
            };
        }


        public static SolidColorBrush GetBsonValueForeground(BsonValue value)
        {
            try
            {
                if (value != null)
                {
                    if (value.IsDocument || value.IsArray || value.IsBinary)
                    {
                        return ThemeManager.TypeHighlighting.ObjectForeground;
                    }
                
                    if (value.IsInt32 || value.IsInt64 || value.IsDecimal || value.IsDouble)
                    {
                        return ThemeManager.TypeHighlighting.NumberForeground;
                    }

                    if (value.IsBoolean)
                    {
                        return ThemeManager.TypeHighlighting.BooleanForeground;
                    }

                    if (value.IsDateTime)
                    {
                        return ThemeManager.TypeHighlighting.DateTimeForeground;
                    }

                    if (value.IsString)
                    {
                        return ThemeManager.TypeHighlighting.StringForeground;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return ThemeManager.TypeHighlighting.Default;
        }

    }
}