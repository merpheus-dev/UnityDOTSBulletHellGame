using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class Settings
        {
            internal static string type = "settings";
            internal static string displayName = "Settings";
            internal static SettingsProvider[] providers;

            [UsedImplicitly, SearchItemProvider]
            internal static SearchProvider CreateProvider()
            {
                if (providers == null)
                    providers = FetchSettingsProviders();

                return new SearchProvider(type, displayName)
                {
                    filterId = "se:",
                    fetchItems = (context, items, provider) =>
                    {
                        items.AddRange(providers.Where(settings =>
                            SearchProvider.MatchSearchGroups(context, settings.settingsPath))
                            .Select(settings => provider.CreateItem(settings.settingsPath, Utils.GetNameFromPath(settings.settingsPath), settings.settingsPath)));
                    },

                    fetchThumbnail = (item, context) => Icons.settings
                };
            }

            [UsedImplicitly, SearchActionsProvider]
            internal static IEnumerable<SearchAction> ActionHandlers()
            {
                return new[]
                {
                    new SearchAction(type, "open", null, "Open project settings...") {
                        handler = (item, context) =>
                        {
                            if (item.id.StartsWith("Project/"))
                                SettingsService.OpenProjectSettings(item.id);
                            else
                                SettingsService.OpenUserPreferences(item.id);
                        }
                    }
                };
            }

            private static SettingsProvider[] FetchSettingsProviders()
            {
                Assembly assembly = typeof(SettingsService).Assembly;
                var managerType = assembly.GetTypes().First(t => t.Name == "SettingsService");
                var method = managerType.GetMethod("FetchSettingsProviders", BindingFlags.NonPublic | BindingFlags.Static);
                var arguments = new object[] {};
                return method.Invoke(null, arguments) as SettingsProvider[];
            }
        }
    }
}
