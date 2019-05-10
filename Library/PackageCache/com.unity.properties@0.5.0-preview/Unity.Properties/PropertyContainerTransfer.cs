namespace Unity.Properties
{
    public static partial class PropertyContainer
    {
        private struct TransferAbstractType<TDestination> : IContainerTypeCallback
        {
            public TDestination Destination;
            public object SourceContainer;

            public void Invoke<T>()
            {
                Visit(ref Destination, new TransferVisitor<T>((T) SourceContainer));
            }
        }

        public static void Transfer<TDestination, TSource>(TDestination destination, TSource source, IVersionStorage versionStorage = null)
        {
            var changeTracker = new ChangeTracker(versionStorage);
            Transfer(ref destination, ref source, ref changeTracker);
        }

        public static void Transfer<TDestination, TSource>(ref TDestination destination, ref TSource source, IVersionStorage versionStorage = null)
        {
            var changeTracker = new ChangeTracker(versionStorage);
            Transfer(ref destination, ref source, ref changeTracker);
        }

        public static void Transfer<TDestination, TSource>(ref TDestination destination, ref TSource source, ref ChangeTracker changeTracker)
        {
            if (RuntimeTypeInfoCache<TSource>.IsAbstractOrInterface())
            {
                var propertyBag = PropertyBagResolver.Resolve(source.GetType());
                var action = new TransferAbstractType<TDestination>
                {
                    Destination = destination,
                    SourceContainer = (object) source
                };
                propertyBag.Cast(action);
                destination = action.Destination;
            }
            else
            {
                Visit(ref destination, new TransferVisitor<TSource>(source), ref changeTracker);
            }
        }
    }
}
