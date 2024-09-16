#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Base.GitIntegration
{
    [InitializeOnLoad]
    public class SmartMergeRegistrar
    {
        private const string SmartMergeRegistrarEditorPrefsKey = "smart_merge_installed";
        private const int VERSION = 1;
        private static readonly string _versionKey = $"{VERSION}_{Application.unityVersion}";

        private static string ExecuteGitWithParams(string param)
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo("git")
            {
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = new System.Diagnostics.Process();
            process.StartInfo = processInfo;
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = param;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception(process.StandardError.ReadLine());

            return process.StandardOutput.ReadLine();
        }

        [MenuItem("Tools/Git/SmartMerge registration")]
        private static void SmartMergeRegister()
        {
            try
            {
                var unityYamlMergePath = EditorApplication.applicationContentsPath + "/Tools" + "/UnityYAMLMerge.exe";
                ExecuteGitWithParams("config merge.unityyamlmerge.name \"Unity SmartMerge (UnityYamlMerge)\"");
                ExecuteGitWithParams($"config merge.unityyamlmerge.driver \"\\\"{unityYamlMergePath}\\\" merge -h -p --force --fallback none %O %B %A %A\"");
                ExecuteGitWithParams("config merge.unityyamlmerge.recursive binary");
                EditorPrefs.SetString(SmartMergeRegistrarEditorPrefsKey, _versionKey);
                Debug.Log($"Successful registered UnityYAMLMerge with path {unityYamlMergePath}");
            }
            catch (Exception e)
            {
                Debug.Log($"Fail to register UnityYAMLMerge with error: {e}");
            }
        }

        //Unity calls the static constructor when the engine opens
        static SmartMergeRegistrar()
        {
            var installedVersionKey = EditorPrefs.GetString(SmartMergeRegistrarEditorPrefsKey);
            if (installedVersionKey != _versionKey)
                SmartMergeRegister();
        }
    }
}
#endif