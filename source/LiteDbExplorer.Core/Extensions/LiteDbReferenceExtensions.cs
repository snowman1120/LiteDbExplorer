using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer;
using LiteDB;

namespace LiteDbExplorer.Core
{
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

        public static string ToDisplayName(this DocumentReference documentReference)
        {
            if (documentReference == null)
            {
                return string.Empty;
            }
            
            return string.Join(" - ", documentReference.Collection?.Name, documentReference.LiteDocument["_id"].AsString);
        }

        public static string ToDisplayValue(this BsonValue bsonValue, int? maxLength = null)
        {
            if (bsonValue == null)
            {
                return string.Empty;
            }

            try
            {
                if (bsonValue.IsDocument)
                {
                    return "[Document]";
                }
                if (bsonValue.IsArray)
                {
                    return "[Array]";
                }
                if (bsonValue.IsBinary)
                {
                    return "[Binary]";
                }
                if (bsonValue.IsString)
                {
                    return maxLength.HasValue ? bsonValue.AsString.Truncate(maxLength.Value) : bsonValue.AsString;
                }
                if (bsonValue.IsObjectId)
                {
                    return bsonValue.AsString;
                }
                if (bsonValue.IsDateTime)
                {
                    return bsonValue.AsDateTime.ToString(CultureInfo.InvariantCulture);
                }
                if (bsonValue.IsInt32)
                {
                    return bsonValue.AsInt32.ToString(CultureInfo.InvariantCulture);
                }
                if (bsonValue.IsInt64)
                {
                    return bsonValue.AsInt64.ToString(CultureInfo.InvariantCulture);
                }
                if (bsonValue.IsDouble)
                {
                    return bsonValue.AsDouble.ToString(CultureInfo.InvariantCulture);
                }
                if (bsonValue.IsDecimal)
                {
                    return bsonValue.AsDecimal.ToString(CultureInfo.InvariantCulture);
                }
                if (bsonValue.IsGuid)
                {
                    return bsonValue.AsGuid.ToString("D");
                }
                
                if (maxLength.HasValue)
                {
                    return bsonValue.ToString().Truncate(maxLength.Value);
                }

                return bsonValue.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}