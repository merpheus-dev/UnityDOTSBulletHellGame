namespace Unity.Properties
{
    /// <summary>
    /// Base interface to visit a property bag.
    /// </summary>
    public interface IPropertyVisitor
    {
        VisitStatus VisitProperty<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref ChangeTracker propertyChangeTracker)
            where TProperty : IProperty<TContainer, TValue>;

        VisitStatus VisitCollectionProperty<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref ChangeTracker changeTracker)
            where TProperty : ICollectionProperty<TContainer, TValue>;
    }

    /// <summary>
    /// Base interface for querying a property from a bag.
    /// </summary>
    public interface IPropertyQuery<TContainer>
    {
        void VisitProperty<TProperty, TValue>(TProperty property, ref TContainer container, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, TValue>;

        void VisitCollectionProperty<TProperty, TValue>(TProperty property, ref TContainer container, ref ChangeTracker changeTracker)
            where TProperty : ICollectionProperty<TContainer, TValue>;
    }
}
