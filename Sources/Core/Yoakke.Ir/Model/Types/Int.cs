// Copyright (c) 2021 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using Yoakke.Ir.Model.Values;

namespace Yoakke.Ir.Model.Types
{
    /// <summary>
    /// Represents a signed integer value.
    /// </summary>
    public record Int(IConstant Bits) : IType
    {
        /// <inheritdoc/>
        public IType Type => Types.Type.Instance;

        /// <inheritdoc/>
        public override string ToString() => $"i{{{this.Bits}}}";
    }
}