using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lesson13
{
    internal class JSONAbbreviateConverter : JsonConverter<Squad>
    {
        public override Squad? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("This converter can only write");
        }

        public override void Write(Utf8JsonWriter writer, Squad value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("sn", value.SquadName);
            writer.WriteString("ht", value.HomeTown);
            writer.WriteNumber("f", value.Formed);
            writer.WriteString("sb", value.SecretBase);
            writer.WriteBoolean("a", value.Active);

            writer.WriteStartArray("m");
            foreach (Member m in value.Members)
            {
                writer.WriteStartObject();
                writer.WriteString("n", value.HomeTown);
                writer.WriteNumber("a", value.Formed);
                writer.WriteString("si", value.SecretBase);
                writer.WriteStartArray("p");
                foreach (string power in m.Powers)
                {
                    writer.WriteStringValue(power);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}
