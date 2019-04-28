//#define QUICKSEARCH_DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if QUICKSEARCH_DEBUG
using System.Reflection;
using Debug = System.Diagnostics.Debug;
#endif

namespace Unity.QuickSearch
{
    public delegate Texture2D PreviewHandler(SearchItem item, SearchContext context);
    public delegate string DescriptionHandler(SearchItem item, SearchContext context);
    public delegate void ActionHandler(SearchItem item, SearchContext context);
    public delegate void StartDragHandler(SearchItem item, SearchContext context);
    public delegate bool EnabledHandler(SearchItem item, SearchContext context);
    public delegate void GetItemsHandler(SearchContext context, List<SearchItem> items, SearchProvider provider);
    public delegate bool IsItemValidHandler(SearchItem item);

    public class SearchAction
    {
        public SearchAction(string type, GUIContent content)
        {
            this.type = type;
            this.content = content;
            isEnabled = (item, context) => true;
        }

        public SearchAction(string type, string name, Texture2D icon = null, string tooltip = null)
            : this(type, new GUIContent(name, icon, tooltip))
        {
        }

        public string type;
        public GUIContent content;
        public ActionHandler handler;
        public EnabledHandler isEnabled;
    }

    public struct SearchItem
    {
        // Unique id of this item among this provider items.
        public string id;
        // Display name of the item
        public string label;
        // If no description is provided, SearchProvider.fetchDescription will be called when the item is first displayed.
        public string description;
        // If no thumbnail are provider, SearchProvider.fetchThumbnail will be called when the item is first displayed.
        public Texture2D thumbnail;
        // Back pointer to the provider.
        public SearchProvider provider;
        // Search provider defined content
        public object data;
    }

    public class SearchFilter
    {
        [DebuggerDisplay("{name.displayName}")]
        public class Entry
        {
            public Entry(NameId name)
            {
                this.name = name;
                isEnabled = true;
            }

            public NameId name;
            public bool isEnabled;
        }

        [DebuggerDisplay("{entry.name.displayName} expanded:{isExpanded}")]
        public class ProviderDesc
        {
            public ProviderDesc(NameId name)
            {
                entry = new Entry(name);
                categories = new List<Entry>();
                isExpanded = false;
                priority = 100;
            }

            public Entry entry;
            public bool isExpanded;
            public List<Entry> categories;
            public int priority;
        }

        public List<SearchProvider> filteredProviders;
        public List<ProviderDesc> providerFilters;

        private List<SearchProvider> m_Providers;
        public List<SearchProvider> Providers
        {
            get => m_Providers;

            set
            {
                m_Providers = value;
                providerFilters.Clear();
                filteredProviders.Clear();
                foreach (var provider in m_Providers)
                {
                    var providerFilter = new ProviderDesc(new NameId(provider.name.id,
                            string.IsNullOrEmpty(provider.filterId) ? provider.name.displayName : provider.name.displayName + " (" + provider.filterId + ")")) {priority = provider.priority};
                    providerFilters.Add(providerFilter);
                    foreach (var subCategory in provider.subCategories)
                    {
                        providerFilter.categories.Add(new Entry(subCategory));
                    }
                }
                UpdateFilteredProviders();
            }
        }

        public SearchFilter()
        {
            filteredProviders = new List<SearchProvider>();
            providerFilters = new List<ProviderDesc>();
        }

        public void ResetFilter(bool enableAll)
        {
            foreach (var providerDesc in providerFilters)
            {
                SetFilterInternal(enableAll, providerDesc.entry.name.id);
            }
            UpdateFilteredProviders();
        }

        public void SetFilter(bool isEnabled, string providerId, string subCategory = null)
        {
            if (SetFilterInternal(isEnabled, providerId, subCategory))
            {
                UpdateFilteredProviders();
            }
        }

        public void SetExpanded(bool isExpanded, string providerId)
        {
            var providerDesc = providerFilters.Find(pd => pd.entry.name.id == providerId);
            if (providerDesc != null)
            {
                providerDesc.isExpanded = isExpanded;
            }
        }

        public bool IsEnabled(string providerId, string subCategory = null)
        {
            var desc = providerFilters.Find(pd => pd.entry.name.id == providerId);
            if (desc != null)
            {
                if (subCategory == null)
                {
                    return desc.entry.isEnabled;
                }

                foreach (var cat in desc.categories)
                {
                    if (cat.name.id == subCategory)
                        return cat.isEnabled;
                }
            }

            return false;
        }

