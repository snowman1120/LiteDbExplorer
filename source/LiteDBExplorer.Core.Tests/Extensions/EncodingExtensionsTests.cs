using LiteDbExplorer.Extensions;
using NUnit.Framework;

namespace LiteDBExplorer.Core.Tests.Extensions
{
    [TestFixture]
    public class EncodingExtensionsTests
    {
        [Test]
        [TestCase(@"\ucTa", @"\ucTa")] //not nex value
        [TestCase(@"\u12af", @"ኯ")]  //valid hex value
        public void DecodeEncodedNonAsciiCharacters(string value, string expected)
        {
            var actual = EncodingExtensions.DecodeEncodedNonAsciiCharacters(value);
            Assert.AreEqual(expected, actual);
        }
    }
}