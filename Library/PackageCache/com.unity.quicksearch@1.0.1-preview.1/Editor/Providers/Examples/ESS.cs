// #define QUICKSEARCH_EXAMPLES
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    namespace Providers
    {
        [UsedImplicitly]
        static class ESS
        {
            internal static string type = "ess";
            internal static string displayName = "Source Search";
            internal static string ess_exe = @"C:\Program Files (x86)\Entrian Source Search\ess.exe";

            internal static bool s_BuildingIndex = true;

            internal static Thread s_SearchThread;
            internal static AutoResetEvent s_StopEvent;

            internal static string indexPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/ess.index"));
            internal static Regex essRx = new Regex(@"([^(]+)\((\d+)\):\s*(.*)");

            struct ESSMatchInfo
            {
                public string path;
                public int lineNumber;
                public string content;
            }

            struct RunResult
            {
                public int code;
                public string output;
                public Exception exception;
            }

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, SearchItemProvider]
            #endif
            internal static SearchProvider CreateProvider()
            {
                if (!File.Exists(ess_exe))
                    return null;

                return new SearchProvider(type, displayName)
                {
                    priority = 7000,
                    filterId = "ess:",
                    fetchItems = (context, items, provider) =>
                    {
                        if (s_BuildingIndex)
                            return;

                        if (context.sendAsyncItems == null)
                            return;

                        if (s_SearchThread != null && s_SearchThread.IsAlive)
                        {
                            s_StopEvent.Set();
                            if (!s_SearchThread.Join(100))
                                s_SearchThread.Abort();
                        }

                        var localSearchId = context.searchId;
                        var localSearchQuery = context.searchQuery;
                        var localIndexPath = indexPath;
                        var localDataPath = Application.dataPath;
                        var localSendItemsCallback = context.sendAsyncItems;
                        s_StopEvent.Reset();
                        s_SearchThread = new Thread(() =>
                        {
                            var result = RunESS("search", ParamValueString("index", localIndexPath), localSearchQuery);
                            if (result.code != 0)
                                return;
                            var entries = result.output.Split('\n').Where(line => line.Trim().Length > 0);
                            var asyncItems = entries.Select(line =>
                            {
                                var m = essRx.Match(line);
                                var filePath = m.Groups[1].Value.Replace("\\", "/");
                                var essmi = new ESSMatchInfo
                                {
                                    path = filePath.Replace(localDataPath, "Assets").Replace("\\", "/"),
                                    lineNumber = int.Parse(m.Groups[2].Value),
                                    content = m.Groups[3].Value
                                };
                                var fsq = localSearchQuery.Replace("*", "");
                                var content = Regex.Replace(essmi.content, fsq, "<color=#FFFF00>" + fsq + "</color>", RegexOptions.IgnoreCase);
                                var description = $"{essmi.path} (<b>{essmi.lineNumber}</b>)";
                                return provider.CreateItem(essmi.content.GetHashCode().ToString(), content, description, null, essmi);
                            }).ToArray();

                            if (asyncItems.Length > 0)
                                localSendItemsCallback(localSearchId, asyncItems);
                        });
                        s_SearchThread.Start();
                    },

                    fetchThumbnail = (item, context) =>
                    {
                        if (item.data == null)
                            return null;

                        if (item.thumbnail)
                            return item.thumbnail;

                        var essmi = (ESSMatchInfo)item.data;
                        item.thumbnail = UnityEditorInternal.InternalEditorUtility.FindIconForFile(essmi.path);
                        return item.thumbnail;
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
                    new SearchAction(type, "locate", null, "locate")
                    {
                        handler = (item, context) =>
                        {
                            var essmi = (ESSMatchInfo)item.data;
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(essmi.path, essmi.lineNumber);
                        }
                    }
                };
            }

            private static RunResult RunESS(params string[] args)
            {
                var result = new RunResult { code = -1 };

                try
                {
                    var essProcess = new Process
                    {
                        StartInfo =
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = ess_exe,
                            Arguments = String.Join(" ", args)
                        },

                        EnableRaisingEvents = true
                    };

                    essProcess.OutputDataReceived += (sender, log) => result.output += log.Data + "\n";
                    essProcess.Start();
                    essProcess.BeginOutputReadLine();

                    while (!essProcess.WaitForExit(50))
                    {
                        if (s_StopEvent.WaitOne(1) && !essProcess.HasExited)
                        {
                            essProcess.Kill();
                            return result;
                        }
                    }

                    result.output = result.output.Trim();
                    result.code = essProcess.ExitCode;
                }
                catch (Exception e)
                {
                    result.exception = e;
                }

                return result;
            }

            private static string ParamValueString(string param, string value)
            {
                return $"-{param}=\"{value}\"";
            }

            #if QUICKSEARCH_EXAMPLES
            [UsedImplicitly, InitializeOnLoadMethod]
            #endif
            private static void BuildIndex()
            {
                s_StopEvent = new AutoResetEvent(false);

                if (!File.Exists(ess_exe))
                    return;

                var localIndexPath = indexPath;
                var localDataPath = Application.dataPath;
                var thread = new Thread(() =>
                {
                    var result = new RunResult { code = 0 };
                    // Create index if not exists
                    if (!Directory.Exists(localIndexPath))
                        result = RunESS("create", ParamValueString("index", localIndexPath), ParamValueString("root", localDataPath), 
                                        ParamValueString("include", "*.cs,*.txt,*.uss,*.asmdef,*.shader,*.json"),
                                        ParamValueString("exclude", "*.meta"));

                    if (result.code != 0)
                    {
                        UnityEngine.Debug.LogError($"[{result.code}] Failed to create ESS index at {localIndexPath}\n\n" + result.output);
                        if (result.exception != null)
                            UnityEngine.Debug.LogException(result.exception);
                        return;
                    }

                    // Update index
                    if (RunESS("update", ParamValueString("index", localIndexPath)).code != 0)
                        result = RunESS("check", ParamValueString("index", localIndexPath), "-fix");

                    if (result.code != 0)
                    {
                        UnityEngine.Debug.LogError($"[{result.code}] Failed fix the ESS index at {localIndexPath}\n\n" + result.output);
                        if (result.exception != null)
                            UnityEngine.Debug.LogException(result.exception);
                        return;
                    }

                    UnityEngine.Debug.Log("ESS index ready");
                    s_BuildingIndex = false;
                });
                thread.Start();
            }
        }
    }
}
