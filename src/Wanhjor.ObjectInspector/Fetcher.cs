﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
// ReSharper disable MemberCanBeProtected.Global

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Fetcher base class
    /// </summary>
    public class Fetcher
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Fetcher type
        /// </summary>
        public FetcherType Type { get; protected set; }

        /// <summary>
        /// .ctor
        /// </summary>
        internal Fetcher(string name)
        {
            Name = name;
            Type = FetcherType.None;
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        public virtual object? Fetch(object? obj) => throw new NotImplementedException();

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        public virtual void Shove(object? obj, object? value) => throw new NotImplementedException();
        
        /// <summary>
        /// Invokes the method
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        public virtual object? Invoke(object? obj, params object[] parameters) => throw new NotImplementedException();
    }
}
