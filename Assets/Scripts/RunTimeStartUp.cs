using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class RunTimeStartUp : MonoBehaviour // // SCRIPT IS A PLACEHOLDER UNTIL ASSETS ACAN BE PLACED ON AZURE
{

    public  void Awake()
    {
        StartCoroutine(CopyFileFromStreamingAssets());
    }
    public static IEnumerator CopyFileFromStreamingAssets()
    {
        string source1 = Path.Combine(Application.streamingAssetsPath, "1.json");
        string dest1 = CrosswordUtils.GetCrosswordPath(1);
        
        string source2 = Path.Combine(Application.streamingAssetsPath, "2.json");
        string dest2 = CrosswordUtils.GetCrosswordPath(2);
        
        string source3 = Path.Combine(Application.streamingAssetsPath, "3.json");
        string dest3 = CrosswordUtils.GetCrosswordPath(3);
        
        if (!File.Exists(dest1))
        {
            UnityWebRequest www = UnityWebRequest.Get(source1);
            yield return www.SendWebRequest();
        
            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(dest1, www.downloadHandler.data);
                Debug.Log("File copied to persistentDataPath: " + dest1);
            }
            else
            {
                Debug.LogError("Failed to copy file: " + www.error);
            }
        }
        
        if (!File.Exists(dest2))
        {
            UnityWebRequest www = UnityWebRequest.Get(source2);
            yield return www.SendWebRequest();
        
            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(dest2, www.downloadHandler.data);
                Debug.Log("File copied to persistentDataPath: " + dest2);
            }
            else
            {
                Debug.LogError("Failed to copy file: " + www.error);
            }
        }
        
        if (!File.Exists(dest3))
        {
            UnityWebRequest www = UnityWebRequest.Get(source3);
            yield return www.SendWebRequest();
        
            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(dest3, www.downloadHandler.data);
                Debug.Log("File copied to persistentDataPath: " + dest3);
            }
            else
            {
                Debug.LogError("Failed to copy file: " + www.error);
            }
        }
    }
}