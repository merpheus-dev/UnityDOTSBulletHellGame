using NUnit.Framework;
using UnityEngine;

namespace Unity.Properties.Tests
{
    [TestFixture]
    internal class PropertyVisitorTests
    {
        private class DebugLogVisitor : PropertyVisitor
        {
            protected override VisitStatus Visit<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
            {
                Debug.Log($"Visit PropertyType=[{typeof(TProperty)}] PropertyName=[{property.GetName()}]");
                return VisitStatus.Handled;
            }

            protected override VisitStatus BeginContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
            {
                Debug.Log($"BeginContainer PropertyType=[{typeof(TProperty)}] PropertyName=[{property.GetName()}]");
                return VisitStatus.Handled;
            }

            protected override void EndContainer<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
            {
                Debug.Log($"EndContainer PropertyType=[{typeof(TProperty)}] PropertyName=[{property.GetName()}]");
            }

            protected override VisitStatus BeginCollection<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
            {
                Debug.Log($"BeginCollection PropertyType=[{typeof(TProperty)}] PropertyName=[{property.GetName()}]");
                return VisitStatus.Handled;
            }

            protected override void EndCollection<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value, ref ChangeTracker changeTracker)
            {
                Debug.Log($"EndCollection PropertyType=[{typeof(TProperty)}] PropertyName=[{property.GetName()}]");
            }
        }

        [SetUp]
        public void SetUp()
        {
            PropertyBagResolver.Register(new TestNestedContainerPropertyBag());
            PropertyBagResolver.Register(new TestPrimitiveContainerPropertyBag());
            PropertyBagResolver.Register(new TestArrayContainerPropertyBag());
        }

        [Test]
        public void PropertyVisitor_Visit_Struct()
        {
            var container = new TestPrimitiveContainer();
            PropertyContainer.Visit(ref container, new DebugLogVisitor());
        }

        [Test]
        public void PropertyVisitor_Visit_StructWithNestedStruct()
        {
            var container = new TestNestedContainer();
            PropertyContainer.Visit(ref container, new DebugLogVisitor());
        }

        [Test]
        public void PropertyVisitor_Visit_StructWithArray()
        {
            var container = new TestArrayContainer
            {
                Int32Array = new [] { 1, 2, 3 },
                TestContainerArray = new [] { new TestPrimitiveContainer(), new TestPrimitiveContainer() }
            };

            PropertyContainer.Visit(ref container, new DebugLogVisitor());
        }
    }
}
