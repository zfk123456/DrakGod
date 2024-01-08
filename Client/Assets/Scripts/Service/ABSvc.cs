using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AbSvc : MonoBehaviour
{
    public IEnumerator DownloadAssetBundle(string url, Action<AssetBundle> onDownloaded, Action<float> onProgress)
    {
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            request.SendWebRequest();

            while (!request.isDone)
            {
                float progress = request.downloadProgress;
                onProgress?.Invoke(progress);
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                onDownloaded?.Invoke(bundle);
            }
            else
            {
                Debug.LogError($"Download failed: {request.error}");
            }
        }
    }
}

