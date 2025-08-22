using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

#region NPCs Serializer Classes
[System.Serializable]
public class NPCList
{
    public NPCData[] npcs;
}

[System.Serializable]
public class NPCData
{
    public string id;
    public string name;
    public string gender;
    public int age;
    public string culture;
    public NPC_Appearance appearance;
    public NPC_Personality personality;
    public List<string> traits;
    public string brief_history;
    public string portrait;
    public string goal;
    public string occupation;
}
[System.Serializable]
public class NPC_Appearance
{
    public string hair_color;
    public string eye_color;
    public int height_cm;
    public string build;
}
[System.Serializable]
public struct NPC_Personality
{
    public float openness;
    public float conscientiousness;
    public float extraversion;
    public float agreeableness;
    public float neuroticism;
}
#endregion

public static class JsonLoader
{
    public static async Task<NPCList> LoadFromStreamingAssets(string filePath)
    {
        string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, filePath);
        
        using (UnityWebRequest request = UnityWebRequest.Get(fullPath))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load NPCs: " + request.error);
                return null;
            }

            // Wrap JSON array in object for JsonUtility
            string jsonText = "{ \"npcs\": " + request.downloadHandler.text + " }";

            NPCList npcList = JsonUtility.FromJson<NPCList>(jsonText);
            return npcList;
        }
    }

}
