using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LiteDbExplorer.Extensions
{
    public static class EncodingExtensions
    {
        public static string EncodeNonAsciiCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    var encodedValue = "\\u" + ((int) c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string DecodeEncodedNonAsciiCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-fA-F0-9]{4})",//if it hex value then we should check only a-f or parse would crash
                m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }
    }
}