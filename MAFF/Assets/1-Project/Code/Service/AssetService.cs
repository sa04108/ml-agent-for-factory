using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Merlin
{
    public class AssetService : ServiceBehaviour
    {
        private const long timeout = 5000;
        private List<string> downloadingAssets = new();

        public T Load<T>(string keyOrPath) where T : Object
        {
            if (string.IsNullOrEmpty(keyOrPath))
            {
                Log.Warning("The Key or Path of asset is null or empty");
                return null;
            }

            if (downloadingAssets.Contains(keyOrPath))
            {
                Log.Warning($"Can not load asset {keyOrPath} synchronously while downloading");
                return null;
            }

            T asset = null;

            if (IsAddressablesKey(keyOrPath))
            {
                asset = Addressables.LoadAssetAsync<T>(keyOrPath).WaitForCompletion();
            }
            else
            {
                asset = Resources.Load<T>(GetResourcesPath(keyOrPath));
            }

            if (asset == null)
            {
                Log.Warning($"Failed to load asset {keyOrPath} from both Addressables and Resources.");
            }

            return asset;
        }

        public IAsyncOperationWrapper<T> LoadAsync<T>(string keyOrPath) where T : Object
        {
            if (string.IsNullOrEmpty(keyOrPath))
            {
                Log.Warning("The Key or Path of asset is null or empty");
                return null;
            }

            if (downloadingAssets.Contains(keyOrPath))
            {
                Log.Warning($"Can not load asset {keyOrPath} synchronously while downloading");
                return null;
            }

            IAsyncOperationWrapper<T> op;

            if (IsAddressablesKey(keyOrPath))
            {
                op = new AsyncOperationHandleWrapper<T>(this, Addressables.LoadAssetAsync<T>(keyOrPath));
            }
            else
            {
                op = new ResourceRequestWrapper<T>(this, Resources.LoadAsync<T>(GetResourcesPath(keyOrPath)));
            }

            op.OnComplete(_ =>
            {
                if (!op.Succeeded)
                {
                    Log.Warning($"Failed to load asset {keyOrPath} from both Resources and Addressables.");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        public GameObject Instantiate(string keyOrPath, Transform parent = null)
        {
            return Instantiate<GameObject>(keyOrPath, parent);
        }

        public T Instantiate<T>(string keyOrPath, Transform parent = null) where T : Object
        {
            if (string.IsNullOrEmpty(keyOrPath))
            {
                Log.Warning("The Key or Path of asset is null or empty");
                return null;
            }

            if (downloadingAssets.Contains(keyOrPath))
            {
                Log.Warning($"Can not instantiate asset {keyOrPath} synchronously while downloading");
                return null;
            }

            T asset = null;
            if (IsAddressablesKey(keyOrPath))
            {
                var go = Addressables.InstantiateAsync(keyOrPath, parent).WaitForCompletion();

                if (typeof(T) == typeof(GameObject))
                {
                    asset = go as T;
                }
                else
                {
                    asset = go?.GetComponent<T>();
                }
            }
            else
            {
                var loaded = Resources.Load<T>(GetResourcesPath(keyOrPath));
                if (loaded != null)
                {
                    asset = Instantiate(loaded, parent);
                }
            }

            if (asset == null)
            {
                Log.Warning($"Failed to instantiate asset {keyOrPath} from both Addressables and Resources.");
            }

            return asset;
        }

        public IAsyncOperationWrapper<GameObject> InstantiateAsync(string keyOrPath, Transform parent = null)
        {
            if (string.IsNullOrEmpty(keyOrPath))
            {
                Log.Warning("The Key or Path of asset is null or empty");
                return null;
            }

            if (downloadingAssets.Contains(keyOrPath))
            {
                Log.Warning($"Can not instantiate asset {keyOrPath} synchronously while downloading");
                return null;
            }

            IAsyncOperationWrapper<GameObject> op;

            if (IsAddressablesKey(keyOrPath))
            {
                op = new AsyncOperationHandleWrapper<GameObject>(this, Addressables.InstantiateAsync(keyOrPath, parent));
            }
            else
            {
                op = new ResourceRequestWrapper<GameObject>(this, Resources.LoadAsync<GameObject>(GetResourcesPath(keyOrPath)));
                op.OnComplete(asset => Instantiate(asset, parent));
            }

            op.OnComplete(_ =>
            {
                if (!op.Succeeded)
                {
                    Log.Warning($"Failed to instantiate asset {keyOrPath} from both Resources and Addressables.");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        public AsyncOperationHandleWrapper<SceneInstance> LoadSceneAsync(string key)
        {
            var handle = Addressables.LoadSceneAsync(key);
            var op = new AsyncOperationHandleWrapper<SceneInstance>(this, handle);
            op.OnComplete(_ =>
            {
                if (op.Succeeded)
                {
                    Log.Info($"Successfully loaded scene: {handle.Result.Scene.name}");
                }
                else
                {
                    Log.Warning($"Failed to load scene: {handle.Result.Scene.name}");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        /// <summary>
        /// Addressables.LoadAssetAsync와는 별개로 에셋을 런타임의 특정 시점에 다운받기
        /// 다운로드 동안 Load를 할 수 없다.
        /// </summary>
        /// <param name="key"></param>

        public AsyncOperationHandleWrapper<long> CheckForDownload(string key)
        {
            var handle = Addressables.GetDownloadSizeAsync(key);
            var op = new AsyncOperationHandleWrapper<long>(this, handle);
            op.OnComplete(_ =>
            {
                if (op.Succeeded)
                {
                    Log.Info($"Total Download size: {handle.Result} for {key}");
                }
                else
                {
                    Log.Warning($"Failed to check download size for {key}");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        public AsyncOperationHandleWrapper<long> CheckForDownload(string[] keys)
        {
            var handle = Addressables.GetDownloadSizeAsync(keys);
            var op = new AsyncOperationHandleWrapper<long>(this, handle);
            op.OnComplete(_ =>
            {
                if (op.Succeeded)
                {
                    Log.Info($"Total Download size: {handle.Result}");
                }
                else
                {
                    Log.Warning($"Failed to check download size");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        public AsyncOperationHandleWrapper Download(string key)
        {
            downloadingAssets.Add(key);

            var handle = Addressables.DownloadDependenciesAsync(key, true);
            var op = new AsyncOperationHandleWrapper(this, handle);
            op.OnComplete(() =>
            {
                downloadingAssets.Remove(key);

                if (op.Succeeded)
                {
                    Log.Info($"{key} successfully downloaded");
                }
                else
                {
                    Log.Warning($"Failed to download {key}");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        public AsyncOperationHandleWrapper Download(string[] keys, Addressables.MergeMode mergeMode)
        {
            downloadingAssets.AddRange(keys);

            var handle = Addressables.DownloadDependenciesAsync(keys, mergeMode, true);
            var op = new AsyncOperationHandleWrapper(this, handle);
            op.OnComplete(() =>
            {
                keys.ForEach(key => downloadingAssets.Remove(key));

                if (op.Succeeded)
                {
                    Log.Info("Successfully downloaded");
                }
                else
                {
                    Log.Warning("Failed to download");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        public AsyncOperationHandleWrapper<List<string>> CheckForCatalogUpdates()
        {
            var handle = Addressables.CheckForCatalogUpdates();
            var op = new AsyncOperationHandleWrapper<List<string>>(this, handle);
            op.OnComplete(() =>
            {
                if (op.Succeeded)
                {
                    Log.Info($"{handle.Result?.Count} catalogs found to be updated.");
                }
                else
                {
                    Log.Warning($"Failed to check catalog updates");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        public AsyncOperationHandleWrapper<List<IResourceLocator>> UpdateCatalogs(List<string> catalogs = null)
        {
            var handle = Addressables.UpdateCatalogs(catalogs);
            var op = new AsyncOperationHandleWrapper<List<IResourceLocator>>(this, handle);
            op.OnComplete(() =>
            {
                if (op.Succeeded)
                {
                    Log.Info($"{handle.Result?.Count} catalogs updated");
                }
                else
                {
                    Log.Warning("Failed to update catalogs");
                }
            });

            StartCoroutine(CoOperateAsync(op));

            return op;
        }

        public void ClearCache()
        {
            Caching.ClearCache();
            Addressables.CleanBundleCache();
        }

        private IEnumerator CoOperateAsync(IAsyncOperation op)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!op.IsDone)
            {
                if (stopwatch.ElapsedMilliseconds > timeout)
                {
                    Log.Warning($"{op} takes too much time, so break the loop");
                    break;
                }

                yield return null;
            }

            stopwatch.Stop();
        }

        // Addressable인지 아닌지 동기적으로 빠르게 확인하는 방법은 현재로서는 key값의 경로를 보고 판단하는 법밖에는 없음
        // Addressables.LoadResourceLocationsAsync 함수로 해당하는 에셋들의 경로정보를 가져올 수 있지만 느리고
        // Resources.Load를 한 번 해보고 없다라고 판단하는 것은 훨씬 더 느림.
        private bool IsAddressablesKey(string key)
        {
            if (key.Contains(C.Asset.kAddressableAssetPath))
            {
                return true;
            }

            return false;
        }

        private string GetResourcesPath(string path)
        {
            string formatted = path;

            int index = path.LastIndexOf("Resources/");
            if (index != -1)
            {
                formatted = path.Substring(index + "Resources/".Length);
            }

            int extensionIndex = formatted.LastIndexOf('.');
            if (extensionIndex >= 0)
            {
                formatted = formatted.Substring(0, extensionIndex);
            }

            return formatted;
        }
    }
}