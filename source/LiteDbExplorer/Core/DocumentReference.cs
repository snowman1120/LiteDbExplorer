using LiteDB;
using System.ComponentModel;
using LiteDbExplorer.Annotations;

namespace LiteDbExplorer
{
    public class DocumentReference : INotifyPropertyChanged
    {
        public DocumentReference()
        {
        }

        public DocumentReference(BsonDocument document, CollectionReference collection)
        {
            LiteDocument = document;
            Collection = collection;
        }

        public BsonDocument LiteDocument { get; set; }

        public CollectionReference Collection { get; set; }

        [UsedImplicitly]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
