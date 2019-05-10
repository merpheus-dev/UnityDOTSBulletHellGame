using NUnit.Framework;
using UnityEngine;

namespace Unity.Properties.Reflection.Tests
{
    [TestFixture]
    internal class ReflectedPropertyBagVisitorTests
    {
        private struct SimpleContainer
        {
#pragma warning disable 649
            public int Int32Value;
            public float Float32Value;
            public string StringValue;
            public byte UInt8Value;
            public ushort Int16Value;
            public NestedContainer Nested;
#pragma warning restore 649
        }

        private struct Foo
        {
#pragma warning disable 649
            public NestedContainer Nested;
#pragma warning restore 649
        }

        private struct NestedContainer
        {
#pragma warning disable 649
            public int Int32Value;
            public int Foo;
            public byte UInt8Value;
            public ushort Int16Value;
#pragma warning restore 649
        }

        private class VoidVisitor : PropertyVisitor
        {
            protected override VisitStatus Visit<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
            {
                return VisitStatus.Handled;
            }
        }

        [Test]
        public void ReflectedPropertyBagVisitor_Visit()
        {
            PropertyBagResolver.Register(new ReflectedPropertyBagProvider().Generate<SimpleContainer>());
            PropertyBagResolver.Register(new ReflectedPropertyBagProvider().Generate<NestedContainer>());

            PropertyContainer.Visit(new SimpleContainer(), new VoidVisitor());
        }

        [Test]
        public void ReflectedPropertyBagVisitor_Transfer_NestedContainer()
        {
            PropertyBagResolver.Register(new ReflectedPropertyBagProvider().Generate<SimpleContainer>());
            PropertyBagResolver.Register(new ReflectedPropertyBagProvider().Generate<NestedContainer>());
            PropertyBagResolver.Register(new ReflectedPropertyBagProvider().Generate<Foo>());

            var source = new SimpleContainer
            {
                Int32Value = 15,
                Nested = new NestedContainer
                {
                    Int32Value = 42
                }
            };

            var foo = new Foo
            {
                Nested = new NestedContainer {Int32Value = 10}
            };

            var changeTracker = new ChangeTracker(null);
            PropertyContainer.Transfer(ref foo, ref source, ref changeTracker);
            Assert.AreEqual(42, foo.Nested.Int32Value);
        }
    }
}
