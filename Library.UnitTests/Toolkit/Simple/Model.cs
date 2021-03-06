﻿namespace NuPattern.Tookit.Simple
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Product
    public interface IAmazonWebServices
    {
        // Primitive properties
        string AccessKey { get; set; }
        string SecretKey { get; set; }
        
        string Name { get; set; }

        // Implicit Element property. 1..1, default auto-create likely
        IStorage Storage { get; set; }
    }

    // Element
    public interface IStorage
    {
        // Element primitive property
        bool RefreshOnLoad { get; set; }

        // Implicit Collection property. 0..n, no default auto-create likely
        // IBucket (T) implicit Element.
        IEnumerable<IBucket> Buckets { get; }
    }

    // Element
    public interface IBucket
    {
        // Element primitive properties
        Permissions Permissions { get; set; }
    }

    public enum Permissions
    {
        ReadOnly,
        ReadWrite,
    }
}