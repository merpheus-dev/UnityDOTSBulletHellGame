using System.Reflection;

namespace Unity.Properties.Reflection
{
    public interface IReflectedPropertyGenerator
    {
        bool Generate<TContainer, TValue>(FieldInfo field, ReflectedPropertyBag<TContainer> propertyBag);
    }
}
