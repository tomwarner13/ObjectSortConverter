using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ObjectSortConverter;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests
{
    [TestClass]
    public class ObjectSortConverterTests
    {
        [TestMethod]
        public void BasicStringConversion()
        {
            var str = "hello";

            var result = Convert(str);

            Assert.AreEqual(result, "\"hello\"");
        }

        [TestMethod]
        public void ListOfInts()
        {
            var l = new List<int>() { 3, 5, 1, 4, 2 };

            var result = Convert(l);

            Assert.AreEqual(result, "[1,2,3,4,5]");
        }

        [TestMethod]
        public void DictionaryConvert()
        {
            var d = new Dictionary<int, string>()
            {
                { 2, "two" },
                { 3, "three" },
                { 5, "five" },
                { 4, "four" },
                { 1, "one" },
            };

            var result = Convert(d);

            Assert.AreEqual(result, 
                "{\"1\":\"one\",\"2\":\"two\",\"3\":\"three\",\"4\":\"four\",\"5\":\"five\"}");
        }

        [TestMethod]
        public void BasicObjectConvert()
        {
            var b = new Box()
            {
                Number = 42,
                Name = "Test",
                Ints = new List<int>() { 0, 3, 2, 1 }
            };

            var result = Convert(b);

            //note that properties are also sorted alphabetically
            Assert.AreEqual(result, 
                "{\"$type\":\"Tests.ObjectSortConverterTests+Box, Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Ints\":[0,1,2,3],\"Name\":\"Test\",\"Number\":42}");
        }

        private static string Convert(object value)
        {
            var converter = new SortedObjectConverter();
            var ser = new JsonSerializer();
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using (var writer = new JsonTextWriter(sw))
            {
                converter.WriteJson(writer, value, ser);

                return sb.ToString();
            }
        }

        public class Box
        {
            public int Number;
            public string Name;
            public List<int> Ints;
        }
    }
}
