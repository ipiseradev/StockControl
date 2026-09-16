using System.Text.Json;
using System.Text.Json.Serialization;

namespace StockControl.Web.Services;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
