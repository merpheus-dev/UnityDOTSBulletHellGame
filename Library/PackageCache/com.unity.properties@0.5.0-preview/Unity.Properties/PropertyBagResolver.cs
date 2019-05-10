using System;
using System.Collections.Generic;

namespace Unity.Properties
{
    public interface IPropertyBagProvider
    {
        IPropertyBag<TContainer> Generate<TContainer>();
        IPropertyBag Generate(Type type);
    }

    public static class PropertyBagResolver
    {
        /// <summary>
        /// Static <see cref="IPropertyBag"/> lookup for strongly typed containers.
        /// </summary>
        /// <typeparam name="TContainer">The host container type.</typeparam>
        private struct Lookup<TContainer>
        {
            public static IPropertyBag<TContainer> PropertyBag;
        }

        /// <summary>
        /// Dynamic lookup by <see cref="System.Type"/> for property bags.
        /// </summary>
        private static readonly Dictionary<Type, IPropertyBag> s_PropertyBagByType = new Dictionary<Type, IPropertyBag>();

        private static readonly IList<IPropertyBagProvider> s_PropertyBagProviders = new List<IPropertyBagProvider>();

        public static void Register<TContainer>(IPropertyBag<TContainer> propertyBag)
        {
            Lookup<TContainer>.PropertyBag = propertyBag;
            s_PropertyBagByType[typeof(TContainer)] = propertyBag;
        }

        public static IPropertyBag<TContainer> Resolve<TContainer>()
        {
            var propertyBag = Lookup<TContainer>.PropertyBag;

            if (null != propertyBag)
            {
                return propertyBag;
            }

            s_PropertyBagByType.TryGetValue(typeof(TContainer), out var untypedPropertyBag);

            if (null != untypedPropertyBag)
            {
                return (IPropertyBag<TContainer>) untypedPropertyBag;
            }

            if (TryGeneratePropertyBag(out propertyBag))
            {
                Register(propertyBag);
            }

            return propertyBag;
        }

        public static IPropertyBag Resolve(Type type)
        {
            s_PropertyBagByType.TryGetValue(type, out var propertyBag);

            if (null == propertyBag)
            {
                if (TryGeneratePropertyBag(type, out propertyBag))
                {
                    s_PropertyBagByType.Add(type, propertyBag);
                }
            }

            return propertyBag;
        }

        public static void RegisterProvider(IPropertyBagProvider provider)
        {
            s_PropertyBagProviders.Add(provider);
        }

        private static bool TryGeneratePropertyBag<TContainer>(out IPropertyBag<TContainer> propertyBag)
        {
            for (var i = 0; i < s_PropertyBagProviders.Count; i++)
            {
                var provider = s_PropertyBagProviders[i];
                propertyBag = provider.Generate<TContainer>();

                if (null != propertyBag)
                {
                    return true;
                }
            }

            propertyBag = null;
            return false;
        }

        private static bool TryGeneratePropertyBag(Type type, out IPropertyBag propertyBag)
        {
            foreach (var provider in s_PropertyBagProviders)
            {
                propertyBag = provider.Generate(type);

                if (null != propertyBag)
                {
                    return true;
                }
            }

            propertyBag = null;
            return false;
        }
    }
}
