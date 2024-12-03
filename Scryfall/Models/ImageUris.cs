﻿using System.Text.Json.Serialization;

namespace Scryfall.Models;

public class ImageUris
{
    [JsonPropertyName("small")] public string Small { get; set; }

    [JsonPropertyName("normal")] public string Normal { get; set; }

    [JsonPropertyName("large")] public string Large { get; set; }

    [JsonPropertyName("png")] public string Png { get; set; }

    [JsonPropertyName("art_crop")] public string ArtCrop { get; set; }

    [JsonPropertyName("border_crop")] public string BorderCrop { get; set; }
}