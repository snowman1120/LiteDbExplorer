using LiteDB;

namespace LiteDbExplorer.Extensions
{
    public static class JsonSerializerExtension
    {
        public static string SerializeDecoded(this BsonValue bsonValue, bool pretty = false)
        {
            var json = JsonSerializer.Serialize(bsonValue, pretty, false);

            return EncodingExtensions.DecodeEncodedNonAsciiCharacters(json);
        }
    }
}