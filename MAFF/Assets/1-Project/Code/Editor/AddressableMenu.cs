using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Merlin
{
    public class AddressableMenu : AssetPostprocessor
    {
        private static string BuildPath
        {
            get
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                var path = settings.profileSettings.GetValueById(settings.activeProfileId, settings.RemoteCatalogBuildPath.Id);
                var projectPath = Directory.GetParent(Application.dataPath).FullName;
                path = Path.Combine(projectPath, path).Replace("/[BuildTarget]", "");

                return path;
            }
        }

        public static void RunBuildPipeline()
        {
            AssetDatabase.Refresh();
            Debug.Log("Assets imported");
            Build();
            Debug.Log("Assets build completed");
        }

        [MenuItem("Addressables/Build", false, 0)]
        public static void Build()
        {
            AddressableAssetSettings.BuildPlayerContent();
        }

        [MenuItem("Addressables/Clean Build", false, 0)]
        public static void CleanBuild()
        {
            // Remote Build Path 제거
            if (Directory.Exists(BuildPath))
            {
                Directory.Delete(BuildPath, true);
            }

            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();
        }

        [MenuItem("Addressables/Clear Bundle Cache", false, 100)]
        public static void ClearCache()
        {
            // 다운로드 받은 모든 번들 캐시를 제거합니다.
            Caching.ClearCache();
            Addressables.CleanBundleCache();

            Debug.Log("All bundle cache removed");
        }

        [MenuItem("Addressables/Open Build File Path", false, 100)]
        public static void OpenFilePath()
        {
            var path = BuildPath;

#if UNITY_EDITOR_WIN
            Debug.Log($"Open path {path}");
            System.Diagnostics.Process.Start("explorer.exe", path);
#else
            Debug.Log($"Not implemented unless platform is Windows");
#endif
        }

        private const string autoAssignBundleMenu = "Addressables/Assign Bundle Automatically";
        private const string autoAssignBundlePref = "AutoBundleAssignmentEnabled";

        [MenuItem(autoAssignBundleMenu, true, 200)]
        private static bool InitAutoAssignBundle()
        {
            bool currentState = EditorPrefs.GetBool(autoAssignBundlePref, true);
            Menu.SetChecked(autoAssignBundleMenu, currentState);

            return true;
        }

        [MenuItem(autoAssignBundleMenu, false, 200)]
        public static void ToggleAutoAssignBundle()
        {
            bool currentState = EditorPrefs.GetBool(autoAssignBundlePref, true);
            currentState = !currentState;
            EditorPrefs.SetBool(autoAssignBundlePref, currentState);
            Menu.SetChecked(autoAssignBundleMenu, currentState);
        }

        // AssetImport시 번들 자동 지정
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool autoAssign = EditorPrefs.GetBool(autoAssignBundlePref, true);
            if (!autoAssign)
                return;

            // Addressable Asset Settings 가져오기
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings를 찾을 수 없습니다. 먼저 Addressables 설정을 생성해주세요.");
                return;
            }

            // 자동 할당을 위한 그룹을 찾거나 생성
            AddressableAssetGroup assetGroup = settings.DefaultGroup;
            if (assetGroup == null)
            {
                assetGroup = settings.CreateGroup("AddressableAssets", true, false, false, null, typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema));
            }

            // 임포트된 에셋 중 폴더 이름이 "__"로 시작하는 경우 자동 지정
            foreach (string assetPath in importedAssets)
            {
                // 에셋 경로의 상위 폴더 이름 확인
                string directory = Path.GetDirectoryName(assetPath);
                if (string.IsNullOrEmpty(directory))
                    continue;

                // 마지막 폴더 이름 추출
                string folderName = new DirectoryInfo(directory).Name;
                if (folderName.StartsWith("__"))
                {
                    string guid = AssetDatabase.AssetPathToGUID(directory);
                    // 이미 Addressable로 지정된 폴더인지 확인
                    var entry = settings.FindAssetEntry(guid);
                    if (entry == null)
                    {
                        settings.CreateOrMoveEntry(guid, assetGroup);
                        Debug.Log($"'{directory}'가 자동으로 Addressable로 지정되었습니다.");
                    }
                }
            }
        }
    }
}