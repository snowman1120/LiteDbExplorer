using System.Collections.Generic;
using LiteDB;

namespace LiteDbExplorer.Modules
{
    public class InteractionEvents
    {
        public class DocumentsUpdated
        {
            public string Source { get; }

            public IReadOnlyCollection<BsonDocument> Documents { get; }

            public DocumentsUpdated(string source, BsonDocument document) : this(source, new []{document})
            {
            }

            public DocumentsUpdated(string source, IReadOnlyCollection<BsonDocument> documents)
            {
                Source = source;
                Documents = documents;

                // TODO: Handle UpdateGridColumns(doc);
                // TODO: Handle UpdateDocumentPreview();
                // TODO: Handle CollectionListView.ScrollIntoSelectedItem();
            }
        }
    }
}