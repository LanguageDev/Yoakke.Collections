﻿// Copyright (c) 2021 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using Yoakke.Symbols;
using Yoakke.Text;

namespace Yoakke.Sample
{
    public class VarSymbol : ISymbol
    {
        public IReadOnlyScope Scope { get; }

        public string Name { get; }

        public Location? Definition { get; }

        public VarSymbol(IReadOnlyScope scope, string name, Location? definition = null)
        {
            this.Scope = scope;
            this.Name = name;
            this.Definition = definition;
        }
    }
}