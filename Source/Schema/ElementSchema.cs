﻿namespace NuPattern.Schema
{
    using System;

    internal class ElementSchema : ContainerSchema, IElementSchema
    {
        public ElementSchema(string id)
            : base(id)
        {
        }
    }
}