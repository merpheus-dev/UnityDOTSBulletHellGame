using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class SceneObjects
        {
            public struct GOD
            {
                public string name;
                public GameObject gameObject;
            }

            class SceneSearchProvider : SearchProvider
            {
                public GOD[] gods { get; set; }

                public SceneSearchProvider(string providerId, string displayName = null)
                    : base(providerId, displayName)
                {
                    priority = 50;
                    filterId = "h:";

                    onEnable = () =>
                    {
                        var objects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
                        gods = new GOD[objects.Length];
                        for (int i = 0; i < objects.Length; ++i)
                        {
                            gods[i].gameObject = (GameObject)objects[i];
                            gods[i].name = gods[i].gameObject.name.ToLower();
                        }
                    };

                    onDisable = () => 
                    {
                        gods = new GOD[0];
                    };

                    fetchItems = (context, items, provider) =>
                    {
                        var sq = context.searchQuery.ToLowerInvariant();

                        int addedCount = 0;
                        int i = 0, end = 0;
                        for (i = 0, end = gods.Length; i != end; ++i)
                        {
                            if (!SearchProvider.MatchSearchGroups(context, gods[i].name, true))
                                continue;

                            var go = gods[i].gameObject;
                            var gameObjectId = go.GetInstanceID().ToString();
                        
                            items.Add(provider.CreateItem(gameObjectId, $"{go.name} ({gameObjectId})", null));
                            if (++addedCount >= 200)
                                break;
                        }
                    };

                    fetchDescription = (item, context) =>
                    {
                        const int maxChars = 85;
                        var go = ObjectFromItem(item);
                        item.data = item.description = go.transform.GetPath();
                        if (item.description.Length > maxChars)
                            item.description = "..." + item.description.Substring(item.description.Length-maxChars, maxChars);
                        return item.description;
                    };

                    fetchThumbnail = (item, context) =>
                    {
                        if (item.thumbnail)
                            return item.thumbnail;

                        var obj = ObjectFromItem(item);
                        if (obj != null)
                        {
                            item.thumbnail = PrefabUtility.GetIconForGameObject(obj);
                            if (item.thumbnail)
                                return item.thumbnail;
                            item.thumbnail = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image as Texture2D;
                        }

                        return item.thumbnail;
                    };

                    startDrag = (item, context) =>
                    {
                        var obj = ObjectFromItem(item);
                        if (obj != null)
                        {
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new[] { obj };
                            DragAndDrop.StartDrag("Drag scene object");
                        }
                    };

                    isItemValid = item => ObjectFromItem(item) != null;
                }
            }

            internal static string type = "scene";
            internal static string displayName = "Scene";
            [UsedImplicitly, SearchItemProvider]
            internal static SearchProvider CreateProvider()
            {
                return new SceneSearchProvider(type, displayName);
            }

            [UsedImplicitly, SearchActionsProvider]
            internal static IEnumerable<SearchAction> ActionHandlers()
            {
                return new SearchAction[]
                {
                    new SearchAction(type, "select", null, "Select object in scene...") {
                        handler = (item, context) =>
                        {
                            var obj = ObjectFromItem(item);
                            if (obj != null)
                            {
                                Selection.activeGameObject = obj;
                                EditorGUIUtility.PingObject(obj);
                                SceneView.lastActiveSceneView.FrameSelected();
                            }
                        }
                    }
                };
            }

            private static GameObject ObjectFromItem(SearchItem item)
            {
                var instanceID = Convert.ToInt32(item.id);
                var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                return obj;
            }

            public static string GetPath(this Transform tform)
            {
                if (tform.parent == null)
                    return "/" + tform.name;
                return tform.parent.GetPath() + "/" + tform.name;
            }

            #if UNITY_2019_1_OR_NEWER
            [UsedImplicitly, Shortcut("Help/Quick Search/Scene", KeyCode.S, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
            public static void PopQuickSearch()
            {
                SearchService.Filter.ResetFilter(false);
                SearchService.Filter.SetFilter(true, type);
                QuickSearchTool.ShowWindow(false);
            }

            #endif
        }
    }
}
