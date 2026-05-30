using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xenocide.Utils
{
    public class ModelJsonConverter : JsonConverter<object>
    {
        private static readonly ConcurrentDictionary<string, Type> TypeCache = new();
        private static readonly HashSet<string> ModelNamespaces = new()
        {
            "ProjectXenocide.Model",
            "ProjectXenocide.UI",
            "ProjectXenocide.Utils"
        };

        private static readonly HashSet<Type> NonModelTypes = new()
        {
            typeof(string), typeof(decimal),
            typeof(Vector2Json), typeof(Vector3Json), typeof(Vector4Json)
        };

        private static readonly HashSet<Type> NonSerializableTypes = new()
        {
            typeof(IntPtr), typeof(UIntPtr),
            typeof(RuntimeTypeHandle), typeof(RuntimeFieldHandle), typeof(RuntimeMethodHandle)
        };

        [ThreadStatic]
        private static Dictionary<object, string> _writeRefs;

        [ThreadStatic]
        private static Dictionary<string, object> _readRefs;

        [ThreadStatic]
        private static int _writeNextId;

        [ThreadStatic]
        private static int _callDepth;

        static ModelJsonConverter()
        {
            var assembly = typeof(ModelJsonConverter).Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (ShouldRegister(type))
                {
                    TypeCache[type.FullName] = type;
                }
            }
        }

        private static bool ShouldRegister(Type type)
        {
            if (type.IsGenericTypeDefinition) return false;
            if (NonModelTypes.Contains(type)) return false;
            if (type.Namespace == null) return false;
            foreach (var ns in ModelNamespaces)
            {
                if (type.Namespace.StartsWith(ns, StringComparison.Ordinal)) return true;
            }
            if (type.GetCustomAttribute<SerializableAttribute>() != null) return true;
            return false;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.IsValueType) return false;
            if (typeToConvert == typeof(string)) return false;
            if (typeToConvert.IsArray) return false;
            if (typeToConvert.IsGenericType)
            {
                var open = typeToConvert.GetGenericTypeDefinition();
                if (open == typeof(List<>) || open == typeof(Dictionary<,>) ||
                    open == typeof(Queue<>) || open == typeof(Stack<>) ||
                    open == typeof(HashSet<>) || open == typeof(LinkedList<>))
                    return false;
            }
            return TypeCache.ContainsKey(typeToConvert.FullName) ||
                   ShouldRegister(typeToConvert);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is null) { writer.WriteNullValue(); return; }

            _callDepth++;
            try
            {
                if (_writeRefs == null)
                {
                    _writeRefs = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);
                    _writeNextId = 0;
                }

                if (_writeRefs.TryGetValue(value, out var existingId))
                {
                    writer.WriteStartObject();
                    writer.WriteString("$ref", existingId);
                    writer.WriteEndObject();
                    return;
                }

                var id = (_writeNextId++).ToString(CultureInfo.InvariantCulture);
                _writeRefs[value] = id;

                var type = value.GetType();
                writer.WriteStartObject();
                writer.WriteString("$id", id);
                writer.WriteString("$type", type.FullName);

                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.IsStatic) continue;
                    if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
                    if (NonSerializableTypes.Contains(field.FieldType)) continue;
                    writer.WritePropertyName(field.Name);
                    JsonSerializer.Serialize(writer, field.GetValue(value), field.FieldType, options);
                }

                writer.WriteEndObject();
            }
            finally
            {
                _callDepth--;
                if (_callDepth == 0)
                {
                    _writeRefs = null;
                }
            }
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;

            _callDepth++;
            try
            {
                if (_readRefs == null)
                {
                    _readRefs = new Dictionary<string, object>();
                }

                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                if (root.TryGetProperty("$ref", out var refProp))
                {
                    var refId = refProp.GetString();
                    if (_readRefs.TryGetValue(refId, out var existing))
                        return existing;

                    return null;
                }

                var typeName = root.TryGetProperty("$type", out var typeProp)
                    ? typeProp.GetString()
                    : typeToConvert.FullName;

                if (!TypeCache.TryGetValue(typeName, out var actualType))
                {
                    actualType = typeToConvert;
                }

                // Instance creation rationale:
                //
                // RuntimeHelpers.GetUninitializedObject creates an object without executing
                // its constructor, so fields can be populated via reflection below. This is
                // necessary because model types (~100+) have only parameterized constructors
                // (e.g. Combatant(CombatantInfo, int), Aircraft(ItemInfo)) that cannot be
                // used for deserialization — they require runtime arguments not present in
                // the JSON save data.
                //
                // Alternatives considered:
                //   A) Add parameterless constructors to all ~90 model types + rework 10+
                //      field initializers that allocate objects (e.g. new GeoTime()) which
                //      would be immediately overwritten by JSON — wasted allocations.
                //   B) Full System.Text.Json migration with [JsonConstructor] — would break
                //      save file compatibility (JSON metadata format differs) and require
                //      explicit derived-type registration for ~50+ polymorphic types.
                //   C) (Current) RuntimeHelpers.GetUninitializedObject — not deprecated,
                //      preserves save compatibility, handles complex type graphs cleanly.
                //
                // Previously used FormatterServices.GetUninitializedObject, which was
                // deprecated in .NET 9 as part of the legacy binary-formatter removal.
                var instance = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(actualType);

                if (root.TryGetProperty("$id", out var idProp))
                {
                    _readRefs[idProp.GetString()] = instance;
                }

                var fields = actualType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.IsStatic) continue;
                    if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                    if (root.TryGetProperty(field.Name, out var fieldProp))
                    {
                        var raw = fieldProp.GetRawText();
                        var bytes = Encoding.UTF8.GetBytes(raw);
                        var fieldValue = JsonSerializer.Deserialize(bytes.AsSpan(), field.FieldType, options);
                        field.SetValue(instance, fieldValue);
                    }
                }

                var onDeserialized = actualType.GetMethod("OnDeserializedMethod",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new[] { typeof(StreamingContext) }, null);
                if (onDeserialized != null)
                {
                    onDeserialized.Invoke(instance, new object[] { new StreamingContext() });
                }

                return instance;
            }
            finally
            {
                _callDepth--;
                if (_callDepth == 0)
                {
                    _readRefs = null;
                }
            }
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }

#pragma warning disable CS0649 // Fields populated via JSON deserialization
    internal struct Vector2Json { public float X; public float Y; }
    internal struct Vector3Json { public float X; public float Y; public float Z; }
    internal struct Vector4Json { public float X; public float Y; public float Z; public float W; }
#pragma warning restore CS0649
}
