using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework.Services;
using LogManager = NLog.LogManager;

namespace LiteDbExplorer.Modules
{
    public class DefaultCommandsHandler : ApplicationCommandHandler
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatabaseInteractions _databaseInteractions;
        private readonly IViewInteractionResolver _viewInteractionResolver;
        private readonly IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public DefaultCommandsHandler(
            IDatabaseInteractions databaseInteractions,
            IViewInteractionResolver viewInteractionResolver,
            IEventAggregator eventAggregator)
        {
            _databaseInteractions = databaseInteractions;
            _viewInteractionResolver = viewInteractionResolver;
            _eventAggregator = eventAggregator;

            Add(Commands.Exit, (sender, args) =>
            {
                Store.Current.CloseDatabases();

                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                }
            });

            Add(ApplicationCommands.Open, (sender, args) =>
            {
                _databaseInteractions.OpenDatabase();
            });

            Add(ApplicationCommands.New, (sender, args) =>
            {
                _databaseInteractions.CreateAndOpenDatabase();
            });
        }

        private Result<DatabaseReference> GetDatabaseReference(object sender, ExecutedRoutedEventArgs args)
        {
            Maybe<DatabaseReference> maybe;
            if (args.Parameter is DatabaseReference databaseReference)
            {
                maybe = databaseReference;
            }
            else if (args.Parameter is CollectionReference collectionReference)
            {
                maybe = collectionReference.Database;
            }
            else
            {
                maybe = Store.Current.SelectedDatabase;
            }

            return maybe.ToResult($"{nameof(DatabaseReference)} not provided.");
        }

        private bool HasDatabaseReference(object sender, CanExecuteRoutedEventArgs args)
        {
            if (args.Parameter is CollectionReference collectionReference)
            {
                return collectionReference.Database != null;
            }

            // var element = args.OriginalSource as FrameworkElement;
            // var dataContext = element?.DataContext;

            return args.Parameter is DatabaseReference || Store.Current.HasSelectedDatabase;
        }

        private Result<CollectionReference> GetCollectionReference(object sender, ExecutedRoutedEventArgs args)
        {
            Maybe<CollectionReference> maybe;
            if (args.Parameter is CollectionReference collectionReference)
            {
                maybe = collectionReference;
            }
            else
            {
                maybe = Store.Current.SelectedCollection;
            }

            return maybe.ToResult($"{nameof(CollectionReference)} not provided.");
        }

        private bool HasCollectionReference(object sender, CanExecuteRoutedEventArgs args)
        {
            return args.Parameter is CollectionReference || Store.Current.HasSelectedCollection;
        }

        Result<DocumentReference> GetDocumentReference(object sender, ExecutedRoutedEventArgs args)
        {
            Maybe<DocumentReference> maybe;
            if (args.Parameter is DocumentReference documentReference)
            {
                maybe = documentReference;
            }
            else
            {
                maybe = Store.Current.SelectedDocument;
            }

            return maybe.ToResult($"{nameof(DocumentReference)} not provided.");
        }

        private bool HasDocumentReference(object sender, CanExecuteRoutedEventArgs args)
        {
            return args.Parameter is DocumentReference || Store.Current.HasSelectedDocument;
        }

        Result<IEnumerable<DocumentReference>> GetManyDocumentsReference(object sender, ExecutedRoutedEventArgs args)
        {
            Maybe<IEnumerable<DocumentReference>> maybe;
            if (args.Parameter is IEnumerable<DocumentReference> documentReference)
            {
                maybe = Maybe<IEnumerable<DocumentReference>>.From(documentReference);
            }
            else
            {
                maybe = Maybe<IEnumerable<DocumentReference>>.From(Store.Current.SelectedDocuments);
            }

            return maybe.ToResult($"{nameof(DocumentReference)} not provided.");
        }

        private bool HasAnyDocumentsReference(object sender, CanExecuteRoutedEventArgs args, DocumentTypeFilter filter = DocumentTypeFilter.All)
        {
            return (args.Parameter as IEnumerable<DocumentReference> ?? Store.Current.SelectedDocuments).HasAnyDocumentsReference(filter);
        }

        public bool NotIsFilesCollection(object sender, CanExecuteRoutedEventArgs args)
        {
            return !(args.Parameter as CollectionReference ?? Store.Current.SelectedCollection).IsFilesCollection();
        }
    }
}