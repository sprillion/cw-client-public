using infrastructure.services.platform.core;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace infrastructure.services.platform.Editor
{
    [CustomEditor(typeof(PlatformConfig))]
    public class PlatformConfigEditor : UnityEditor.Editor
    {
        private static readonly string[] AllDefines =
        {
            "YANDEX_GAMES", "VK_PLAY", "GOOGLE_PLAY", "RU_STORE"
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Активная платформа для билда", EditorStyles.boldLabel);

            var current = DetectCurrentPlatform();
            EditorGUILayout.LabelField("Сейчас:", PlatformLabel(current), EditorStyles.helpBox);

            EditorGUILayout.Space(4);

            DrawApplyButton(Platform.Editor,      "🖥  Editor (убрать все defines)");
            DrawApplyButton(Platform.YandexGames, "🟡 Яндекс Игры  [YANDEX_GAMES]");
            DrawApplyButton(Platform.VK,          "🔵 VK Play       [VK_PLAY]");
            DrawApplyButton(Platform.GooglePlay,  "🟢 Google Play   [GOOGLE_PLAY]");
            DrawApplyButton(Platform.RuStore,     "🟠 RuStore       [RU_STORE]");
        }

        private void DrawApplyButton(Platform platform, string label)
        {
            var isCurrent = DetectCurrentPlatform() == platform;
            GUI.enabled = !isCurrent;
            if (GUILayout.Button(label))
                ApplyPlatform(platform);
            GUI.enabled = true;
        }

        private static void ApplyPlatform(Platform platform)
        {
            // Выставляем нужный BuildTarget
            var (standaloneTarget, namedTarget, define) = PlatformInfo(platform);

            // Ставим нужный define на все цели
            foreach (var named in new[] { NamedBuildTarget.Standalone, NamedBuildTarget.WebGL, NamedBuildTarget.Android })
            {
                var current = PlayerSettings.GetScriptingDefineSymbols(named);
                var symbols = new System.Collections.Generic.HashSet<string>(current.Split(';'));

                foreach (var d in AllDefines)
                    symbols.Remove(d);

                if (!string.IsNullOrEmpty(define))
                    symbols.Add(define);

                PlayerSettings.SetScriptingDefineSymbols(named, string.Join(";", symbols));
            }

            // Переключаем активный Build Target
            if (EditorUserBuildSettings.activeBuildTarget != standaloneTarget)
                EditorUserBuildSettings.SwitchActiveBuildTarget(namedTarget, standaloneTarget);

            Debug.Log($"[PlatformConfig] Платформа применена: {platform}");
        }

        private static (BuildTarget, BuildTargetGroup, string define) PlatformInfo(Platform platform) =>
            platform switch
            {
                Platform.YandexGames => (BuildTarget.WebGL,              BuildTargetGroup.WebGL,   "YANDEX_GAMES"),
                Platform.VK          => (BuildTarget.WebGL,              BuildTargetGroup.WebGL,   "VK_PLAY"),
                Platform.GooglePlay  => (BuildTarget.Android,            BuildTargetGroup.Android, "GOOGLE_PLAY"),
                Platform.RuStore     => (BuildTarget.Android,            BuildTargetGroup.Android, "RU_STORE"),
                _                    => (BuildTarget.Android, BuildTargetGroup.Android, ""),
            };

        private static Platform DetectCurrentPlatform()
        {
            var defines = PlayerSettings.GetScriptingDefineSymbols(
                NamedBuildTarget.FromBuildTargetGroup(
                    BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));

            if (defines.Contains("YANDEX_GAMES")) return Platform.YandexGames;
            if (defines.Contains("VK_PLAY"))      return Platform.VK;
            if (defines.Contains("GOOGLE_PLAY"))  return Platform.GooglePlay;
            if (defines.Contains("RU_STORE"))     return Platform.RuStore;
            return Platform.Editor;
        }

        private static string PlatformLabel(Platform p) => p switch
        {
            Platform.YandexGames => "Яндекс Игры (WebGL + YANDEX_GAMES)",
            Platform.VK          => "VK Play (WebGL + VK_PLAY)",
            Platform.GooglePlay  => "Google Play (Android + GOOGLE_PLAY)",
            Platform.RuStore     => "RuStore (Android + RU_STORE)",
            _                    => "Editor (Standalone, без платформенных defines)",
        };
    }
}
