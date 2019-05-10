namespace Unity.Properties
{
    public struct Property<TContainer, TValue> : IProperty<TContainer, TValue>
    {
        public delegate TValue Getter(ref TContainer container);
        public delegate void Setter(ref TContainer container, TValue value);

        private readonly string m_Name;
        private readonly Getter m_Getter;
        private readonly Setter m_Setter;
        private readonly IPropertyAttributeCollection m_Attributes;

        public string GetName() => m_Name;
        public bool IsReadOnly => null == m_Setter;
        public bool IsContainer => RuntimeTypeInfoCache<TValue>.IsContainerType();
        public IPropertyAttributeCollection Attributes => m_Attributes;

        public Property(string name, Getter getter, Setter setter = null, IPropertyAttributeCollection attributes = null)
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
