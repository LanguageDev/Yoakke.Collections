﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Symbols
{
    /// <summary>
    /// Represents a single symbol.
    /// </summary>
    public interface ISymbol
    {
        /// <summary>
        /// The scope that contains this symbol.
        /// </summary>
        public IReadOnlyScope Scope { get; }
        /// <summary>
        /// The name of this symbol.
        /// </summary>
        public string Name { get; }
    }
}
