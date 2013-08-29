﻿namespace NuPattern.Schema
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IComponentSchema : IInstanceSchema
    {
        /// <summary>
        /// Schema identifier, used on elements created 
        /// based on this schema.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Default instance name for components of this kind.
        /// </summary>
        string DefaultName { get; }

        /// <summary>
        /// Gets a value indicating whether component instances
        /// created from this schema can be named or renamed by 
        /// the user.
        /// </summary>
        bool CanRename { get; }

        IEnumerable<IPropertySchema> Properties { get; }

        // string Icon { get; set; }
    }
}