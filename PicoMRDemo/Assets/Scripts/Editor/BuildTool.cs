/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PicoMRDemo.Editor
{
    public static class BuildTool
    {
        private const string MENU = "Tools/BuildTool/";
        private const int PRIO_BASE = 1000;

        public const string TAG = "BuildTool";
        public const string LOGTAG = TAG + ": ";

        public static string BaseOutputFilename = "PicoMRDemo";

        private static BuilderContext CurrentBuilderContext = new BuilderContext();

        [MenuItem(MENU + "BuildAndroid", false, PRIO_BASE)]
        public static void BuildAndroid()
        {
            BuildPlayer(BuildTarget.Android);
        }

        [MenuItem(MENU + "BuildIOS", false, PRIO_BASE)]
        public static void BuildIOS()
        {
            BuildPlayer(BuildTarget.iOS);
        }

        private static void EditBuildSetting_Development_UpdateMenuItem() {
            var enabled = EditorUserBuildSettings.development;
            Menu.SetChecked(MENU + "Set Development Build/Development: [On]", enabled);
            Menu.SetChecked(MENU + "Set Development Build/Development: [Off]", !enabled);
        }
        [MenuItem(MENU + "Set Development Build/Development: [On]", true)]
        [MenuItem(MENU + "Set Development Build/Development: [Off]", true)]
        private static bool EditBuildSetting_Development_Validate() {
            EditBuildSetting_Development_UpdateMenuItem();
            return true;
        }
        [MenuItem(MENU + "Set Development Build/Development: [On]", false, PRIO_BASE + 50)]
        private static void EditBuildSetting_Development_On() {
            EditorUserBuildSettings.development = true;
            EditBuildSetting_Development_UpdateMenuItem();
        }
        [MenuItem(MENU + "Set Development Build/Development: [Off]", false, PRIO_BASE + 50)]
        private static void EditBuildSetting_Development_Off() {
            EditorUserBuildSettings.development = false;
            EditBuildSetting_Development_UpdateMenuItem();
        }


        public static void BuildPlayer(BuildTarget buildTarget)
        {
            CurrentBuilderContext = new BuilderContext();
            var context = CurrentBuilderContext;

            context.JobName = context.MakeBuilderName(buildTarget);
            context.BuildTarget = buildTarget;
            context.VersionNumber = PlayerSettings.bundleVersion;
            context.IsDevelopment = EditorUserBuildSettings.development;
            context.CustomFilenameSuffix = "";
            context.ExtraFilenameSuffix = "";
            context.SetupOutputFile(BaseOutputFilename);

            _DoBuildPlayer(context);
        }

        public static void CommandLineBuildPlayer()
        {
            CurrentBuilderContext = new BuilderContext();
            var context = CurrentBuilderContext;
            var buildTarget = GetArg_BuildTarget();
            if (string.IsNullOrEmpty(buildTarget))
            {
                context.Cancel("-buildTarget arg is empty from command line args!");
            }

            context.JobName = context.MakeBuilderName(buildTarget);
            context.BuildTarget = context.MakeBuildTarget(buildTarget);
            context.VersionNumber = GetArg_BuildVersion();
            context.IsDevelopment = GetArg_IsDevelopment();
            context.CustomFilenameSuffix = GetArg_CustomFilenameSuffix();
            context.ExtraFilenameSuffix = GetArg_ExtraFilenameSuffix();
            context.SetupOutputFile(BaseOutputFilename);

            _DoBuildPlayer(context);
        }

        class BuilderContext
        {
            public bool CanThrowException = true;
            // === setup ===
            public string JobName = "";
            public BuildTarget BuildTarget = BuildTarget.NoTarget;
            public string VersionNumber;
            public bool IsDevelopment;
            public string OutputDir;
            public string OutputFilename;
            public string CustomFilenameSuffix;
            public string ExtraFilenameSuffix;
            public string OutputFilePath;

            public void Cancel(string cancelMsg = "", bool isError = true)
            {
                CancelledMsg = isError ? (cancelMsg ?? "Unknown error") : "Unknown reason.";
                Debug.LogWarning(LOGTAG + $"{JobName} cancelled: {cancelMsg}");
                EditorUtility.DisplayDialog(TAG, $"{JobName} cancelled:\n" + cancelMsg, "OK");
                if (isError && CanThrowException)
                {
                    throw new InvalidOperationException(LOGTAG + cancelMsg);
                }
            }

            public bool IsCancelled { get; private set; }
            public string CancelledMsg { get; private set; }

            public string MakeBuilderName(BuildTarget target)
            {
                string name = $"Build{target}";
                return name;
            }

            public string MakeBuilderName(string buildTarget)
            {
                string name = $"Build{buildTarget}";
                return name;
            }

            public BuildTarget MakeBuildTarget(string buildTarget)
            {
                switch (buildTarget)
                {
                    case "Android":
                        return BuildTarget.Android;
                    case "iOS":
                        return BuildTarget.iOS;
                    case "Win64":
                    case "StandaloneWindows64":
                        return BuildTarget.StandaloneWindows64;
                    case "MacOS":
                    case "StandaloneOSX":
                        return BuildTarget.StandaloneOSX;
                    default:
                        throw new InvalidOperationException(LOGTAG + $"buildTarget: \"{buildTarget}\" not supported in BuildTool!");
                }
            }

            public string MakeOutputFilenameExt()
            {
                switch (BuildTarget)
                {
                    case BuildTarget.Android:
                        return ".apk";
                    case BuildTarget.iOS:
                        return ".ipa";
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        return ".exe";
                    case BuildTarget.StandaloneOSX:
                        return ".app";
                    default:
                        return "";
                }
            }

            private string SetupOutputDir()
            {
                if (BuildTarget == BuildTarget.NoTarget)
                    throw new InvalidOperationException(LOGTAG + $"invalid buildTarget: \"{BuildTarget}\"!");
                OutputDir = $"Builds/{BuildTarget}";
                Debug.Log(LOGTAG + $"build: {nameof(OutputDir)}: \"{OutputDir}\"");
                return OutputDir;
            }

            private string SetupOutputFilename(string baseOutputFilename)
            {
                var filename = baseOutputFilename;
                var ext = MakeOutputFilenameExt();

                if (string.IsNullOrEmpty(CustomFilenameSuffix))
                {
                    if (!string.IsNullOrEmpty(VersionNumber))
                    {
                        filename += $"-v{VersionNumber}";
                        Debug.Log(LOGTAG + $"build: {nameof(VersionNumber)}: \"{VersionNumber}\"");
                    }

                    if (IsDevelopment)
                    {
                        filename += "-Dev";
                        Debug.Log(LOGTAG + $"build: {nameof(IsDevelopment)}: \"{IsDevelopment}\"");
                    }
                }
                else
                {
                    filename += CustomFilenameSuffix;
                }

                if (!string.IsNullOrEmpty(ExtraFilenameSuffix))
                {
                    filename += ExtraFilenameSuffix;
                }

                if (!string.IsNullOrEmpty(ext))
                {
                    filename += ext;
                }

                OutputFilename = filename;
                Debug.Log(LOGTAG + $"build: {nameof(OutputFilename)}: \"{OutputFilename}\"");
                return OutputFilename;
            }

            public string SetupOutputFile(string baseOutputFilename)
            {
                SetupOutputDir();
                SetupOutputFilename(baseOutputFilename);
                OutputFilePath = OutputDir + "/" + OutputFilename;
                Debug.Log(LOGTAG + $"build: {nameof(OutputFilePath)}: \"{OutputFilePath}\"");
                return OutputFilePath;
            }
        }

        private static void FixBuildPipelineOptions(BuilderContext context)
        {
            if (context.BuildTarget == BuildTarget.Android)
            {
                var exportAndroidProj = EditorUserBuildSettings.exportAsGoogleAndroidProject;
                if (exportAndroidProj)
                {
                    Debug.LogWarning(LOGTAG + "EditorUserBuildSettings.exportAsGoogleAndroidProject was true. Now fix it to: false");
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                }
            }

            if (!string.IsNullOrEmpty(context.VersionNumber) && context.VersionNumber != PlayerSettings.bundleVersion)
            {
                var preVal = PlayerSettings.bundleVersion;
                var newVal = context.VersionNumber;
                Debug.LogWarning(LOGTAG + $"PlayerSettings.bundleVersion was {preVal}. Now set to: {newVal}");
                PlayerSettings.bundleVersion = newVal;
            }

            if (context.IsDevelopment != EditorUserBuildSettings.development)
            {
                var preVal = EditorUserBuildSettings.development;
                var newVal = context.IsDevelopment;
                Debug.LogWarning(LOGTAG + $"EditorUserBuildSettings.development was {preVal}. Now set to: {newVal}");
                EditorUserBuildSettings.development = newVal;
            }
        }

        private static string GetArg_BuildTarget()
        {
            return GetCommandLineArgValue("-buildTarget");
        }

        private static string GetArg_BuildVersion()
        {
            return GetCommandLineArgValue("-buildVersion");
        }

        private static bool GetArg_IsDevelopment()
        {
            return HasCommandLineArg("-buildDevelopment");
        }

        private static string GetArg_CustomFilenameSuffix()
        {
            return GetCommandLineArgValue("-customFilenameSuffix");
        }

        private static string GetArg_ExtraFilenameSuffix()
        {
            return GetCommandLineArgValue("-extraFilenameSuffix");
        }

        private static bool HasCommandLineArg(string argName)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == argName)
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetCommandLineArgValue(string argName)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            string value = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == argName && i + 1 < args.Length)
                {
                    value = args[i + 1];
                    break;
                }
            }
            value = value.Trim();
            Debug.Log("GetCommandLineArg - argName: " + argName + ", value: " + value);
            return value;
        }

        public static void CreateDirectory(string dirPath)
        {
            if (!System.IO.Directory.Exists(dirPath))
            {
                Debug.Log(LOGTAG + "CreateDirectory: " + dirPath);
                System.IO.Directory.CreateDirectory(dirPath);
            }
        }

        private static bool RemoveFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                Debug.Log(LOGTAG + "Delete file:" + filePath);
                System.IO.File.Delete(filePath);
                return true;
            }

            var dir = new System.IO.DirectoryInfo(filePath);
            if (dir.Exists)
            {
                Debug.Log(LOGTAG + "Delete dir:" + filePath);
                dir.Delete(true);
            }
            return false;
        }

        private static BuildOptions GetBuildPipelineOptions()
        {
            var options = BuildOptions.None;
            var development = EditorUserBuildSettings.development;
            options = development ? (options | BuildOptions.Development) : options;
            if (development)
            {
                Debug.Log(LOGTAG + "BuildOptions.Development - On.");
            }

            return options;
        }

        public static string[] GetBuildScenes()
        {
            List<string> names = new List<string>();
            int i = 0;
            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
            {
                if (e == null)
                    continue;

                if (e.enabled)
                {
                    Debug.Log(LOGTAG + $"Build scene #{i}: " + e.path);
                    names.Add(e.path);
                    i++;
                }
            }
            return names.ToArray();
        }


        /// <summary>
        /// build
        /// </summary>
        /// <returns>build output path</returns>
        private static string _DoBuildPlayer(BuilderContext context)
        {
            var jobName = context.JobName;
            Debug.Log(LOGTAG + $"{jobName} ...");
            if (Application.isPlaying)
            {
                context.Cancel("Cannot build during Editor Playing!");
                return "";
            }

            string outputDir = context.OutputDir;
            string outputFilename = context.OutputFilename;
            if (string.IsNullOrEmpty(outputFilename))
            {
                context.Cancel("Build output filename is null!");
                return "";
            }

            bool isQuietBuild = Application.isBatchMode;
            if (!isQuietBuild)
            {
                var ok = EditorUtility.DisplayDialog(TAG, $"Build output file:\n{outputFilename}" +
                                                          $"\n\noutput dir:\n{outputDir}",
                    "Build", "Cancel");
                if (!ok)
                {
                    context.Cancel("Dialog Cancel", false);
                    return "";
                }
            }
            else
            {
                Debug.Log(LOGTAG + $"[{jobName}] Build output apk:\n{outputFilename}" +
                          $"\noutput dir:\n{outputDir}");
            }

            CreateDirectory(outputDir);
            string outputPath = context.OutputFilePath;
            RemoveFile(outputPath);

            DateTime beginTime = DateTime.Now;
            Debug.Log(LOGTAG + $"[{jobName}] outputPath: " + outputPath);
            Debug.Log(LOGTAG + $"[{jobName}] build begin time: {beginTime:yyyy-MM-dd HH:mm:ss [zz]}");
            FixBuildPipelineOptions(context);
            var options = GetBuildPipelineOptions();
            var report = BuildPipeline.BuildPlayer(GetBuildScenes(), outputPath, BuildTarget.Android, options);

            DateTime endTime = DateTime.Now;
            TimeSpan costTime = DateTime.Now - beginTime;
            Debug.Log(LOGTAG + $"[{jobName}] build finish time: {endTime:yyyy-MM-dd HH:mm:ss [zz]}");
            Debug.Log(LOGTAG +
                      $"BuildAndroid [{jobName}] cost time: {costTime} ({costTime.TotalHours:N0}h{costTime.Minutes:0}m{costTime.Seconds:0}.{costTime.Milliseconds:000}s)");

            if (report != null)
            {
                switch (report.summary.result)
                {
                    case BuildResult.Failed:
                        Debug.LogError(LOGTAG + $"[{jobName}] Build Failed!");
                        context.Cancel("Build Failed!");
                        return "";
                    case BuildResult.Succeeded:
                        Debug.Log(LOGTAG + $"[{jobName}] Build Succeeded.");
                        break;
                }
            }

            return outputPath;
        }
    }
}
