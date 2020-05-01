﻿using Newtonsoft.Json;

namespace PackagingLib
{
    [JsonObject]
    public class EncryptionDetails
    {
        [JsonProperty("algo")]
        public string Algorithm { get; set; }

        [JsonProperty("hash")]
        public string HashAlgorithm { get; set; }

        [JsonProperty("salt")]
        public byte[] Salt { get; set; }
    }
}