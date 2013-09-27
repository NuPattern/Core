﻿namespace NuPattern.Schema
{
    using NuPattern.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;

    internal class ToolkitSchema : IToolkitSchema, IToolkitInfo
    {
        public ToolkitSchema(string toolkitId, string toolkitVersion)
            : this(toolkitId, SemanticVersion.Parse(toolkitVersion))
        {
        }

        public ToolkitSchema(string toolkitId, SemanticVersion toolkitVersion)
        {
            Guard.NotNullOrEmpty(() => toolkitId, toolkitId);
            Guard.NotNull(() => toolkitVersion, toolkitVersion);

            var products = new ObservableCollection<ProductSchema>();
            products.CollectionChanged += OnPatternsChanged;

            this.Id = toolkitId;
            this.Version = toolkitVersion;
            this.Products = products;
        }

        public string Id { get; private set; }
        public SemanticVersion Version { get; private set; }
        public ICollection<ProductSchema> Products { get; private set; }

        public IProductSchema CreateProductSchema(string schemaId)
        {
            var schema  = new ProductSchema(schemaId);
            Products.Add(schema);
            return schema;
        }

        public TVisitor Accept<TVisitor>(TVisitor visitor) where TVisitor : IVisitor
        {
            visitor.Visit<IToolkitSchema>(this);

            foreach (var product in Products)
            {
                product.Accept(visitor);
            }

            return visitor;
        }

        IEnumerable<IProductInfo> IToolkitInfo.Products { get { return Products; } }

        IEnumerable<IProductSchema> IToolkitSchema.Products { get { return Products; } }

        IProductSchema IToolkitSchema.CreateProductSchema(string schemaId)
        {
            return CreateProductSchema(schemaId);
        }

        private void OnPatternsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var product in e.NewItems.OfType<ProductSchema>())
                {
                    product.Toolkit = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var product in e.OldItems.OfType<ProductSchema>())
                {
                    product.Toolkit = null;
                }
            }
        }
    }
}