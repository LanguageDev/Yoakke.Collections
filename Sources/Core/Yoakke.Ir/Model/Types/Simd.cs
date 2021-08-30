// Copyright (c) 2021 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using Yoakke.Ir.Model.Values;

namespace Yoakke.Ir.Model.Types
{
    /// <summary>
    /// Represents a fixed-size SIMD vector of elements.
    /// </summary>
    public record Simd(IType Element, IConstant Size) : IType
    {
        /// <inheritdoc/>
        public IType Type => Types.Type.Instance;

        /// <inheritdoc/>
        public override string ToString() => $"simd[{this.Size}]{this.Element}";
    }
}