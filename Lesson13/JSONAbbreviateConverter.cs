using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lesson13
{
    internal class JSONAbbreviateConverter : JsonConverter<Squad>
    {

        private object? GetField(ref Utf8JsonReader reader, string fieldName)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Number:
                    return reader.GetInt32();
                case JsonTokenType.True:
                case JsonTokenType.False:
                    return reader.GetBoolean();
                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException($"\"{fieldName}\" must be {reader.TokenType}");
            }
        }
        public override Squad? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            Squad s = new Squad();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return s;
                }

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string? propertyName = reader.GetString();
                reader.Read();
                
                switch (propertyName)
                {
                    case "sn":
                        s.SquadName = (string)GetField(ref reader, "sn");
                        break;
                    case "ht":
                        s.HomeTown = (string)GetField(ref reader, "ht");
                        break;
                    case "f":
                        s.Formed = (int)GetField(ref reader, "f");
                        break;
                    case "sb":
                        s.SecretBase = (string)GetField(ref reader, "sb");
                        break;
                    case "a":
                        s.Active = (bool)GetField(ref reader, "a");
                        break;

                    case "m":
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            List<Member> lm = new List<Member>();
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    Member m = new Member();
                                    while (reader.Read())
                                    {
                                        if (reader.TokenType == JsonTokenType.EndObject) break;
                                        if (reader.TokenType != JsonTokenType.PropertyName)
                                        {
                                            throw new JsonException();
                                        }
                                        string innerPropertyName = reader.GetString();
                                        reader.Read();
                                        switch (innerPropertyName)
                                        {
                                            case "n":
                                                m.Name = (string)GetField(ref reader, "n");
                                                break;
                                            case "a":
                                                m.Age = (int)GetField(ref reader, "a");
                                                break;
                                            case "si":
                                                m.SecretIdentity = (string)GetField(ref reader, "si");
                                                break;

                                            case "p":
                                                if (reader.TokenType == JsonTokenType.StartArray)
                                                {
                                                    List<string> l = new List<string>();
                                                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                                                    {
                                                        if (reader.TokenType != JsonTokenType.String && reader.TokenType != JsonTokenType.Null)
                                                            throw new JsonException($"\"p\" elements must be \"{JsonTokenType.String}\"");
                                                        l.Add(reader.GetString());
                                                    }
                                                    m.Powers = l;
                                                }
                                                else if (reader.TokenType == JsonTokenType.Null)
                                                {
                                                    m.Powers = null;
                                                }
                                                else throw new JsonException($"\"p\" must be \"{JsonTokenType.StartArray}\"");
                                                break;

                                            default:
                                                throw new JsonException($"\"m\" must be \"{JsonTokenType.StartObject}\"");
                                        }
                                    }
                                    lm.Add(m);
                                }
                                else throw new JsonException($"\"m\" element is not \"{JsonTokenType.StartObject}\"");
                            }
                            s.Members = lm;
                        }

                        
                        break;
                    default:
                        throw new JsonException($"Unknown field \"{propertyName}\"");
                }
            }
            return s;
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
