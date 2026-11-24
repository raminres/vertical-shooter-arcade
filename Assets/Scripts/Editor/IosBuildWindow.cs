using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;

namespace VerticalShooter.Editor
{
    public sealed class IosBuildWindow : EditorWindow
    {
        private Label preview;
        private Label status;

        [MenuItem("Tools/Build/iOS Build")]
        public static void Open()
        {
            GetWindow<IosBuildWindow>("iOS Build");
        }

        public void CreateGUI()
        {
            rootVisualElement.style.paddingLeft = 12;
            rootVisualElement.style.paddingRight = 12;
            rootVisualElement.style.paddingTop = 12;

            preview = new Label();
            preview.style.whiteSpace = WhiteSpace.Normal;
            rootVisualElement.Add(preview);

            var buildButton = new Button(Build) { text = "Build iOS Xcode Project" };
            rootVisualElement.Add(buildButton);

            status = new Label();
            status.style.whiteSpace = WhiteSpace.Normal;
            rootVisualElement.Add(status);
            RefreshPreview();
        }

        private void OnFocus()
        {
            if (preview != null)
                RefreshPreview();
        }

        private void RefreshPreview()
        {
            try
            {
                string folder = IosBuildNumbers.NextFolder(Path.GetFullPath("Builds"));
                string version = IosBuildNumbers.NextVersion(PlayerSettings.bundleVersion);
                string build = IosBuildNumbers.NextBuildNumber(PlayerSettings.iOS.buildNumber);
                preview.text = $"Output  Builds/{Path.GetFileName(folder)}\nVersion  {version}\niOS build  {build}\n\nA build attempt saves these numbers in Player Settings.";
            }
            catch (Exception exception)
            {
                preview.text = exception.Message;
            }
        }

        private void Build()
        {
            try
            {
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
                    throw new InvalidOperationException("Select iOS as the active build target in Build Profiles before building.");
                if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                    throw new InvalidOperationException("The iOS Build Support module is unavailable.");

                string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
                if (scenes.Length == 0)
                    throw new InvalidOperationException("Add an enabled scene to the build settings.");

                string version = IosBuildNumbers.NextVersion(PlayerSettings.bundleVersion);
                string build = IosBuildNumbers.NextBuildNumber(PlayerSettings.iOS.buildNumber);
                string folder = IosBuildNumbers.ReserveFolder(Path.GetFullPath("Builds"));

                PlayerSettings.bundleVersion = version;
                PlayerSettings.iOS.buildNumber = build;
                AssetDatabase.SaveAssets();

                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    target = BuildTarget.iOS,
                    locationPathName = folder,
                    options = BuildOptions.None
                });

                status.text = report.summary.result == BuildResult.Succeeded
                    ? $"Build succeeded.\n{folder}"
                    : $"Build {report.summary.result}. See the Console for details.\n{folder}";
                RefreshPreview();
            }
            catch (Exception exception)
            {
                status.text = $"Build could not finish. {exception.Message}";
                Debug.LogException(exception);
                RefreshPreview();
            }
        }
    }

    public static class IosBuildNumbers
    {
        public static string NextVersion(string current)
        {
            string[] parts = current.Split('.');
            if (parts.Length != 3 || parts.Any(part => !IsDigits(part)) ||
                !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int patch) ||
                patch == int.MaxValue)
                throw new FormatException("Set the Player version to three numeric components, such as 0.1.0.");
            return $"{parts[0]}.{parts[1]}.{patch + 1}";
        }

        public static string NextBuildNumber(string current)
        {
            if (!IsDigits(current) ||
                !int.TryParse(current, NumberStyles.None, CultureInfo.InvariantCulture, out int number) ||
                number == int.MaxValue)
                throw new FormatException("Set the iOS build number to a non-negative integer.");
            return (number + 1).ToString(CultureInfo.InvariantCulture);
        }

        public static string NextFolder(string root)
        {
            int largest = -1;
            if (Directory.Exists(root))
            {
                foreach (string path in Directory.EnumerateFileSystemEntries(root, "_build*"))
                {
                    string suffix = Path.GetFileName(path).Substring("_build".Length);
                    if (IsDigits(suffix) && int.TryParse(suffix, NumberStyles.None,
                            CultureInfo.InvariantCulture, out int number))
                        largest = Math.Max(largest, number);
                }
            }
            return Path.Combine(root, "_build" + checked(largest + 1));
        }

        public static string ReserveFolder(string root)
        {
            Directory.CreateDirectory(root);
            string folder = NextFolder(root);
            Directory.CreateDirectory(folder);
            return folder;
        }

        private static bool IsDigits(string value)
        {
            return !string.IsNullOrEmpty(value) && value.All(character => character >= '0' && character <= '9');
        }
    }
}
