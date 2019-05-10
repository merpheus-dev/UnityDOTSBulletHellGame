using System;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    internal class PropertyContainerValueTests
    {
        [SetUp]
        public void SetUp()
        {
            PropertyBagResolver.Register(new TestNestedContainerPropertyBag());
            PropertyBagResolver.Register(new TestPrimitiveContainerPropertyBag());
            PropertyBagResolver.Register(new TestArrayContainerPropertyBag());
        }

        [Test]
        public void PropertyContainer_SetValue_Primitives()
        {
            var container = new TestPrimitiveContainer();

            PropertyContainer.SetValue(ref container, nameof(TestPrimitiveContainer.Int32Value), 10);
            PropertyContainer.SetValue(ref container, nameof(TestPrimitiveContainer.Float32Value), 2.5f);

            Assert.AreEqual(10, container.Int32Value);
            Assert.AreEqual(2.5f, container.Float32Value);
        }
        
        [Test]
        public void PropertyContainer_SetValue_NestedContainer()
        {
            var container = new TestNestedContainer();

            PropertyContainer.SetValue(ref container, nameof(TestNestedContainer.TestPrimitiveContainer), new TestPrimitiveContainer { Int32Value = 42 });

            Assert.AreEqual(42, container.TestPrimitiveContainer.Int32Value);
        }
        
        [Test]
        public void PropertyContainer_SetValue_Collection()
        {
            var container = new TestArrayContainer();

            PropertyContainer.SetValue(ref container, nameof(TestArrayContainer.Int32Array), new [] { 4, 5, 6 });

            Assert.AreEqual(3, container.Int32Array.Length);
        }
        
        [Test]
        public void PropertyContainer_SetValue_Conversion_DoesNotThrow()
        {
            var container = new TestPrimitiveContainer();

            Assert.DoesNotThrow(() =>
            {
                PropertyContainer.SetValue(ref container, nameof(TestPrimitiveContainer.Int32Value), 10.5f);
            });

            Assert.AreEqual(10, container.Int32Value);
        }

        private struct NotSupportedType
        {
        }
        
        [Test]
        public void PropertyContainer_SetValue_Conversion_Throws()
        {
            var container = new TestPrimitiveContainer();
            var value = new NotSupportedType();

            Assert.Throws<Exception>(() =>
            {
                PropertyContainer.SetValue(ref container, nameof(TestPrimitiveContainer.Int32Value), value);
            });
        }
        
        [Test]
        public void PropertyContainer_SetValue_InvalidName_Throws()
        {
            var container = new TestPrimitiveContainer();

            Assert.Throws<Exception>(() =>
            {
                PropertyContainer.SetValue(ref container, "test", 10);
            });
        }
        
        [Test]
        [TestCase(0, 1, true)]
        [TestCase(2, 3, true)]
        [TestCase(4, 4, false)]
        [TestCase(0, 0, false)]
        public void PropertyContainer_SetValue_ChangeTracker(int start, int end, bool expected)
        {
            var container = new TestPrimitiveContainer
            {
                Int32Value = start
            };
            
            var changeTracker = new ChangeTracker();
            PropertyContainer.SetValue(ref container, nameof(TestPrimitiveContainer.Int32Value), end, ref changeTracker);
            Assert.AreEqual(expected, changeTracker.IsChanged());
        }
        
        [Test]
        public void PropertyContainer_SetValue_InvalidPropertyBag_Throws()
        { 
            Assert.Throws<Exception>(() =>
            {
                var container = 10;
                PropertyContainer.SetValue(ref container, "test", 10);
            });
        }
        
        [Test]
        public void PropertyContainer_GetValue_Primitives()
        {
            var container = new TestPrimitiveContainer
            {
                Int32Value = 42, 
                Float64Value = 12345.678
            };
            
            Assert.AreEqual(42, PropertyContainer.GetValue<TestPrimitiveContainer, int>(ref container, nameof(TestPrimitiveContainer.Int32Value)));
            Assert.AreEqual(12345.678, PropertyContainer.GetValue<TestPrimitiveContainer, double>(ref container, nameof(TestPrimitiveContainer.Float64Value)));
        }
        
        [Test]
        public void PropertyContainer_GetValue_NestedContainer()
        {
            var container = new TestNestedContainer
            {
                TestPrimitiveContainer = new TestPrimitiveContainer
                {
                    Int32Value = 42
                }
            };

            var value = PropertyContainer.GetValue<TestNestedContainer, TestPrimitiveContainer>(ref container, nameof(TestNestedContainer.TestPrimitiveContainer));
            
            Assert.AreEqual(42, value.Int32Value);
        }
        
        [Test]
        public void PropertyContainer_GetValue_Conversion_DoesNotThrow()
        {
            var container = new TestPrimitiveContainer
            {
                Float64Value = 12345.678
            };
            
            var value = PropertyContainer.GetValue<TestPrimitiveContainer, int>(ref container, nameof(TestPrimitiveContainer.Float64Value));
            
            Assert.AreEqual(12345, value);
        }
    }
}