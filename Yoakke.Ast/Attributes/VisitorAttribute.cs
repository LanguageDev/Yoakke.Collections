﻿using System;

namespace Yoakke.Ast.Attributes
{
    /// <summary>
    /// An attribute to denote the generation of a visitor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class VisitorAttribute : Attribute
    {
        /// <summary>
        /// The name of the visitor to generate.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The type to return from the visitor calls.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Initializes a new <see cref="VisitorAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the visitor to generate.</param>
        /// <param name="type">The type to return from the visitor calls.</param>
        public VisitorAttribute(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
