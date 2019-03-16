using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Framework.Windows;
using LiteDbExplorer.Modules;
using LiteDbExplorer.Windows;

namespace LiteDbExplorer
{
    public class MainWindowViewInteractionResolver : IViewInteractionResolver
    {
        public Window Owner { get; set; }

        public bool OpenDatabaseProperties(DatabaseReference database)
        {
            var window = new DatabasePropertiesWindow(database.LiteDatabase)
            {
                Owner = Owner
            };

            return window.ShowDialog() == true;
        }

        public bool OpenEditDocument(DocumentReference document)
        {
            var windowController = new WindowController {Title = "Document Editor"};
            var control = new DocumentEntryControl(document, windowController);
            var window = new DialogWindow(control, windowController)
            {
                Owner = Owner,
                Height = 600
            };

            return window.ShowDialog() == true;
        }

        public bool RevealInExplorer(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }

            //Clean up file path so it can be navigated OK
            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            return true;
        }

        public void ActivateDocument(DocumentReference document)
        {
            Store.Current.SelectCollection(document.Collection);
            Store.Current.SelectDocument(document);
        }

        public void ActivateCollection(CollectionReference collection, IEnumerable<DocumentReference> selectedDocuments = null)
        {
            Store.Current.SelectCollection(collection);
            Store.Current.SelectDocument(selectedDocuments?.FirstOrDefault());
            Store.Current.SelectedDocuments = selectedDocuments.ToList();
        }

        public bool ShowConfirm(string message, string title = "Are you sure?")
        {
            return MessageBox.Show(
                       message,
                       title,
                       MessageBoxButton.YesNo,
                       MessageBoxImage.Question
                   ) == MessageBoxResult.Yes;
        }

        public void ShowError(string message, string title = "")
        {
            MessageBox.Show(
                message,
                string.IsNullOrEmpty(title) ? "Error" : title,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        public void ShowError(Exception exception, string message, string title = "")
        {
            var exceptionViewer = new ExceptionViewer(message, exception);
            var baseDialogWindow = new BaseDialogWindow
            {
                Title = string.IsNullOrEmpty(title) ? "Error" : title,
                Content = exceptionViewer,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                IsMinButtonEnabled = false
            };
            baseDialogWindow.ShowDialog();
        }

    }
}