using g3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sutro.Core.Settings
{
    public class Interval1iConverter : JsonConverter<Interval1i>
    {
        private static readonly string MinPropertyName = "Min";
        private static readonly string MaxPropertyName = "Max";

        public override Interval1i ReadJson(JsonReader reader, Type objectType, [AllowNull] Interval1i existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var interval = new Interval1i(Interval1i.Empty);

            var data = JObject.Load(reader);

            var min = data.Property(MinPropertyName);
            if (min.HasValues)
            {
                if (min.Count != 1)
                    throw new JsonException("Malformed Json");
                interval.a = min.First.Value<int>();
            }

            var max = data.Property(MaxPropertyName);
            if (max.HasValues)
            {
                if (max.Count != 1)
                    throw new JsonException("Malformed Json");
                interval.b = max.First.Value<int>();
            }

            return interval;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Interval1i value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(MinPropertyName);
            writer.WriteValue(value.a);
            writer.WritePropertyName(MaxPropertyName);
            writer.WriteValue(value.b);
            writer.WriteEndObject();
        }
    }
}
