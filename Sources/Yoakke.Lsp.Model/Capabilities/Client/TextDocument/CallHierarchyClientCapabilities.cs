﻿// Copyright (c) 2021 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using Newtonsoft.Json;

namespace Yoakke.Lsp.Model.Capabilities.Client.TextDocument
{
    public class CallHierarchyClientCapabilities
    {
        /// <summary>
        /// Whether implementation supports dynamic registration. If this is set to
        /// `true` the client supports the new `(TextDocumentRegistrationOptions &
        /// StaticRegistrationOptions)` return value for the corresponding server
        /// capability as well.
        /// </summary>
        [JsonProperty("dynamicRegistration", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DynamicRegistration { get; set; }
    }
}