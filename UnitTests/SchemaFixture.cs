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
            var schema = new ProductSchema
            {
                Components =
                {
                    new ElementSchema 
                    { 
                        Name = "Foo", 
                        Properties = 
                        {
                            new PropertySchema { Name = "Id", Type = typeof(int) },
                            new PropertySchema { Name = "Text", Type = typeof(string) },                            
                        }
                    },
                    new CollectionSchema 
                    {
                        Name = "Bars",
                        Components = 
                        {
                            new ElementSchema 
                            { 
                                Name = "Bar", 
                                Properties = 
                                {
                                    new PropertySchema { Name = "Id", Type = typeof(int) },
                                    new PropertySchema { Name = "Text", Type = typeof(string) },                            
                                }
                            },
                        }
                    },
                }
            };

            Assert.Null(schema.Parent);
            Assert.Same(schema, schema.Root);
            Assert.Same(schema, schema.Components.First().Root);
            Assert.Same(schema, schema.Components.OfType<ICollectionSchema>().First().Components.First().Root);
        }

        [Fact]
        public void when_building_schema_hierarchy_then_sets_proper_parents()
        {
            var schema = new ProductSchema
            {
                Components =
                {
                    new ElementSchema 
                    { 
                        Name = "Foo", 
                        Properties = 
                        {
                            new PropertySchema { Name = "Id", Type = typeof(int) },
                            new PropertySchema { Name = "Text", Type = typeof(string) },                            
                        }
                    },
                    new CollectionSchema 
                    {
                        Name = "Bars",
                        Components = 
                        {
                            new ElementSchema 
                            { 
                                Name = "Bar", 
                                Properties = 
                                {
                                    new PropertySchema { Name = "Id", Type = typeof(int) },
                                    new PropertySchema { Name = "Text", Type = typeof(string) },                            
                                }
                            },
                        }
                    },
                }
            };

            Assert.Null(schema.Parent);

            var toolkit = new ToolkitSchema { Products = { schema } };

            Assert.NotNull(schema.Toolkit);
            Assert.Same(toolkit, schema.Toolkit);
            Assert.Equal(1, toolkit.Products.Count);
            Assert.Equal(1, ((IToolkitSchema)toolkit).Products.Count());

            var foo = (IElementSchema)schema.Components.First();
            Assert.Same(schema, foo.Parent);
            Assert.True(foo.Properties.All(p => p.Parent == foo));

            var bars = (ICollectionSchema)schema.Components.Skip(1).First();
            Assert.Same(schema, bars.Parent);
            Assert.True(bars.Components.All(c => c.Parent == bars));

            var bar = bars.Components.First();
            Assert.True(bar.Properties.All(c => c.Parent == bar));
        }

        [Fact]
        public void when_removing_pattern_from_toolkit_resets_its_toolkit_property()
        {
            var schema = new ProductSchema();
            var toolkit = new ToolkitSchema { Products = { schema } };

            Assert.NotNull(schema.Toolkit);
            Assert.Same(toolkit, schema.Toolkit);

            toolkit.Products.Remove(schema);

            Assert.Null(schema.Parent);
        }

        [Fact]
        public void when_removing_element_from_pattern_resets_its_parent_property()
        {
            var schema = new ProductSchema();
            var element = new ElementSchema();

            schema.Components.Add(element);

            Assert.NotNull(element.Parent);
            Assert.Same(schema, element.Parent);

            schema.Components.Remove(element);

            Assert.Null(element.Parent);
        }

        [Fact]
        public void when_removing_collection_from_pattern_resets_its_parent_property()
        {
            var schema = new ProductSchema();
            var collection = new CollectionSchema();

            schema.Components.Add(collection);

            Assert.NotNull(collection.Parent);
            Assert.Same(schema, collection.Parent);

            schema.Components.Remove(collection);

            Assert.Null(collection.Parent);
        }

        [Fact]
        public void when_removing_element_from_element_resets_its_parent_property()
        {
            var parent = new ElementSchema();
            var child = new ElementSchema();

            parent.Components.Add(child);

            Assert.NotNull(child.Parent);
            Assert.Same(parent, child.Parent);

            parent.Components.Remove(child);

            Assert.Null(child.Parent);
        }

        [Fact]
        public void when_removing_element_from_collection_resets_its_parent_property()
        {
            var parent = new CollectionSchema();
            var child = new ElementSchema();

            parent.Components.Add(child);

            Assert.NotNull(child.Parent);
            Assert.Same(parent, child.Parent);

            parent.Components.Remove(child);

            Assert.Null(child.Parent);
        }

        [Fact]
        public void when_removing_collection_from_element_resets_its_parent_property()
        {
            var parent = new ElementSchema();
            var child = new CollectionSchema();

            parent.Components.Add(child);

            Assert.NotNull(child.Parent);
            Assert.Same(parent, child.Parent);

            parent.Components.Remove(child);

            Assert.Null(child.Parent);
        }

        [Fact]
        public void when_removing_collection_from_collection_resets_its_parent_property()
        {
            var parent = new CollectionSchema();
            var child = new CollectionSchema();

            parent.Components.Add(child);

            Assert.NotNull(child.Parent);
            Assert.Same(parent, child.Parent);

            parent.Components.Remove(child);

            Assert.Null(child.Parent);
        }

        [Fact]
        public void when_removing_property_from_component_resets_its_parent_property()
        {
            var parent = new ElementSchema();
            var property = new PropertySchema();

            parent.Properties.Add(property);

            Assert.NotNull(property.Parent);
            Assert.Same(parent, property.Parent);

            parent.Properties.Remove(property);

            Assert.Null(property.Parent);
        }
    }
}