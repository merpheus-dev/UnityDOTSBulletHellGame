// #define QUICKSEARCH_EXAMPLES
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class Calculator
        {
            internal static string type = "calculator";
            internal static string displayName = "Calculator";

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, SearchItemProvider]
            #endif
            internal static SearchProvider CreateProvider()
            {
                return new SearchProvider(type, displayName)
                {
                    filterId = "=",
                    fetchItems = (context, items, provider) =>
                    {
                        items.Add(provider.CreateItem(type, "compute", context.searchQuery));
                    },

                    fetchThumbnail = (item, context) => Icons.settings
                };
            }

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, SearchActionsProvider]
            #endif
            internal static IEnumerable<SearchAction> ActionHandlers()
            {
                return new[]
                {
                    new SearchAction(type, "compute", null, "Compute...") {
                        handler = (item, context) =>
                        {
                            try
                            {
                                ExpressionEvaluator.Evaluate<double>(context.searchQuery, out var result);
                                UnityEngine.Debug.Log(result);
                                EditorGUIUtility.systemCopyBuffer = result.ToString();
                            }
                            catch (Exception)
                            {
                                UnityEngine.Debug.LogError("Error while parsing: " + context.searchQuery);
                            }
                        }
                    }
                };
            }
        }
    }
}
