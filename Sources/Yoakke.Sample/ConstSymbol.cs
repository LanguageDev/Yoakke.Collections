﻿// Copyright (c) 2021 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using Yoakke.Symbols;
using Yoakke.Text;

namespace Yoakke.Sample
{
    public class ConstSymbol : ISymbol
    {
        public IReadOnlyScope Scope { get; }

        public string Name { get; }

        public Location? Definition { get; }

        public readonly object? Value;

        public ConstSymbol(IReadOnlyScope scope, string name, object? value, Location? definition = null)
        {
            this.Scope = scope;
            this.Name = name;
            this.Value = value;
            this.Definition = definition;
        }
    }
}