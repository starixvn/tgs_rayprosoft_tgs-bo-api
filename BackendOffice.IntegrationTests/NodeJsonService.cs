using System.Text.Json;
using System.Text.Json.Serialization;
using Jering.Javascript.NodeJS;

namespace BackendOffice.IntegrationTests;

internal class NodeJsonService : IJsonService
{
    private readonly static JsonSerializerOptions _SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new InvocationRequestConverter(),
            new JsonStringEnumConverter()
        }
    };

    public ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<T>(
            stream,
            _SerializerOptions,
            cancellationToken);

    public Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default)
        => JsonSerializer.SerializeAsync(
            stream,
            value,
            _SerializerOptions,
            cancellationToken);

    private class InvocationRequestConverter : JsonConverter<InvocationRequest>
    {
        public override InvocationRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<InvocationRequest>(
                ref reader,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

        public override void Write(Utf8JsonWriter writer, InvocationRequest value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber(
                options.PropertyNamingPolicy?.ConvertName(nameof(InvocationRequest.ModuleSourceType))
                    ?? nameof(InvocationRequest.ModuleSourceType),
                (int)value.ModuleSourceType);

            if (value.ModuleSource is not null)
                writer.WriteString(
                    options.PropertyNamingPolicy?.ConvertName(nameof(InvocationRequest.ModuleSource))
                    ?? nameof(InvocationRequest.ModuleSource),
                value.ModuleSource);

            if (value.CacheIdentifier is not null)
                writer.WriteString(
                    options.PropertyNamingPolicy?.ConvertName(nameof(InvocationRequest.CacheIdentifier))
                        ?? nameof(InvocationRequest.CacheIdentifier),
                    value.CacheIdentifier);

            if (value.ExportName is not null)
                writer.WriteString(
                    options.PropertyNamingPolicy?.ConvertName(nameof(InvocationRequest.ExportName))
                        ?? nameof(InvocationRequest.ExportName),
                    value.ExportName);

            if (value.Args is not null)
            {
                writer.WritePropertyName(
                    options.PropertyNamingPolicy?.ConvertName(nameof(InvocationRequest.Args))
                        ?? nameof(InvocationRequest.Args));
                JsonSerializer.Serialize(writer, value.Args, options);
            }

            writer.WriteEndObject();
        }
    }
}
