using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Burst.Compiler.IL.Tests
{
    internal class Functions
    {
        [TestCompiler]
        public static int CheckFunctionCall()
        {
            return AnotherFunction();
        }

        private static int AnotherFunction()
        {
            return 150;
        }

        [TestCompiler(ExpectCompilerException = true)]
        public static void Boxing()
        {
            var a = new CustomStruct();
            // This will box CustomStruct, so this method should fail when compiling
            a.GetType();
        }

        private struct CustomStruct
        {

        }

        public static int NotDiscardable()
        {
            return 3;
        }

        [BurstDiscard]
        public static void Discardable()
        {
        }

        [TestCompiler()]
        public static int TestCallsOfDiscardedMethodRegression()
        {
            // The regression was that we would queue all calls of a method, but if we encountered a discardable one
            // We would stop visiting pending methods. This resulting in method bodies not being visited.
            Discardable();
            return NotDiscardable();
        }

        // Notes: InternalFunctionWithBool does not have an implementation and should not be called,
        //this test is provided as a way to verify the compilation behaviour of calling a function with a
        //local.variable defined bool and an internal defined bool (32 bits vs 8 bits)
        //TestCompilerCommand.GetExternalFunctionPointer(string functionName) contains a match
        //for the below function in order to return a non null ptr to avoid an explicit
        //check at compilation time.
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool InternalFunctionWithBool(int a, bool c);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool NoInlineInternalFunctionWithBool(int a, bool c)
        {
            return InternalFunctionWithBool(a,c);
        }

        [TestCompiler(true)]
        public static bool TestInternalFunctionWithBool(bool localVar)
        {
            int value = 1;
            if (!localVar)
                return NoInlineInternalFunctionWithBool(value,localVar);
            return false;
        }
    }
}
