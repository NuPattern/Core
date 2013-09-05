﻿using System.Reactive.Linq;
namespace NuPattern
{
    using NuPattern.Schema;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class ProductStore : IProductStore, IDisposable
    {
        private ProductStoreSettings settings;
        private IProductSerializer serializer;
        private Dictionary<string, IToolkitSchema> toolkits;
        private List<Product> products = new List<Product>();

        public ProductStore(
            ProductStoreSettings settings,
            IProductSerializer serializer,
            IEnumerable<IToolkitBuilder> builders)
        {
            Guard.NotNull(() => settings, settings);
            Guard.NotNull(() => serializer, serializer);
            Guard.NotNull(() => builders, builders);

            this.settings = settings;
            this.serializer = serializer;
            this.toolkits = builders.Select(x => x.Build()).ToDictionary(x => x.Id);
        }

        public bool IsDisposed { get; private set; }
        public string Name { get { return settings.StoreName; } }

        public IEnumerable<IProduct> Products { get { return products.AsReadOnly(); } }

        public IProduct Create(string name, string toolkitId, string schemaId)
        {
            Guard.NotNullOrEmpty(() => toolkitId, toolkitId);
            Guard.NotNullOrEmpty(() => schemaId, schemaId);

            if (products.Any(x => x.Name == name))
                throw new ArgumentException();

            var toolkit = toolkits.Find(toolkitId);
            if (toolkit == null)
                throw new ArgumentException();

            var schema = toolkit.ProductSchemas.FirstOrDefault(x => x.SchemaId == schemaId);
            if (schema == null)
                throw new ArgumentException();

            var product = new Product(name, schemaId);
            SchemaMapper.SyncProduct(product, schema);
            products.Add(product);

            return product;
        }

        public void Dispose()
        {
            Clear();
            IsDisposed = true;
        }

        public void Load(IProgress<int> progress)
        {
            Guard.NotNull(() => progress, progress);

            Clear();
            using (var reader = File.OpenText(settings.StateFile))
            {
                var current = 0;
                foreach (var product in serializer.Deserialize(reader))
                {
                    var toolkit = toolkits.Find(product.Toolkit.Id);
                    if (toolkit != null)
                    {
                        if (product.Toolkit.Version < toolkit.Version)
                        {
                            // TODO: implement some version migration if 
                            // provided by the toolkit schema?
                        }

                        var schema = toolkit.ProductSchemas.FirstOrDefault(x => x.SchemaId == product.SchemaId);
                        if (schema == null)
                        {
                            // TODO: provide some "SchemaMissing" callback 
                            // on the toolkit? Maybe it was just a type rename?
                        }
                        else
                        {
                            SchemaMapper.SyncProduct(product, schema);
                        }
                    }

                    progress.Report(++current);
                    product.Deleted += OnProductDeleted;
                    products.Add(product);
                }
            }
        }

        public void Save(IProgress<int> progress)
        {
            Guard.NotNull(() => progress, progress);

            using (var writer = new StreamWriter(settings.StateFile, false))
            {
                serializer.Serialize(writer, Report(products, progress));
            }
        }

        private void OnProductDeleted(object sender, EventArgs args)
        {
            var product = (Product)sender;
            products.Remove(product);
            product.Deleted -= OnProductDeleted;
        }

        private void Clear()
        {
            foreach (var product in products.ToArray())
            {
                // Avoids the event calling back for each product.
                product.Deleted -= OnProductDeleted;
                product.Dispose();
            }

            // Remove all on one call.
            products.Clear();
        }

        private IEnumerable<Product> Report(IEnumerable<Product> products, IProgress<int> progress)
        {
            var current = 0;
            foreach (var product in products)
            {
                yield return product;
                progress.Report(++current);
            }
        }
    }
}