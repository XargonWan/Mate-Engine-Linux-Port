// This file mirrors Assets/Plugins/SyntheticHeart/SyntheticHeartIntegrationRunner.cs for packaging.
using System;
using System.Linq;
using UnityEngine;

namespace SyntheticHeart
{
    public sealed class SyntheticHeartIntegrationRunner : MonoBehaviour
    {
        private const string ArgPrefix = "--synth-integration-test=";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            string[] args = Environment.GetCommandLineArgs();
            string target = args.FirstOrDefault(a => a.StartsWith(ArgPrefix, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(target))
                return;

            string url = target.Substring(ArgPrefix.Length).Trim();
            if (string.IsNullOrWhiteSpace(url))
                return;

            var runnerObject = new GameObject("SyntheticHeartIntegrationRunner");
            DontDestroyOnLoad(runnerObject);
            var runner = runnerObject.AddComponent<SyntheticHeartIntegrationRunner>();
            runner.StartIntegrationTest(url);
        }

        private async void StartIntegrationTest(string baseUrl)
        {
            int exitCode = 1;
            try
            {
                var client = new SyntheticHeartClient(baseUrl);
                var response = await client.GetPromptOverrideAsync();
                if (response != null && !string.IsNullOrWhiteSpace(response.injection))
                    exitCode = 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SyntheticHeartIntegrationRunner] Integration test failed: {ex}");
            }

            ExitWithCode(exitCode);
        }

        private static void ExitWithCode(int code)
        {
            try
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.Exit(code);
#else
                Application.Quit(code);
#endif
            }
            catch (Exception)
            {
                Environment.Exit(code);
            }
        }
    }
}
