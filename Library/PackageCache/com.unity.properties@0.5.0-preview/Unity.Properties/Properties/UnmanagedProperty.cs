using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Properties
{
    public interface IUnmanagedProperty : IProperty
    {
        int Offset { get; }
    }

    public struct UnmanagedProperty<TContainer, TValue> : IUnmanagedProperty, IProperty<TContainer, TValue>
        where TContainer : struct
        where TValue : unmanaged
    {
        private readonly string m_Name;
        private readonly int m_Offset;
        private readonly IPropertyAttributeCollection m_Attributes;

        public string GetName() => m_Name;
        public int Offset => m_Offset;
        public bool IsReadOnly => false;
        public bool IsContainer => RuntimeTypeInfoCache<TValue>.IsContainerType();
        public IPropertyAttributeCollection Attributes => m_Attributes;

        public UnmanagedProperty(string name, int offset, IPropertyAttributeCollection attributes = null)
        {
            m_Name = name;
            m_Offset = offset;
            m_Attributes = attributes;
        }

        public unsafe TValue GetValue(ref TContainer container)
            => *(TValue*) ((byte*) UnsafeUtility.AddressOf(ref container) + m_Offset);

        public unsafe void SetValue(ref TContainer container, TValue value)
            => *(TValue*) ((byte*) UnsafeUtility.AddressOf(ref container) + m_Offset) = value;
    }
}
