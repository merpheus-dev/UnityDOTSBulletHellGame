using NUnit.Framework;

namespace Unity.Properties.Tests
{
    [TestFixture]
    internal class PropertyContainerTransferTests
    {
        [SetUp]
        public void SetUp()
        {
            PropertyBagResolver.Register(new TestNestedContainerPropertyBag());
            PropertyBagResolver.Register(new TestPrimitiveContainerPropertyBag());
            PropertyBagResolver.Register(new TestArrayContainerPropertyBag());
            PropertyBagResolver.Register(new CustomDataFooPropertyBag());
            PropertyBagResolver.Register(new CustomDataBarPropertyBag());
        }

        [Test]
        public void PropertyContainer_Transfer_Primitive()
        {
            var src = new TestPrimitiveContainer
            {
                Int32Value = 10
            };

            var dst = new TestPrimitiveContainer
            {
                Int32Value = 20
            };

            PropertyContainer.Transfer(ref dst, ref src);

            Assert.AreEqual(10, dst.Int32Value);
        }
    }
}
