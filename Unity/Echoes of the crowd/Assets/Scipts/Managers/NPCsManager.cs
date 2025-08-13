using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCsManager : MonoBehaviour
{
    #region Fields
    public List<NPC> npcs; // List to hold all NPCs in the game
    private string jsonFilePath = "NPC/NPCs.json";
    #endregion

    // This class will manage the creation of the NPCs in the game 
    #region Singleton Setup
    public static NPCsManager Instance { get; private set; }
    private void Awake()
    {
        // If another instance exists, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Assign instance
        Instance = this;

        // Keep this object alive between scenes
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public async void LoadNPCs()
    {
        NPCList npcList = await JsonLoader.LoadFromStreamingAssets(jsonFilePath);
        if (npcList != null)
        {
            foreach (NPCData npcData in npcList.npcs)
                CreateNPC(npcData);
        }
        else
            Debug.LogError("Failed to load NPCs from JSON.");
    }

    private void CreateNPC(NPCData NPCs)
    {
        npcs.Add(new NPC(
            NPCs.id, NPCs.name, NPCs.gender, NPCs.age, NPCs.culture,
            NPC.NPC_Appearance(NPCs.appearance.hair_color,NPCs.appearance.eye_color,NPCs.appearance.height_cm,NPCs.appearance.build),
            NPC.NPC_Personality(NPCs.personality.openness, NPCs.personality.conscientiousness, NPCs.personality.extraversion, NPCs.personality.agreeableness, NPCs.personality.neuroticism),
            NPCs.traits, NPCs.brief_history, NPCs.portrait, NPCs.goal, NPCs.occupation
        ));
    }

}
