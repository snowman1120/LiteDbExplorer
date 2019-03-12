using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
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

            Add(ApplicationCommands.Close, (sender, args) =>
            {
                GetDatabaseReference(sender, args)
                    .OnSuccess(reference =>
                    {
                        _databaseInteractions.CloseDatabase(reference);
                        _eventAggregator.PublishOnUIThread(new InteractionEvents.DatabaseClosed(reference));
                    });

            }, (sender, args) =>
            {
                args.CanExecute = HasDatabaseReference(sender, args);
            });

            Add(ApplicationCommands.Copy, (sender, args) =>
            {
                GetManyDocumentsReference(sender, args)
                    .OnSuccess(references => _databaseInteractions.CopyDocuments(references));

            }, (sender, e) =>
            {
                e.CanExecute = HasAnyDocumentsReference(sender, e, DocumentTypeFilter.BsonDocument);
            });

            Add(ApplicationCommands.Paste, (sender, args) =>
            {
                try
                {
                    GetCollectionReference(sender, args)
                        .OnSuccess(reference =>
                        {
                            var textData = Clipboard.GetText();
                            return _databaseInteractions.ImportDataFromText(reference, textData);
                        })
                        .OnSuccess(update => _eventAggregator.PublishOnUIThread(update));
                }
                catch (Exception exc)
                {
                    Logger.Warn(exc, "Cannot process clipboard data.");
                    MessageBox.Show("Failed to paste document from clipboard: " + exc.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }, (sender, e) =>
            {
                e.CanExecute = NotIsFilesCollection(sender, e) && Clipboard.ContainsText();
            });


            Add(Commands.EditDbProperties, (sender, args) =>
            {
                GetDatabaseReference(sender, args)
                    .OnSuccess(reference => _viewInteractionResolver.OpenDatabaseProperties(reference));

            }, (sender, args) =>
            {
                args.CanExecute = HasDatabaseReference(sender, args);
            });
            
            Add(Commands.Add, (sender, args) =>
            {
                GetCollectionReference(sender, args)
                    .OnSuccess(reference => _databaseInteractions.CreateItem(reference))
                    .OnSuccess(reference =>
                    {
                        _viewInteractionResolver.ActivateCollection(reference.CollectionReference, reference.NewDocuments);
                        _eventAggregator.PublishOnUIThread(reference);
                    });

            }, (sender, args) =>
            {
                args.CanExecute = HasCollectionReference(sender, args);
            });

            Add(Commands.AddFile, (sender, args) =>
            {
                GetDatabaseReference(sender, args)
                    .OnSuccess(reference => _databaseInteractions.AddFileToDatabase(reference))
                    .OnSuccess(reference =>
                    {
                        _viewInteractionResolver.ActivateCollection(reference.CollectionReference, reference.NewDocuments);
                        _eventAggregator.PublishOnUIThread(reference);
                    });
                
            }, (sender, args) =>
            {
                args.CanExecute = HasDatabaseReference(sender, args);
            });

            Add(Commands.Edit, (sender, args) =>
            {

                GetDocumentReference(sender, args)
                    .OnSuccess(reference => _databaseInteractions.OpenEditDocument(reference))
                    .OnSuccess(maybe =>
                    {
                        maybe.Execute(value => _eventAggregator.PublishOnUIThread(new InteractionEvents.DocumentUpdated(value)));
                    });
                
            }, (sender, args) =>
            {
                args.CanExecute = HasDocumentReference(sender, args);
            });

            Add(Commands.AddCollection, (sender, args) =>
            {
                GetDatabaseReference(sender, args)
                    .OnSuccess(reference =>
                    {
                        var addCollection = _databaseInteractions.AddCollection(reference);
                        if (addCollection.IsSuccess)
                        {
                            _viewInteractionResolver.ActivateCollection(addCollection.Value);
                        }
                    });

            }, (sender, args) =>
            {
                args.CanExecute = HasDatabaseReference(sender, args);
            });

            Add(Commands.Remove, (sender, args) =>
            {
                GetManyDocumentsReference(sender, args)
                    .OnSuccess(references => _databaseInteractions.RemoveDocuments(references));

            }, (sender, args) =>
            {
                args.CanExecute = HasAnyDocumentsReference(sender, args);
            });

            Add(Commands.Export, (sender, args) =>
            {
                GetManyDocumentsReference(sender, args)
                    .OnSuccess(references => _databaseInteractions.ExportDocuments(references.ToList()));
               
            }, (sender, args) =>
            {
                args.CanExecute = HasAnyDocumentsReference(sender, args);
            });

            Add(Commands.RefreshCollection, (sender, args) =>
            {
                GetCollectionReference(sender, args)
                    .OnSuccess(reference => reference.Refresh());
                
            }, (sender, e) =>
            {
                e.CanExecute = HasCollectionReference(sender, e);
            });
            
            Add(Commands.RenameCollection, (sender, args) =>
            {
                GetCollectionReference(sender, args)
                    .OnSuccess(reference => _databaseInteractions.RenameCollection(reference));

            }, (sender, args) =>
            {
                args.CanExecute = NotIsFilesCollection(sender, args);
            });

            Add(Commands.DropCollection, (sender, args) =>
            {
                GetCollectionReference(sender, args)
                    .OnSuccess(reference =>
                    {
                        var dropCollection = _databaseInteractions.DropCollection(reference);
                        if (dropCollection.IsSuccess)
                        {
                            _eventAggregator.PublishOnUIThread(new InteractionEvents.CollectionRemoved(reference));
                        }
                    });
            }, (sender, args) =>
            {
                args.CanExecute = HasCollectionReference(sender, args);
            });


            Add(Commands.ExportCollection, (sender, args) =>
            {
                GetCollectionReference(sender, args)
                    .OnSuccess(reference => _databaseInteractions.ExportCollection(reference));

            }, (sender, args) =>
            {
                args.CanExecute = HasCollectionReference(sender, args);
            });


            Add(Commands.RefreshDatabase, (sender, args) =>
            {
                GetDatabaseReference(sender, args)
                    .OnSuccess(reference => reference.Refresh());
                
            }, (sender, args) =>
            {
                args.CanExecute = HasDatabaseReference(sender, args);
            });

            Add(Commands.RevealInExplorer, (sender, args) =>
            {
                GetDatabaseReference(sender, args)
                    .OnSuccess(reference => _databaseInteractions.RevealInExplorer(reference));

            }, (sender, args) =>
            {
                args.CanExecute = HasDatabaseReference(sender, args);
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

    public static class LiteDbReferenceExtensions
    {
        public static bool HasAnyDocumentsReference(this IEnumerable<DocumentReference> documentReferences, DocumentTypeFilter filter = DocumentTypeFilter.All)
        {
            if (documentReferences == null)
            {
                return false;
            }

            if (filter == DocumentTypeFilter.All)
            {
                return documentReferences.Any();
            }

            return documentReferences
                       .Where(p => p.Collection != null)
                       .All(p => filter == DocumentTypeFilter.File ? p.Collection.IsFilesOrChunks : !p.Collection.IsFilesOrChunks);
        }
        
        public static bool IsFilesCollection(this CollectionReference collectionReference)
        {
            return collectionReference != null && collectionReference.IsFilesOrChunks;
        }
    }
}