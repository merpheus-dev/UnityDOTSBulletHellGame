namespace Unity.Properties
{
    public interface ICollectionElementGetter<TContainer>
    {
        void VisitProperty<TElementProperty, TElement>(TElementProperty property, ref TContainer container)
            where TElementProperty : ICollectionElementProperty<TContainer, TElement>;

        void VisitCollectionProperty<TElementProperty, TElement>(TElementProperty property, ref TContainer container)
            where TElementProperty : ICollectionProperty<TContainer, TElement>, ICollectionElementProperty<TContainer, TElement>;
    }

    public interface ICollectionElementProperty : IProperty
    {
        int Index { get; }
    }

    public interface ICollectionElementProperty<TContainer, TElement> : IProperty<TContainer, TElement>, ICollectionElementProperty
    {
    }

    public interface ICollectionProperty<TContainer, TValue> : IProperty<TContainer, TValue>
    {
        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        int GetCount(ref TContainer container);

        /// <summary>
        /// Sets the number of elements contained in the collection.
        /// </summary>
        void SetCount(ref TContainer container, int count);

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        void Clear(ref TContainer container);

        /// <summary>
        /// Gets the strongly typed element at the specified index.
        /// </summary>
        void GetPropertyAtIndex<TGetter>(ref TContainer container, int index, ref ChangeTracker changeTracker, TGetter getter)
            where TGetter : ICollectionElementGetter<TContainer>;
    }
}