        public List<Entry> GetSubCategories(SearchProvider provider)
        {
            var desc = providerFilters.Find(pd => pd.entry.name.id == provider.name.id);
            return desc?.categories;
        }

        internal void UpdateFilteredProviders()
        {
            filteredProviders = Providers.Where(p => IsEnabled(p.name.id)).ToList();
        }

        internal bool SetFilterInternal(bool isEnabled, string providerId, string subCategory = null)
        {
            var providerDesc = providerFilters.Find(pd => pd.entry.name.id == providerId);
            if (providerDesc != null)
            {
                if (subCategory == null)
                {
                    providerDesc.entry.isEnabled = isEnabled;
                    foreach (var cat in providerDesc.categories)
                    {
                        cat.isEnabled = isEnabled;
                    }
                }
                else
                {
                    foreach (var cat in providerDesc.categories)
                    {
                        if (cat.name.id == subCategory)
                        {
                            cat.isEnabled = isEnabled;
                            if (isEnabled)
                                providerDesc.entry.isEnabled = true;
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }

    [DebuggerDisplay("{id}")]
    public class NameId
    {
        public NameId(string id, string displayName = null)
        {
            this.id = id;
            this.displayName = displayName ?? id;
        }

        public string id;
        public string displayName;
    }

    [DebuggerDisplay("{name}")]
    public class SearchProvider
    {
        public SearchProvider(string id, string displayName = null)
        {
            name = new NameId(id, displayName);
            actions = new List<SearchAction>();
            fetchItems = (context, items, provider) => {};
            fetchThumbnail = (item, context) => item.thumbnail ?? Icons.quicksearch;
            fetchDescription = (item, context) => item.description ?? String.Empty;
            subCategories = new List<NameId>();
            isItemValid = item => true;
            priority = 100;
        }

        public SearchItem CreateItem(string id, string label = null, string description = null, Texture2D thumbnail = null, object data = null)
        {
            return new SearchItem
            {
                id = id,
                label = label ?? id,
                description = description,
                thumbnail = thumbnail,
                provider = this,
                data = data
            };
        }

        public static bool MatchSearchGroups(SearchContext context, string content, bool useLowerTokens = false)
        {
            return MatchSearchGroups(context.searchQuery,
                useLowerTokens ? context.tokenizedSearchQueryLower : context.tokenizedSearchQuery, content, out _, out _, 
                useLowerTokens ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchSearchGroups(string searchContext, string[] tokens, string content, out int startIndex, out int endIndex, StringComparison sc = StringComparison.OrdinalIgnoreCase)
        {
            startIndex = endIndex = -1;
            if (content == null)
                return false;

            if (string.IsNullOrEmpty(searchContext) || searchContext == content)
            {
                startIndex = 0;
                endIndex = content.Length - 1;
                return true;
            }

            // Each search group is space separated
            // Search group must match in order and be complete.
            var searchGroups = tokens;
            var startSearchIndex = 0;
            foreach (var searchGroup in searchGroups)
            {
                if (searchGroup.Length == 0)
                    continue;

                startSearchIndex = content.IndexOf(searchGroup, startSearchIndex, sc);
                if (startSearchIndex == -1)
                {
                    return false;
                }

                startIndex = startIndex == -1 ? startSearchIndex : startIndex;
                startSearchIndex = endIndex = startSearchIndex + searchGroup.Length - 1;
            }

            return startIndex != -1 && endIndex != -1;
        }

        public NameId name;
        public string filterId;
        public DescriptionHandler fetchDescription;
        public PreviewHandler fetchThumbnail;
        public StartDragHandler startDrag;
        public GetItemsHandler fetchItems;
        public List<SearchAction> actions;
        public List<NameId> subCategories;
        public Action onEnable;
        public Action onDisable;
        public IsItemValidHandler isItemValid;
        public int priority;
    }

    [DebuggerDisplay("{searchQuery}")]
    public class SearchContext
    {
        public int searchId;
        public string searchBoxText;
        public string searchQuery;
        public EditorWindow focusedWindow;
        public string[] tokenizedSearchQuery;
        public string[] tokenizedSearchQueryLower;
        public string[] textFilters;
        public List<SearchFilter.Entry> categories;
        public int totalItemCount;

        public Action<int, SearchItem[]> sendAsyncItems;
    }

    public class SearchItemProviderAttribute : Attribute
    {
    }

    public class SearchActionsProviderAttribute : Attribute
    {
    }

    public static class SearchService
    {
        const string k_FilterPrefKey = "quicksearch.filters";
        const string k_LastSearchPrefKey = "quicksearch.last_search";
        
        private static int s_CurrentSearchId = 0;
        private static string s_LastSearch;
        private static List<string> s_RecentSearches = new List<string>(10);
        private static int s_RecentSearchIndex = -1;

        internal static List<SearchProvider> Providers { get; private set; }
        internal static HashSet<string> ProviderTextFilters { get; private set; }
        internal static SearchFilter TextFilter { get; private set; }

        internal static string LastSearch
        {
            get => s_LastSearch;
            set
            {
                if (value == s_LastSearch)
                    return;
                s_LastSearch = value;
                if (String.IsNullOrEmpty(value))
                    return;
                s_RecentSearchIndex = 0;
                s_RecentSearches.Insert(0, value);
                if (s_RecentSearches.Count > 10)
                    s_RecentSearches.RemoveRange(10, s_RecentSearches.Count - 10);
                s_RecentSearches = s_RecentSearches.Distinct().ToList();
            }
        }

        internal static string CyclePreviousSearch(int shift)
        {
            if (s_RecentSearches.Count == 0)
                return s_LastSearch;

            s_RecentSearchIndex = Wrap(s_RecentSearchIndex + shift, s_RecentSearches.Count);
            
            return s_RecentSearches[s_RecentSearchIndex];
        }

        private static int Wrap(int index, int n)
        {
            return ((index % n) + n) % n;
        }

        public static SearchFilter Filter { get; private set; }
        public static event Action<IEnumerable<SearchItem>> asyncItemReceived;

        static SearchService()
        {
            Refresh();
        }

        internal static void Refresh()
        {
            Providers = new List<SearchProvider>();
            Filter = new SearchFilter();
            TextFilter = new SearchFilter();
            var settingsValid = FetchProviders();
            settingsValid = LoadSettings() || settingsValid;
            LastSearch = EditorPrefs.GetString(k_LastSearchPrefKey, "");

            if (!settingsValid)
            {
                // Override all settings
                SaveSettings();
            }
        }

        internal static bool LoadSettings()
        {
            LastSearch = EditorPrefs.GetString(k_LastSearchPrefKey, "");
            return LoadFilters();
        }

        internal static void SaveSettings()
        {
            SaveFilters();
            SaveLastSearch();
        }

        internal static void Reset()
        {
            EditorPrefs.SetString(k_FilterPrefKey, null);
            EditorPrefs.SetString(k_LastSearchPrefKey, null);
            Refresh();
        }

        public static List<SearchItem> GetItems(SearchContext context)
        {
            if (!string.IsNullOrEmpty(context.searchQuery))
            {
                if (TextFilter.filteredProviders.Count > 0)
                {
                    return GetItems(context, TextFilter);
                }
                return GetItems(context, Filter);
            }

            return new List<SearchItem>(0);
        }

        internal static void SaveLastSearch()
        {
            EditorPrefs.SetString(k_LastSearchPrefKey, LastSearch);
        }

        internal static void SearchTextChanged(SearchContext context)
        {
            context.searchQuery = context.searchBoxText;
            string providerFilterOverride = null;
            foreach (var textFilter in ProviderTextFilters)
            {
                if (context.searchQuery.StartsWith(textFilter))
                {
                    providerFilterOverride = textFilter;
                    context.searchQuery = context.searchQuery.Remove(0, textFilter.Length).Trim();
                    break;
                }
            }

            var tokens = context.searchQuery.Split(' ');
            context.tokenizedSearchQuery = tokens.Where(t => !t.Contains(":")).ToArray();
            context.tokenizedSearchQueryLower = context.tokenizedSearchQuery.Select(t => t.ToLowerInvariant()).ToArray();
            context.textFilters = tokens.Where(t => t.Contains(":")).ToArray();

            // Reformat search text so it only contains text filter that are specific to providers and ensure those filters are at the beginning of the search text.
            context.searchQuery = string.Join(" ", context.textFilters.Concat(context.tokenizedSearchQuery));

            if (providerFilterOverride != null)
            {
                TextFilter.ResetFilter(false);
                foreach (var provider in Providers)
                {
                    if (provider.filterId == providerFilterOverride)
                    {
                        TextFilter.SetFilter(true, provider.name.id);
                    }
                }
            }
            else if (TextFilter.filteredProviders.Count > 0)
            {
                TextFilter.ResetFilter(false);
            }
        }

        private static List<SearchItem> GetItems(SearchContext context, SearchFilter filter)
        {
            context.searchId = ++s_CurrentSearchId;
            context.sendAsyncItems = OnAsyncItemsReceived;
            var allItems = new List<SearchItem>(100);
            foreach (var provider in filter.filteredProviders)
            {
                #if QUICKSEARCH_DEBUG
                using (new DebugTimer($"{provider.name.id} fetch items"))
                #endif
                {
                    context.categories = filter.GetSubCategories(provider);
                    try
                    {
                        provider.fetchItems(context, allItems, provider);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Failed to get fetch {provider.name.displayName} provider items.\r\n{ex}");
                    }
                }
            }

            SortItemList(allItems);
            return allItems;
        }

        internal static void SortItemList(List<SearchItem> items)
        {
            items.Sort(SortItemComparer);
        }

        private static int SortItemComparer(SearchItem item1, SearchItem item2)
        {
            var po = item1.provider.priority.CompareTo(item2.provider.priority);
            if (po != 0) return po;
            return String.Compare(item1.label, item2.label, StringComparison.Ordinal);
        }

        private static void OnAsyncItemsReceived(int searchId, SearchItem[] items)
        {
            if (s_CurrentSearchId != searchId)
                return;
            EditorApplication.delayCall += () => asyncItemReceived?.Invoke(items);
        }

        private static bool FetchProviders()
        {
            try
            {
                Providers = Utils.GetAllMethodsWithAttribute<SearchItemProviderAttribute>()
                    .Select(methodInfo => methodInfo.Invoke(null, null) as SearchProvider)
                    .Where(provider => provider != null).ToList();

                foreach (var action in Utils.GetAllMethodsWithAttribute<SearchActionsProviderAttribute>()
                         .SelectMany(methodInfo => methodInfo.Invoke(null, null) as object[]).Where(a => a != null).Cast<SearchAction>())
                {
                    var provider = Providers.Find(p => p.name.id == action.type);
                    provider?.actions.Add(action);
                }

                Filter.Providers = Providers;
                TextFilter.Providers = Providers;

                ProviderTextFilters = new HashSet<string>();
                foreach (var provider in Providers)
                {
                    if (string.IsNullOrEmpty(provider.filterId))
                        continue;

                    if (char.IsLetterOrDigit(provider.filterId[provider.filterId.Length - 1]))
                    {
                        UnityEngine.Debug.LogWarning($"Provider: {provider.name.id} filterId: {provider.filterId} must ends with non-alphanumeric character.");
                        continue;
                    }

                    ProviderTextFilters.Add(provider.filterId);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static bool LoadFilters()
        {
            try
            {
                var filtersStr = EditorPrefs.GetString(k_FilterPrefKey, null);
                Filter.ResetFilter(true);

                if (!string.IsNullOrEmpty(filtersStr))
                {
                    var filters = Utils.JsonDeserialize(filtersStr) as List<object>;
                    foreach (var filterObj in filters)
                    {
                        var filter = filterObj as Dictionary<string, object>;
                        if (filter == null)
                            continue;

                        var providerId = filter["providerId"] as string;
                        Filter.SetExpanded(filter["isExpanded"].ToString() == "True", providerId);
                        Filter.SetFilterInternal(filter["isEnabled"].ToString() == "True", providerId);
                        var categories = filter["categories"] as List<object>;
                        foreach (var catObj in categories)
                        {
                            var cat = catObj as Dictionary<string, object>;
                            Filter.SetFilterInternal(cat["isEnabled"].ToString() == "True", providerId, cat["id"] as string);
                        }
                    }
                }

                Filter.UpdateFilteredProviders();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static string FilterToString()
        {
            var filters = new List<object>();
            foreach (var providerDesc in Filter.providerFilters)
            {
                var filter = new Dictionary<string, object>
                {
                    ["providerId"] = providerDesc.entry.name.id, 
                    ["isEnabled"] = providerDesc.entry.isEnabled, 
                    ["isExpanded"] = providerDesc.isExpanded
                };
                var categories = new List<object>();
                filter["categories"] = categories;
                foreach (var cat in providerDesc.categories)
                {
                    categories.Add(new Dictionary<string, object>()
                    {
                        { "id", cat.name.id },
                        { "isEnabled", cat.isEnabled }
                    });
                }
                filters.Add(filter);
            }

            return Utils.JsonSerialize(filters);
        }

        private static void SaveFilters()
        {
            var filter = FilterToString();
            EditorPrefs.SetString(k_FilterPrefKey, filter);
        }
    }
}
