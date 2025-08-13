using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCsManager : MonoBehaviour
{
    #region Fields
    public Canvas npcCanvas;
    public GameObject npcPrefab; // Prefab for the NPC
    public List<NPC> npcs; // List to hold all NPCs in the game
    private string jsonFilePath = "NPC/NPCs.json";

    // Variables for setting prefab correctly in the canvas
    private int gridColumns = 5; // Number of columns in the grid
    private float cellWidth = 100f; // Width of each button/image
    private float cellHeight = 120f; // Height of each button/image
    private Vector2 startOffset = new Vector2(100f, -100f); // Top-left starting point
    private int npcCount = 0;

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

    #region NPC Management functions
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

    private void CreateNPC(NPCData npcData)
    {
        // Instantiate the prefab as a child of the Canvas
        GameObject npcObject = Instantiate(npcPrefab, npcCanvas.transform);

        // Optionally set the name in the hierarchy
        npcObject.name = npcData.name;

        // Reset RectTransform to default so it appears properly
        RectTransform rectTransform = npcObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            // Calculate position in grid
            int row = npcCount / gridColumns;
            int col = npcCount % gridColumns;

            float x = startOffset.x + col * cellWidth;
            float y = startOffset.y - row * cellHeight; // Negative because Y goes down in UI

            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.localScale = Vector3.one;
        }

        // Get the NPC component (on the same GameObject as the button)
        NPC npcComponent = npcObject.GetComponent<NPC>();
        if (npcComponent != null)
        {
            npcComponent.Initialize(
                npcData.id,
                npcData.name,
                npcData.gender,
                npcData.age,
                npcData.culture,
                new NPC.NPC_Appearance(
                    npcData.appearance.hair_color,
                    npcData.appearance.eye_color,
                    npcData.appearance.height_cm,
                    npcData.appearance.build
                ),
                new NPC.NPC_Personality(
                    npcData.personality.openness,
                    npcData.personality.conscientiousness,
                    npcData.personality.extraversion,
                    npcData.personality.agreeableness,
                    npcData.personality.neuroticism
                ),
                npcData.traits,
                npcData.brief_history,
                npcData.portrait,
                npcData.goal,
                npcData.occupation
            );

            // Add to list if needed
            npcs.Add(npcComponent);

            npcCount++;
        }
        else
        {
            Debug.LogError("Prefab is missing NPC component!");
        }
    }
    #endregion
}
