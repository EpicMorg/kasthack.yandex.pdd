﻿/*
        stuff from https://github.com/kasthack/VKSharp/blob/master/Sources/VKSharp/Helpers/
*/

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace kasthack.yandex.pdd.Helpers {
    internal class SnakeCaseContractResolver : DefaultContractResolver {
        protected override string ResolvePropertyName( string propertyName ) => propertyName.ToSnake();
    }

    //https://stackoverflow.com/questions/17745866
    //converts doubles|strings -> ints
    internal class CustomIntConverter : JsonConverter {
        public override bool CanConvert( Type objectType ) => objectType == typeof( int );

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) {
            var jsonValue = serializer.Deserialize<JValue>( reader );
            switch ( jsonValue.Type ) {
                case JTokenType.Float:
                    return (int) Math.Round( jsonValue.Value<double>() );
                case JTokenType.Integer:
                    return jsonValue.Value<int>();
                case JTokenType.String:
                    return int.Parse( jsonValue.Value<string>() );
            }
            throw new FormatException();
        }

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) => JObject.FromObject( value ).WriteTo( writer );
    }

    /*
        true/1/yes/ok -> true
        false/0/no/error -> false
    */

    internal class PerdonBoolConverter : JsonConverter {
        public override bool CanWrite => false;

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) {
            throw new NotImplementedException();
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) {
            var jsonValue = serializer.Deserialize<JValue>( reader );
            switch ( jsonValue.Type ) {
                case JTokenType.Boolean:
                    return jsonValue.Value<bool>();
                case JTokenType.Integer: {
                    var value = jsonValue.Value<int>();
                    if ( value == 1 ) return true;
                    if ( value == 0 ) return false;
                    break; //anything else -> formatException
                }
                case JTokenType.String: {
                    var value = jsonValue.Value<string>();
                    if ( value == "yes"
                         || value == "ok" ) return true;
                    if ( value == "no"
                         || value == "error" ) return true;
                    break;
                }
                case JTokenType.Null:
                    return null;
            }
            throw new FormatException();
        }

        public override bool CanConvert( Type objectType ) => objectType == typeof( bool ) || objectType == typeof( bool? );
    }

    // "enum_value" -> Enum.EnumValue
    internal class SnakeCaseEnumConverter : StringEnumConverter {
        public override bool CanConvert( Type objectType ) {
            if ( IsNullableType( objectType ) ) objectType = Nullable.GetUnderlyingType( objectType );
            return objectType.IsEnum;
        }

        public new bool AllowIntegerValues
        {
            get
            {
                return base.AllowIntegerValues;
            }
            set
            {
                base.AllowIntegerValues = value;
            }
        }

        public override bool CanWrite => true;

        private static bool IsNullableType( Type t ) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof( Nullable<> );

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) {
            var isNullable = IsNullableType( objectType );
            var t = isNullable ? Nullable.GetUnderlyingType( objectType ) : objectType;
            if ( reader.TokenType == JsonToken.Null ) {
                if ( !IsNullableType( objectType ) ) throw new JsonSerializationException( $"Cannot convert null value to {objectType}." );
                return null;
            }
            try {
                switch ( reader.TokenType ) {
                    case JsonToken.String:
                        return Enum.Parse( t, reader.Value.ToString().ToMeth(), true );
                    case JsonToken.Integer:
                        return base.ReadJson( reader, objectType, existingValue, serializer );
                }
            }
            catch ( Exception ex ) {
                throw new JsonSerializationException( $"Error converting value {reader.Value} to type '{objectType}'.", ex );
            }
            throw new JsonSerializationException( $"Unexpected token {reader.TokenType.ToNCString()} when parsing enum." );
        }
    }
}