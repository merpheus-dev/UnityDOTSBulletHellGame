namespace Unity.Properties
{
    /// <summary>
    /// Helper class to avoid paying the cost of runtime type lookups.
    ///
    /// This is also used to abstract underlying type info in the runtime (e.g. RuntimeTypeHandle vs StaticTypeReg)
    /// </summary>
    internal struct RuntimeTypeInfoCache<T>
    {
        private static readonly bool s_IsPrimitive;
        private static readonly bool s_IsValueType;
        private static readonly bool s_IsInterface;
        private static readonly bool s_IsAbstract;
        private static readonly bool s_IsEnum;
        private static readonly bool s_IsArray;

        static RuntimeTypeInfoCache()
        {
            var type = typeof(T);
            s_IsPrimitive = type.IsPrimitive;
            s_IsValueType = type.IsValueType;
            s_IsInterface = type.IsInterface;
            s_IsAbstract = type.IsAbstract;
            s_IsArray = type.IsArray;
            s_IsEnum = type.IsEnum;
        }

        public static bool IsValueType()
        {
            return s_IsValueType;
        }

        public static bool IsInterface()
        {
            return s_IsInterface;
        }

        public static bool IsAbstract()
        {
            return s_IsAbstract;
        }

        public static bool IsArray()
        {
            return s_IsArray;
        }

        public static bool IsContainerType()
        {
            return !(s_IsPrimitive || s_IsEnum);
        }

        public static bool IsAbstractOrInterface()
        {
            return s_IsAbstract || s_IsInterface;
        }
    }
}
