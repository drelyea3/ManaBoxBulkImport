﻿using System.Text.Json.Serialization;

namespace Scryfall.Models;

public class ImageUris
{
    [JsonPropertyName("small")] public Uri Small { get; set; }

    [JsonPropertyName("normal")] public Uri Normal { get; set; }

    [JsonPropertyName("large")] public Uri Large { get; set; }

    [JsonPropertyName("png")] public Uri Png { get; set; }

    [JsonPropertyName("art_crop")] public Uri ArtCrop { get; set; }

    [JsonPropertyName("border_crop")] public Uri BorderCrop { get; set; }
}