// Copyright (c) 2021 Yoakke.
// Licensed under the Apache License, Version 2.0.
// Source repository: https://github.com/LanguageDev/Yoakke

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Yoakke.Ir.Model.Attributes;

namespace Yoakke.Ir.Model
{
    /// <summary>
    /// Base of all IR value types.
    /// </summary>
    public abstract record Type : Constant, IReadOnlyAttributeTarget
    {
        /// <inheritdoc/>
        public Attributes.AttributeTargets Flag => this.attributeTarget.Flag;

        private readonly AttributeTarget attributeTarget = new(Attributes.AttributeTargets.TypeDefinition);

        /// <inheritdoc/>
        public IEnumerable<IAttribute> GetAttributes() => this.attributeTarget.GetAttributes();

        /// <inheritdoc/>
        public IEnumerable<IAttribute> GetAttributes(string name) => this.attributeTarget.GetAttributes(name);

        /// <inheritdoc/>
        public IEnumerable<TAttrib> GetAttributes<TAttrib>()
            where TAttrib : IAttribute => this.attributeTarget.GetAttributes<TAttrib>();

        /// <inheritdoc/>
        public bool TryGetAttribute(string name, [MaybeNullWhen(false)] out IAttribute attribute) =>
            this.attributeTarget.TryGetAttribute(name, out attribute);

        /// <inheritdoc/>
        public bool TryGetAttribute<TAttrib>([MaybeNullWhen(false)] out TAttrib attribute)
            where TAttrib : IAttribute => this.attributeTarget.TryGetAttribute(out attribute);
    }
}