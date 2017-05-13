using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ObjectSortConverter
{
    /// <summary>
    /// A converter for JSON.NET that serializes dictionaries with the keys in order,
    /// orders object members by name alphabetically, and sorts all lists using their default comparers.
    /// This means we can compare checksums to verify that 2 serialized files are identical.
    /// </summary>
    public class SortedObjectConverter : JsonConverter
    {
        /// <summary>
        /// Sorts dictionary keys and object members in alphabetical order; sorts lists; writes everything else as normal
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => WriteValue(writer, value);

        private void WriteValue(JsonWriter writer, object value)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var t = JToken.FromObject(value);
            switch (t.Type)
            {
                case JTokenType.Object:
                    WriteObject(writer, value);
                    break;
                case JTokenType.Array:
                    WriteArray(writer, value);
                    break;
                default:
                    writer.WriteValue(value);
                    break;
            }
        }

        private void WriteObject(JsonWriter writer, object value)
        {
            writer.WriteStartObject();

            var dict = value as IDictionary;
            if (dict != null)
            {
                var pairs = dict.Entries().Select(d => new KeyValuePair<string, object>(d.Key.ToString(), d.Value)).ToList();

                pairs.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.InvariantCultureIgnoreCase));

                foreach (var kvp in pairs)
                {
                    writer.WritePropertyName(kvp.Key);
                    WriteValue(writer, kvp.Value);
                }
            }
            else
            {
                var type = value.GetType();

                //write types so that we can deserialize implementations of abstract classes properly
                writer.WritePropertyName("$type");
                writer.WriteValue($"{type.FullName}, {type.Assembly}");

                var members =
                    type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m is PropertyInfo || m is FieldInfo)
                        .OrderBy(m => m.Name)
                        .ToList();

                foreach (var member in members)
                {
                    writer.WritePropertyName(member.Name);
                    WriteValue(writer, GetValue(member, value));
                }
            }

            writer.WriteEndObject();
        }

        private void WriteArray(JsonWriter writer, object value)
        {
            writer.WriteStartArray();
            var list = ((IEnumerable) value).Cast<object>().ToList();

            list.Sort();

            foreach (var o in list)
            {
                WriteValue(writer, o);
            }
            writer.WriteEndArray();
        }

        /// <summary>
        /// Don't use this class to read; it will throw an exception
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns>An exception. You been warned.</returns>
        /// <exception cref="NotImplementedException">This class is not for reading</exception>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("This ain't no reader class");
        }

        public override bool CanConvert(Type objectType) => true;

        public override bool CanRead => false;

        private static object GetValue(MemberInfo info, object data)
        {
            var fieldInfo = info as FieldInfo;
            return fieldInfo != null
                ? fieldInfo.GetValue(data)
                : ((PropertyInfo) info).GetValue(data, null);
        }
    }

    // https://stackoverflow.com/questions/9713311/how-to-get-an-ienumerabledictionaryentry-out-of-an-idictionary/9713698#9713698
    internal static class EnumerableExt
    {
        public static IEnumerable<DictionaryEntry> Entries(this IDictionary dict)
        {
            foreach (var item in dict) yield return (DictionaryEntry)item;
        }
    }
}