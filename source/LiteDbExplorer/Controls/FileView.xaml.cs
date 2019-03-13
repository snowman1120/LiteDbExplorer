﻿using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for FileView.xaml
    /// </summary>
    public partial class FileView : UserControl
    {
        public FileView()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty FileSourceProperty = DependencyProperty.Register(
            nameof(FileSource), typeof(object), typeof(FileView), new PropertyMetadata(null, OnFileSourceChanged));

        private static void OnFileSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fileView = d as FileView;
            if (e.NewValue is LiteFileInfo liteFileInfo)
            {
                fileView?.LoadFile(liteFileInfo);
            }
            else
            {
                fileView?.Reset();
            }
        }

        public object FileSource
        {
            get => (object) GetValue(FileSourceProperty);
            set => SetValue(FileSourceProperty, value);
        }

        public void LoadFile(LiteFileInfo file)
        {
            var textRegex = new Regex("text|json|script|xml");
            if (file.MimeType.StartsWith("image"))
            {
                using (var fStream = file.OpenRead())
                {
                    var stream = new MemoryStream();
                    fStream.CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    ImageImage.Source = bitmap;
                    ImageImage.Visibility = Visibility.Visible;
                    TextText.Visibility = Visibility.Collapsed;
                }
            }
            else if (textRegex.IsMatch(file.MimeType))
            {
                using (var fileStream = file.OpenRead())
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        var myStr = reader.ReadToEnd();

                        TextText.Text = myStr;
                        TextText.Visibility = Visibility.Visible;
                        ImageImage.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                Reset();
            }
        }

        public void Reset()
        {
            TextText.Text = string.Empty;
            ImageImage.Source = null;
            TextText.Visibility = Visibility.Collapsed;
            ImageImage.Visibility = Visibility.Collapsed;
        }

    }
}
