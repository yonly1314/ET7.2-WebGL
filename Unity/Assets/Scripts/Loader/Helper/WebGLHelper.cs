
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ET
{
    public static class WebGLHelper
    {
        private static Dictionary<string, byte[]> dicAssetDatas = new();
        private static Dictionary<string,Dictionary<string, UnityEngine.Object>> resourceCache = new();
        private static string GetWebRequestPath(string asset)
        {
            var path = $"{Application.streamingAssetsPath}/{asset}";
            if (!path.Contains("://"))
            {
                path = "file://" + path;
            }

            return path;
        }


        public static async ETTask<AssetBundle> GetAssetBundle(string assetName)
        {
            assetName = assetName.ToLower();
            string dllPath = GetWebRequestPath(assetName);
            byte[] data;
            if (!dicAssetDatas.TryGetValue(dllPath, out data))
            {
                Debug.Log($"start download asset:{dllPath}");
                UnityWebRequest www = UnityWebRequest.Get(dllPath);
                UnityWebRequestAsyncOperation operation = www.SendWebRequest();
                ETTask tsc = ETTask.Create();
                operation.completed += (_) => { tsc.SetResult(); };
                await tsc;

#if UNITY_2020_1_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
#else
                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.Log(www.error);
                }
#endif
                else
                {
                    // Or retrieve results as binary data
                    data = www.downloadHandler.data;
                    Debug.Log($"dll:{assetName}  size:{data.Length}");
                    dicAssetDatas[dllPath] = data;
                }
            }
            return AssetBundle.LoadFromMemory(data);
        }

        public static async ETTask<Dictionary<string, UnityEngine.Object>> LoadBundle(string assetBundleName)
        {
            Dictionary<string, UnityEngine.Object> objects = new Dictionary<string, UnityEngine.Object>();
            UnityEngine.AssetBundle assetBundle = await GetAssetBundle(assetBundleName);
            if (!assetBundle)
            {
                return objects;
            }
            UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
            foreach (UnityEngine.Object asset in assets)
            {
                objects.Add(asset.name, asset);
            }
            assetBundle.Unload(false);
            return objects;
        }
        
        public static async ETTask<UnityEngine.Object> GetAsset(string bundleName, string prefab)
        {
            bundleName = bundleName.ToLower();
            Dictionary<string, UnityEngine.Object> objects;
            if (!resourceCache.TryGetValue(bundleName, out objects))
            {
                objects = new Dictionary<string, UnityEngine.Object>();
                UnityEngine.AssetBundle assetBundle = await GetAssetBundle(bundleName);
                UnityEngine.Object[] assets = assetBundle.LoadAllAssets();
                foreach (UnityEngine.Object asset in assets)
                {
                    objects.Add(asset.name, asset);
                }
                assetBundle.Unload(false);
                resourceCache[bundleName] = objects;
            }
            UnityEngine.Object resource = null;
            if (!objects.TryGetValue(prefab, out resource))
            {
                throw new Exception($"not found asset: {bundleName} {prefab}");
            }
            return resource;
        }
        
        public static string StringToAB(this string value)
        {
            return value + ".unity3d";
        }
    }
}