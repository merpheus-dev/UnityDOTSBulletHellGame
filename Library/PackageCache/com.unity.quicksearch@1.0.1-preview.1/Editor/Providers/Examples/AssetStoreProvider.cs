//#define QUICKSEARCH_EXAMPLES
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class AssetStoreList
        {
            internal static readonly string type = "asset_store";
            internal static readonly string displayName = "Asset Store";
            internal static readonly string url = "http://shawarma.unity3d.com/public-api/search/assets.json?unityversion=2019.2.0a7&skip_terms=1&q=";

            private static UnityWebRequest s_CurrentSearchRequest;
            private static UnityWebRequestAsyncOperation s_CurrentSearchRequestOp;
            private static Action<int, SearchItem[]> s_AddAsyncItems;
            private static int s_SearchId;
            private static SearchProvider s_Provider;

            class AssetStoreData
            {
                public int id;
                public int packageId;
            }
            class AssetPreviewData
            {
                public string staticPreviewUrl;
                public Texture2D preview;
                public UnityWebRequest request;
                public UnityWebRequestAsyncOperation requestOp;
            }

            private static Dictionary<string, AssetPreviewData> s_CurrentSearchAssetData = new Dictionary<string, AssetPreviewData>();

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, SearchItemProvider]
            #endif
            internal static SearchProvider CreateProvider()
            {
                return new SearchProvider(type, displayName)
                {
                    filterId = "store:",
                    fetchItems = (context, items, provider) =>
                    {
                        if (s_CurrentSearchRequestOp != null)
                        {
                            s_CurrentSearchRequestOp.completed -= SearchCompleted;
                        }

                        s_SearchId = context.searchId;
                        s_Provider = provider;
                        s_AddAsyncItems = context.sendAsyncItems;
                        s_CurrentSearchRequest = UnityWebRequest.Get(url + context.searchQuery);
                        s_CurrentSearchRequest.SetRequestHeader("X-Unity-Session", InternalEditorUtility.GetAuthToken());
                        s_CurrentSearchRequestOp = s_CurrentSearchRequest.SendWebRequest();
                        s_CurrentSearchRequestOp.completed += SearchCompleted;
                    },

                    fetchThumbnail = (item, context) =>
                    {
                        if (item.thumbnail)
                            return item.thumbnail;

                        if (s_CurrentSearchAssetData.ContainsKey(item.id))
                        {
                            var searchPreview = s_CurrentSearchAssetData[item.id];
                            if (searchPreview.preview)
                                item.thumbnail = s_CurrentSearchAssetData[item.id].preview;

                            if (item.thumbnail)
                                return item.thumbnail;

                            if (searchPreview.request == null)
                            {
                                searchPreview.request = UnityWebRequestTexture.GetTexture(searchPreview.staticPreviewUrl);
                                searchPreview.requestOp = searchPreview.request.SendWebRequest();
                                searchPreview.requestOp.completed += TextureFetched;
                            }
                        }

                        item.thumbnail = InternalEditorUtility.FindIconForFile(item.label);
                        return item.thumbnail ?? Icons.store;
                    },

                    onDisable = () =>
                    {
                        foreach(var sp in s_CurrentSearchAssetData.Values)
                        {
                            if (sp.preview)
                            {
                                UnityEngine.Object.DestroyImmediate(sp.preview);
                            }
                            sp.request = null;
                            sp.requestOp = null;
                        }
                        s_CurrentSearchAssetData.Clear();
                    }

                };
            }

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, SearchActionsProvider]
            #endif
            internal static IEnumerable<SearchAction> ActionHandlers()
            {
                return new[]
                {
                    new SearchAction(type, "open", null, "Open project settings...") {
                        handler = (item, context) =>
                        {
                            var data = (AssetStoreData)item.data;
                            AssetStore.Open(string.Format("content/{0}?assetID={1}", data.packageId, data.id));
                        }
                    }
                };
            }

            internal static void TextureFetched(AsyncOperation op)
            {
                var searchPreview = s_CurrentSearchAssetData.Values.FirstOrDefault(sp => sp.requestOp == op);
                if (searchPreview != null)
                {
                    if (searchPreview.request.isDone && !searchPreview.request.isHttpError && !searchPreview.request.isNetworkError)
                    {
                        searchPreview.preview = DownloadHandlerTexture.GetContent(searchPreview.request);
                    }
                    searchPreview.requestOp.completed -= TextureFetched;
                    searchPreview.requestOp = null;
                }
            }

            internal static void SearchCompleted(AsyncOperation op)
            {
                if (s_CurrentSearchRequest.isDone && !s_CurrentSearchRequest.isHttpError && !s_CurrentSearchRequest.isNetworkError && !string.IsNullOrEmpty(s_CurrentSearchRequest.downloadHandler.text))
                {
                    try
                    {
                        var reqJson = Utils.JsonDeserialize(s_CurrentSearchRequest.downloadHandler.text) as Dictionary<string, object>;
                        if (!reqJson.ContainsKey("status") || reqJson["status"].ToString() != "ok" || !reqJson.ContainsKey("groups"))
                        {
                            return;
                        }

                        if (!(reqJson["groups"] is List<object> groups))
                            return;

                        var limitedMatches = new List<Dictionary<string, object>>();
                        foreach (var g in groups)
                        {
                            var group = g as Dictionary<string, object>;
                            if (group == null || !group.ContainsKey("matches"))
                                continue;

                            var matches = group["matches"] as List<object>;
                            if (matches == null)
                                continue;

                            foreach (var m in matches.Take(50))
                            {
                                var item = m as Dictionary<string, object>;
                                if (item == null)
                                    continue;
                                item["groupType"] = group["name"];
                                limitedMatches.Add(item);
                            }
                        }
                        
                        var items = new List<SearchItem>();
                        foreach (var match in limitedMatches)
                        {
                            if (!match.ContainsKey("name") || !match.ContainsKey("id") || !match.ContainsKey("package_id"))
                                continue;
                            var id = match["id"].ToString();
                            var item = s_Provider.CreateItem(id, match["name"].ToString(), $"Asset Store ({match["groupType"]})");
                            var data = new AssetStoreData();
                            data.packageId = Convert.ToInt32(match["package_id"]);
                            data.id = Convert.ToInt32(match["id"]);
                            item.data = data;

                            items.Add(item);
                            if (match.ContainsKey("static_preview_url") && !s_CurrentSearchAssetData.ContainsKey(id))
                            {
                                s_CurrentSearchAssetData.Add(id, new AssetPreviewData() { staticPreviewUrl = match["static_preview_url"].ToString() });
                            }
                        }

                        SearchService.SortItemList(items);
                        s_AddAsyncItems(s_SearchId, items.ToArray());
                    }
                    catch
                    {
                        // ignored
                    }
                }

                s_CurrentSearchRequestOp.completed -= SearchCompleted;
                s_CurrentSearchRequestOp = null;
                s_CurrentSearchRequest = null;
            }
        }
    }
}
