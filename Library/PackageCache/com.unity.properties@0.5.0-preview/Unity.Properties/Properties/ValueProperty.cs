namespace Unity.Properties
{
    public struct ValueProperty<TContainer, TValue> : IProperty<TContainer, TValue>
        where TValue : new()
    {
        public delegate TValue Getter(ref TContainer container);
        public delegate void Setter(ref TContainer container, TValue value);

        private readonly string m_Name;
        private readonly Getter m_Getter;
        private readonly Setter m_Setter;
        private readonly IPropertyAttributeCollection m_Attributes;

        public string GetName() => m_Name;
        public bool IsReadOnly => false;
        public bool IsContainer => RuntimeTypeInfoCache<TValue>.IsContainerType();
        public IPropertyAttributeCollection Attributes => m_Attributes;

        public ValueProperty(string name, Getter getter, Setter setter, IPropertyAttributeCollection attributes = null)
        {
            m_Name = name;
            m_Getter = getter;
            m_Setter = setter;
            m_Attributes = attributes;
        }

        public TValue GetValue(ref TContainer container)
        {
            return m_Getter(ref container);
        }

        public void SetValue(ref TContainer container, TValue value)
        {
            m_Setter(ref container, value);
        }
    }
}
