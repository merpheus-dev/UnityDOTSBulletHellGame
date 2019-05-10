namespace Unity.Properties
{
    public interface IContainerTypeCallback
    {
        void Invoke<T>();
    }

    public interface IPropertyBag
    {
        void Accept<TVisitor>(object container, TVisitor visitor, ref ChangeTracker changeTracker)
            where TVisitor : IPropertyVisitor;

        void Cast<TCallback>(TCallback callback)
            where TCallback : IContainerTypeCallback;
    }

    public interface IPropertyBag<TContainer> : IPropertyBag
    {
        void Accept<TVisitor>(ref TContainer container, TVisitor visitor, ref ChangeTracker changeTracker)
            where TVisitor : IPropertyVisitor;

        bool FindProperty<TAction>(string name, ref TContainer container, ref ChangeTracker changeTracker, ref TAction action)
            where TAction : IPropertyQuery<TContainer>;
    }

    public abstract class PropertyBag<TContainer> : IPropertyBag<TContainer>
    {
        public void Accept<TVisitor>(object container, TVisitor visitor, ref ChangeTracker changeTracker)
            where TVisitor : IPropertyVisitor
        {
            var typed = (TContainer) container;
            Accept(ref typed, visitor, ref changeTracker);
        }

        public void Cast<TCallback>(TCallback callback) where TCallback : IContainerTypeCallback
        {
            callback.Invoke<TContainer>();
        }

        public abstract void Accept<TVisitor>(ref TContainer container, TVisitor visitor, ref ChangeTracker changeTracker) where TVisitor : IPropertyVisitor;
        public abstract bool FindProperty<TAction>(string name, ref TContainer container, ref ChangeTracker changeTracker, ref TAction action) where TAction : IPropertyQuery<TContainer>;
    }
}
