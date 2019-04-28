using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class AssetProvider
        {
            internal static string type = "asset";
            internal static string displayName = "Asset";
            /* Filters:
                 t:<type>
                l:<label>
                ref[:id]:path
                v:<versionState>
                s:<softLockState>
                a:<area> [assets, packages]
             */

            internal static string[] typeFilter = new[]
            {
                "DefaultAsset",
                "AnimationClip",
                "AudioClip",
                "AudioMixer",
                "ComputeShader",
                "Font",
                "GUISKin",
                "Material",
                "Mesh",
                "Model",
                "PhysicMaterial",
                "Prefab",
                "Scene",
                "Script",
                "Shader",
                "Sprite",
                "Texture",
                "VideoClip"
            };

            internal static string[] areaFilter = new[]
            {
                "assets",
                "packages"
            };

            internal static string[] extraFilter = new[]
            {
                "folders"
            };

            private static readonly char[] k_InvalidSearchFileChars = Path.GetInvalidFileNameChars().Where(c => c != '*').ToArray();

            [UsedImplicitly, SearchItemProvider]
            internal static SearchProvider CreateProvider()
            {
                var provider = new SearchProvider(type, displayName)
                {
                    priority = 25,
                    filterId = "p:",
                    fetchItems = (context, items, _provider) =>
                    {
                        var filter = context.searchQuery;

                        // Check if folder should be filtered or not then remove the a: find tag
                        bool findFolders = !context.categories.Any(c => c.name.displayName == "folders" && c.isEnabled == false);

                        var areas = context.categories.GetRange(0, areaFilter.Length);

                        if (areas.Any(c => !c.isEnabled))
                        {
                            // Not all categories are enabled, so create a proper filter:
                            filter = string.Join(" ", areas.Where(c => c.isEnabled).Select(c => c.name.id)) + " " + filter;
                        }

                        var nonTypeFilterCount = areaFilter.Length + extraFilter.Length;
                        var types = context.categories.GetRange(nonTypeFilterCount, context.categories.Count - nonTypeFilterCount);
                        if (types.Any(c => c.isEnabled))
                        {
                            if (types.Any(c => !c.isEnabled))
                            {
                                // Not all categories are enabled, so create a proper filter:
                                filter = string.Join(" ", types.Where(c => c.isEnabled).Select(c => c.name.id)) + " " + filter;
                            }

                            items.AddRange(AssetDatabase.FindAssets(filter)
                                                        .Select(AssetDatabase.GUIDToAssetPath)
                                                        .Where(path => !AssetDatabase.IsValidFolder(path))
                                                        .Take(1001)
                                                        .Select(path => _provider.CreateItem(path, Path.GetFileName(path))));
                        }
                        
                        var safeFilter = string.Join("_", context.searchQuery.Split(k_InvalidSearchFileChars));
                        if (context.searchQuery.Contains('*'))
                        {
                            items.AddRange(Directory.GetFiles(Application.dataPath, safeFilter, SearchOption.AllDirectories)
                                .Select(path => _provider.CreateItem(path.Replace(Application.dataPath, "Assets").Replace("\\", "/"), Path.GetFileName(path))));
                        }

                        if (findFolders)
                        {
                            items.AddRange(Directory.GetDirectories(Application.dataPath, safeFilter + "*", SearchOption.AllDirectories)
                                                    .Select(path => _provider.CreateItem(path.Replace(Application.dataPath, "Assets").Replace("\\", "/"), Path.GetFileName(path))));
                            
                        }
                    },

                    fetchDescription = (item, context) =>
                    {
                        if (AssetDatabase.IsValidFolder(item.id))
                            return item.id;
                        long fileSize = new FileInfo(item.id).Length;
                        item.description = $"{item.id} ({EditorUtility.FormatBytes(fileSize)})";
                        return item.description;
                    },

                    fetchThumbnail = (item, context) =>
                    {
                        if (item.thumbnail)
                            return item.thumbnail;

                        if (context.totalItemCount < 200)
                        {
                            var obj = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                            if (obj != null)
                                item.thumbnail = AssetPreview.GetAssetPreview(obj);
                            if (item.thumbnail)
                                return item.thumbnail;
                        }
                        item.thumbnail = AssetDatabase.GetCachedIcon(item.id) as Texture2D;
                        if (item.thumbnail)
                            return item.thumbnail;

                        item.thumbnail = UnityEditorInternal.InternalEditorUtility.FindIconForFile(item.id);
                        return item.thumbnail;
                    },

                    startDrag = (item, context) =>
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                        if (obj != null)
                        {
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new[] { obj };
                            DragAndDrop.StartDrag("Drag asset");
                        }
                    },

                    subCategories = new List<NameId>()
                };

                foreach (var subCat in areaFilter)
                    provider.subCategories.Add(new NameId("a:" + subCat, subCat));

                foreach (var subCat in extraFilter)
                    provider.subCategories.Add(new NameId("special:" + subCat, subCat));

                // Type filter always need to be added last to the category list
                foreach (var subCat in typeFilter)
                    provider.subCategories.Add(new NameId("t:" + subCat, subCat));

                return provider;
            }

            [UsedImplicitly, SearchActionsProvider]
            internal static IEnumerable<SearchAction> ActionHandlers()
            {
                #if UNITY_EDITOR_OSX
                const string k_RevealActionLabel = "Reveal in Finder...";
                #else
                const string k_RevealActionLabel = "Show in Explorer...";
                #endif

                return new[]
                {
                    new SearchAction("asset", "select", null, "Select asset...")
                    {
                        handler = (item, context) =>
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                            if (asset != null)
                            {
                                Selection.activeObject = asset;
                                EditorApplication.delayCall += () =>
                                {
                                    EditorWindow.FocusWindowIfItsOpen(Utils.GetProjectBrowserWindowType());
                                    EditorGUIUtility.PingObject(asset);
                                };
                            }
                            else
                            {
                                EditorUtility.RevealInFinder(item.id);
                            }
                        }
                    },
                    new SearchAction("asset", "open", null, "Open asset...")
                    {
                        handler = (item, context) =>
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(item.id);
                            if (asset != null) AssetDatabase.OpenAsset(asset);
                        }
                    },
                    new SearchAction("asset", "reveal", null, k_RevealActionLabel)
                    {
                        handler = (item, context) => EditorUtility.RevealInFinder(item.id)
                    }
                };
            }

            #if UNITY_2019_1_OR_NEWER
            [UsedImplicitly, Shortcut("Help/Quick Search/Assets", KeyCode.A, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
            public static void PopQuickSearch()
            {
                SearchService.Filter.ResetFilter(false);
                SearchService.Filter.SetFilter(true, type);
                SearchService.Filter.SetFilter(false, type, "a:" + areaFilter[1]);
                QuickSearchTool.ShowWindow(false);
            }
            #endif
        }
    }
}
