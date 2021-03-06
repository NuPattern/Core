﻿namespace NuPattern.Schema
{
    using NetFx.StringlyTyped;
    using NuPattern.Core.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NuPattern;

    public class SchemaBuilder
    {
        public SchemaBuilder()
        {
            this.Map = new SchemaTypeMap();
        }

        public SchemaTypeMap Map { get; private set; }

        public IProductSchema BuildProduct(IToolkitSchema toolkit, Type productType)
        {
            if (!productType.IsInterface)
                throw new ArgumentException(Strings.SchemaBuilder.ModelMustBeInterfaces(productType));

            var schema = toolkit.CreateProductSchema(productType.ToTypeFullName());

            BuildType(productType, schema);

            return schema;
        }

        private void BuildType(Type type, IContainerSchema schema)
        {
            Map.AddSchema(type, schema);

            foreach (var property in type.GetProperties().Where(x => x.PropertyType.IsNative()))
            {
                if (property.Name == "Name")
                {
                    if (property.PropertyType != typeof(string))
                        throw new ArgumentException(Strings.SchemaBuilder.NamePropertyMustBeString);
                    
                    // Skip adding Name as it's built-in.
                    continue;
                }

                schema.CreatePropertySchema(property.Name, property.PropertyType);
            }

            foreach (var property in type.GetProperties().Where(x => !x.PropertyType.IsNative()))
            {
                if (!property.PropertyType.IsInterface)
                    throw new ArgumentException(Strings.SchemaBuilder.ModelMustBeInterfaces(property.PropertyType));

                if (property.PropertyType.IsCollection())
                {
                    var collectionSchema = schema.CreateCollectionSchema(property.PropertyType.ToTypeFullName());
                    collectionSchema.DefaultName = property.Name;
                    var itemType = property.PropertyType.GetItemType();
                    var itemSchema = collectionSchema.CreateItemSchema(itemType.ToTypeFullName());

                    BuildType(itemType, itemSchema);
                    BuildType(property.PropertyType, collectionSchema);
                }
                else
                {
                    var elementSchema = schema.CreateElementSchema(property.PropertyType.ToTypeFullName());
                    elementSchema.DefaultName = property.Name;
                    BuildType(property.PropertyType, elementSchema);
                }
            }
        }
    }
}
