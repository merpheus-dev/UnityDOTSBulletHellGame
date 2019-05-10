using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unity.Properties.Reflection
{
    public class ReflectedPropertyBagProvider : IPropertyBagProvider
    {
        private readonly MethodInfo m_GenerateMethod;
        private readonly MethodInfo m_CreatePropertyMethod;
        private readonly List<IReflectedPropertyGenerator> m_Generators;

        public ReflectedPropertyBagProvider()
        {
            m_GenerateMethod = typeof(ReflectedPropertyBagProvider).GetMethods().First(x => x.Name == nameof(Generate) && x.IsGenericMethod);
            m_CreatePropertyMethod = typeof(ReflectedPropertyBagProvider).GetMethod("CreateProperty", BindingFlags.Instance | BindingFlags.NonPublic);
            m_Generators = new List<IReflectedPropertyGenerator>();

            // Register default generators.
            AddGenerator(new ReflectedFieldPropertyGenerator()); // baseline FieldInfo property
            AddGenerator(new UnmanagedPropertyGenerator()); // unmanaged offset based property
        }

        public void AddGenerator(IReflectedPropertyGenerator generator)
        {
            m_Generators.Add(generator);
        }

        public IPropertyBag<TContainer> Generate<TContainer>()
        {
            if (typeof(TContainer).IsEnum)
            {
                return null;
            }

            var propertyBag = new ReflectedPropertyBag<TContainer>();
            var fields = typeof(TContainer).GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var method = m_CreatePropertyMethod.MakeGenericMethod(typeof(TContainer), field.FieldType);
                method.Invoke(this, new object[] {field, propertyBag});
            }

            return propertyBag;
        }

        public IPropertyBag Generate(Type type)
        {
            var method = m_GenerateMethod.MakeGenericMethod(type);
            return (IPropertyBag) method.Invoke(this, null);
        }

        // ReSharper disable once UnusedMember.Local
        private void CreateProperty<TContainer, TValue>(FieldInfo field, ReflectedPropertyBag<TContainer> propertyBag)
        {
            for (var index = m_Generators.Count - 1; index >= 0; index--)
            {
                var generator = m_Generators[index];

                if (!generator.Generate<TContainer, TValue>(field, propertyBag))
                {
                    continue;
                }

                break;
            }
        }
    }
}
