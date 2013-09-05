﻿namespace NuPattern
{
    using NuPattern.Schema;
    using System;
    using System.Linq;
    using Xunit;

    public class SchemaFixture
    {
        [Fact]
        public void when_building_schema_hierarchy_then_can_retrieve_root_schema()
        {
            var schema = new ProductSchema("Product")
            {
                ComponentSchemas =
                {
                    new ElementSchema("Element"),
                    new CollectionSchema("Collection")
                    {
                        ComponentSchemas = 
                        {
                            new ElementSchema("Element")
                        }
                    },
                }
            };

            Assert.Null(schema.Parent);
            Assert.Same(schema, schema.Root);
            Assert.Same(schema, schema.ComponentSchemas.First().Root);
            Assert.Same(schema, schema.ComponentSchemas.OfType<ICollectionSchema>().First().ComponentSchemas.First().Root);

            schema.ComponentSchemas.OfType<ICollectionSchema>().First().CreateItemSchema("Item");

            Assert.Same(schema, schema.ComponentSchemas.OfType<ICollectionSchema>().First().ItemSchema.Root);
        }

        [Fact]
        public void when_building_schema_hierarchy_then_sets_proper_parents()
        {
            var schema = new ProductSchema("Product")
            {
                ComponentSchemas =
                {
                    new ElementSchema("Element")
                    { 
                        DefaultName = "Foo", 
                        CanRename = false,
                        PropertySchemas = 
                        {
                            new PropertySchema("Id", typeof(string)),
                            new PropertySchema("Text", typeof(string)),
                        }
                    },
                    new CollectionSchema("Collection")
                    {
                        DefaultName = "Bars",
                        CanRename = false,
                        ComponentSchemas = 
                        {
                            new ElementSchema("Element")
                            {
                                PropertySchemas = 
                                {
                                    new PropertySchema("Id", typeof(string)),
                                    new PropertySchema("Text", typeof(string)),
                                }
                            }
                        }
                    },
                }
            };

            Assert.Null(schema.Parent);

            var toolkit = new ToolkitSchema("FooToolkit", "1.0") { ProductSchemas = { schema } };

            Assert.NotNull(schema.ToolkitSchema);
            Assert.Same(toolkit, schema.ToolkitSchema);
            Assert.Equal(1, toolkit.ProductSchemas.Count);
            Assert.Equal(1, ((IToolkitSchema)toolkit).ProductSchemas.Count());

            var foo = (IElementSchema)schema.ComponentSchemas.First();
            Assert.Same(schema, foo.Parent);
            Assert.True(foo.PropertySchemas.All(p => p.Parent == foo));

            var bars = (ICollectionSchema)schema.ComponentSchemas.Skip(1).First();
            Assert.Same(schema, bars.Parent);
            Assert.True(bars.ComponentSchemas.All(c => c.Parent == bars));

            var bar = bars.ComponentSchemas.First();
            Assert.True(bar.PropertySchemas.All(c => c.Parent == bar));
        }

        [Fact]
        public void when_creating_name_property_then_throws_because_its_reserved()
        {
            var toolkit = new ToolkitSchema("FooToolkit", "1.0");

            Assert.Throws<ArgumentException>(() =>
                toolkit.CreateProductSchema("IFoo").CreatePropertySchema("Name", typeof(string)));
        }

        [Fact]
        public void when_creating_duplicate_named_property_then_throws()
        {
            var toolkit = new ToolkitSchema("FooToolkit", "1.0");
            var product = toolkit.CreateProductSchema("IFoo");

            product.CreatePropertySchema("IsVisible", typeof(bool));

            Assert.Throws<ArgumentException>(() => product.CreatePropertySchema("IsVisible", typeof(bool)));
        }
    }
}