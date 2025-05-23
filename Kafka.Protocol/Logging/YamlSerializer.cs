﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Kafka.Protocol.Logging
{
    internal static class YamlSerializer
    {
        internal static string Serialize(object? obj) =>
            Serialize(obj, string.Empty, out _);

        private static string Serialize(object? obj, string indentation, out bool simple)
        {
            if (TrySerializeSimpleType(obj, 
                    indentation, 
                    out var simpleValue))
            {
                simple = true;
                return simpleValue;
            }

            simple = false;
            var type = obj.GetType();

            var isMap = IsMap(type);
            if (obj is IEnumerable list)
            {
                var enumerable = string.Join(Environment.NewLine,
                    list.Cast<object?>()
                        .Select(
                            item => isMap
                                ? Serialize(item, indentation, out _)
                                : $"{indentation}- {Serialize(item, indentation + "  ", out _).TrimStart()}"));
                if (enumerable != string.Empty)
                {
                    return enumerable;
                }
                simple = true;
                return isMap ? "{}" : "[]";
            }

            if (IsKeyValuePair(type))
            {
                var key = type.GetProperty("Key")!.GetValue(obj);
                var value = type.GetProperty("Value")!.GetValue(obj);

                return $"{indentation}? {Serialize(key, indentation + "  ", out _).TrimStart()}{Environment.NewLine}" +
                       $"{indentation}: {Serialize(value, indentation + "  ", out _).TrimStart()}";
            }

            var properties = GetProperties(type);
            return string.Join(Environment.NewLine, properties.Select(
                property =>
                {
                    var value = property.GetValue(obj, null);
                    var serializedValue =
                        Serialize(value, indentation + "  ", out var isSimple);
                    return
                        $"{indentation}{property.Name}: {(isSimple ? string.Empty : Environment.NewLine)}{serializedValue}";
                }));
        }

        private static bool IsMap(Type type) =>
            type.GetInterfaces().Any(@interface =>
                (@interface.IsGenericType &&
                 @interface.GetGenericTypeDefinition() ==
                 typeof(IDictionary<,>)) ||
                @interface == typeof(IDictionary));

        private static bool IsKeyValuePair(Type type) =>
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);

        private static IEnumerable<PropertyInfo> GetProperties(Type type) =>
            type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(info =>
                    info.CanRead && info.GetIndexParameters().Length == 0);

        private static bool TrySerializeSimpleType([NotNullWhen(false)] object? obj, 
            string indentation,
            [NotNullWhen(true)] out string? serializedValue)
        {
            if (obj is null)
            {
                serializedValue = "null";
                return true;
            }

            var type = obj.GetType();
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            serializedValue = obj switch
            {
                string str when str == string.Empty => "\"\"",
                string str when str.All(c => c == ' ') => $"\"{str}\"",
                // https://yaml.org/spec/1.2.2/#812-literal-style
                string str => $"|{Environment.NewLine}{indentation}{str.Replace(Environment.NewLine, $"{Environment.NewLine}{indentation}")}",
                bool boolValue => boolValue ? "true" : "false",
                decimal value => value.ToString(NumberFormatInfo.InvariantInfo),
                double value => value.ToString(NumberFormatInfo.InvariantInfo),
                DateTimeOffset value => value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ"),
                DateTime value => value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ"),
                TimeSpan value => value.ToString(@"d\.hh\:mm\:ss\.fff"),
                Guid guid => guid.ToString(),
                _ when type.IsPrimitive || type.IsEnum => obj.ToString(),
                _ => null
            };

            if (serializedValue != null)
            {
                return true;
            }


            serializedValue = null;
            return false;

        }
    }
}