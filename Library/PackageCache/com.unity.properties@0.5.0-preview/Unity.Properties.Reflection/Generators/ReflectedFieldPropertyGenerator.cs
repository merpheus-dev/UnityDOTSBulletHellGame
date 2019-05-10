using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unity.Properties.Reflection
{
    public class ReflectedFieldPropertyGenerator : IReflectedPropertyGenerator
    {
        private struct ReflectedFieldProperty<TContainer, TValue> : IProperty<TContainer, TValue>
        {
            private readonly FieldInfo m_FieldInfo;
            private readonly IPropertyAttributeCollection m_Attributes;

            public string GetName() => m_FieldInfo.Name;
            public bool IsReadOnly => false;
            public bool IsContainer => !(m_FieldInfo.FieldType.IsPrimitive || m_FieldInfo.FieldType.IsEnum || m_FieldInfo.FieldType == typeof(string));
            public IPropertyAttributeCollection Attributes => m_Attributes;

            public ReflectedFieldProperty(FieldInfo fieldInfo)
            {
                m_FieldInfo = fieldInfo;
                m_Attributes = new PropertyAttributeCollection(fieldInfo.GetCustomAttributes().ToArray());
            }

            public TValue GetValue(ref TContainer container)
            {
                return (TValue) m_FieldInfo.GetValue(container);
            }

            public void SetValue(ref TContainer container, TValue value)
            {
                var boxed = (object) container;
                m_FieldInfo.SetValue(boxed, value);
                container = (TContainer) boxed;
            }
        }

        private struct ReflectedListProperty<TContainer, TValue, TElement> : ICollectionProperty<TContainer, TValue>
            where TValue : IList<TElement>
        {
            private struct CollectionElementProperty : ICollectionElementProperty<TContainer, TElement>
            {
                private readonly ReflectedListProperty<TContainer, TValue, TElement> m_Property;
                private readonly IPropertyAttributeCollection m_Attributes;
                private readonly int m_Index;

                public string GetName() => "[" + Index + "]";
                public bool IsReadOnly => false;
                public bool IsContainer => RuntimeTypeInfoCache<TElement>.IsContainerType();
                public IPropertyAttributeCollection Attributes => m_Attributes;
                public int Index => m_Index;

                public CollectionElementProperty(ReflectedListProperty<TContainer, TValue, TElement> property, int index, IPropertyAttributeCollection attributes = null)
                {
                    m_Property = property;
                    m_Attributes = attributes;
                    m_Index = index;
                }

                public TElement GetValue(ref TContainer container)
                {
                    return m_Property.GetValue(ref container)[Index];
                }

                public void SetValue(ref TContainer container, TElement value)
                {
                    m_Property.GetValue(ref container)[Index] = value;
                }
            }

            private readonly FieldInfo m_FieldInfo;
            private readonly IPropertyAttributeCollection m_Attributes;

            public string GetName() => m_FieldInfo.Name;
            public bool IsReadOnly => false;
            public bool IsContainer => !(m_FieldInfo.FieldType.IsPrimitive || m_FieldInfo.FieldType.IsEnum || m_FieldInfo.FieldType == typeof(string));
            public IPropertyAttributeCollection Attributes => m_Attributes;

            public ReflectedListProperty(FieldInfo fieldInfo)
            {
                m_FieldInfo = fieldInfo;
                m_Attributes = new PropertyAttributeCollection(fieldInfo.GetCustomAttributes().ToArray());
            }

            public TValue GetValue(ref TContainer container)
            {
                return (TValue) m_FieldInfo.GetValue(container);
            }

            public void SetValue(ref TContainer container, TValue value)
            {
                m_FieldInfo.SetValue(container, value);
            }

            public int GetCount(ref TContainer container)
            {
                return GetValue(ref container).Count;
            }

            public void SetCount(ref TContainer container, int count)
            {
                var list = GetValue(ref container);

                if (list.Count == count)
                {
                    return;
                }

                if (list.Count < count)
                {
                    for (var i = list.Count; i < count; i++)
                        list.Add(default(TElement));
                }
                else
                {
                    for (var i = list.Count - 1; i >= count; i--)
                        list.RemoveAt(i);
                }
            }

            public void Clear(ref TContainer container)
            {
                GetValue(ref container).Clear();
            }

            public void GetPropertyAtIndex<TGetter>(ref TContainer container, int index, ref ChangeTracker changeTracker, TGetter getter) where TGetter : ICollectionElementGetter<TContainer>
            {
                getter.VisitProperty<CollectionElementProperty, TElement>(new CollectionElementProperty(this, index), ref container);
            }
        }

        public bool Generate<TContainer, TValue>(FieldInfo field, ReflectedPropertyBag<TContainer> propertyBag)
        {
            if (typeof(IList).IsAssignableFrom(typeof(TValue)))
            {
                var elementType = typeof(TValue).GetGenericArguments()[0];
                var method = typeof(ReflectedFieldPropertyGenerator).GetMethod(nameof(GenerateListProperty), BindingFlags.Instance | BindingFlags.NonPublic);
                var genericMethod = method.MakeGenericMethod(typeof(TContainer), field.FieldType, elementType);
                genericMethod.Invoke(this, new object[] {field, propertyBag});
            }
            else
            {
                propertyBag.AddProperty<ReflectedFieldProperty<TContainer, TValue>, TValue>(
                    new ReflectedFieldProperty<TContainer, TValue>(field));
            }

            return true;
        }

        private void GenerateListProperty<TContainer, TValue, TElement>(FieldInfo field, ReflectedPropertyBag<TContainer> propertyBag)
            where TValue : IList<TElement>
        {
            propertyBag.AddCollectionProperty<ReflectedListProperty<TContainer, TValue, TElement>, TValue>(
                new ReflectedListProperty<TContainer, TValue, TElement>(field));
        }
    }
}
