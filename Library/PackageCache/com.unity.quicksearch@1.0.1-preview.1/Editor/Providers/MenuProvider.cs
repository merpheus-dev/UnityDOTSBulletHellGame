using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class MenuProvider
        {
            internal static string type = "menu";
            internal static string displayName = "Menu";

            internal static string[] itemNamesLower;
            internal static List<string> itemNames = new List<string>();

            [UsedImplicitly, SearchItemProvider]
            internal static SearchProvider CreateProvider()
            {
                List<string> shortcuts = new List<string>();
                GetMenuInfo(itemNames, shortcuts);
                itemNamesLower = itemNames.Select(n => n.ToLowerInvariant()).ToArray();

                return new SearchProvider(type, displayName)
                {
                    priority = 80,
                    filterId = "me:",
                    fetchItems = (context, items, provider) =>
                    {
                        for (int i = 0; i < itemNames.Count; ++i)
                        {
                            var menuName = itemNames[i];
                            if (!SearchProvider.MatchSearchGroups(context, itemNamesLower[i], true))
                                continue;
                            items.Add(provider.CreateItem(menuName, Utils.GetNameFromPath(menuName), menuName));
                        }
                    },

                    fetchThumbnail = (item, context) => Icons.shortcut
                };
            }

            [UsedImplicitly, SearchActionsProvider]
            internal static IEnumerable<SearchAction> ActionHandlers()
            {
                return new[]
                {
                    new SearchAction("menu", "exec", null, "Execute shortcut...")
                    {
                        handler = (item, context) =>
                        {
                            var menuId = item.id;
                            EditorApplication.delayCall += () => EditorApplication.ExecuteMenuItem(menuId);
                        }
                    }
                };
            }

            #if UNITY_2019_1_OR_NEWER
            [UsedImplicitly, Shortcut("Help/Quick Search/Menu", KeyCode.M, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
            public static void PopQuickSearch()
            {
                SearchService.Filter.ResetFilter(false);
                SearchService.Filter.SetFilter(true, type);
                QuickSearchTool.ShowWindow(false);
            }

            #endif

            private static void GetMenuInfo(List<string> outItemNames, List<string> outItemDefaultShortcuts)
            {
                Assembly assembly = typeof(Menu).Assembly;
                var managerType = assembly.GetTypes().First(t => t.Name == "Menu");
                var method = managerType.GetMethod("GetMenuItemDefaultShortcuts", BindingFlags.NonPublic | BindingFlags.Static);
                var arguments = new object[] { outItemNames, outItemDefaultShortcuts };
                method.Invoke(null, arguments);
            }
        }
    }
}
