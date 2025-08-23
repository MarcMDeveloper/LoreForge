using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCsManager : MonoBehaviour
{
    #region Fields
    [Header("References")]
    public GameObject npcCanvas;
    public GameObject npcPrefab;
    
    [Header("Grid Configuration")]
    [SerializeField] private int gridColumns = 5;
    [SerializeField] private float cellWidth = 100f;
    [SerializeField] private float cellHeight = 120f;
    [SerializeField] private Vector2 startOffset = new Vector2(100f, -100f);
    
    [Header("WebGL Optimizations")]
    [SerializeField] private int maxPoolSize = 50;
    [SerializeField] private bool useObjectPooling = true;
    
    // Data
    public List<NPC> npcs = new List<NPC>();
    private string jsonFilePath = "NPC/NPCs.json";
    private int npcCount = 0;
    
    // Object pooling for WebGL performance
    private Queue<GameObject> npcPool = new Queue<GameObject>();
    private List<GameObject> activeNPCs = new List<GameObject>();
    
    // Loading state
    private bool isLoading = false;
    
    #endregion

    #region Singleton Setup
    public static NPCsManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Start()
    {
        InitializeObjectPool();
        SetupCanvasForWebGL();
    }

    private void InitializeObjectPool()
    {
        if (!useObjectPooling) return;
        
        for (int i = 0; i < maxPoolSize; i++)
        {
            GameObject npcObject = Instantiate(npcPrefab, npcCanvas.transform);
            npcObject.SetActive(false);
            npcPool.Enqueue(npcObject);
        }
    }

    private void SetupCanvasForWebGL()
    {
        RectTransform canvasRect = npcCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.anchorMin = new Vector2(0, 0);
            canvasRect.anchorMax = new Vector2(1, 0);
            canvasRect.pivot = new Vector2(0.5f, 0f);
            canvasRect.sizeDelta = new Vector2(0, 0);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && npcs.Count > 1)
            DialogueManager.Instance.StartNPCtoNPCChat(npcs[0], npcs[1], "Hello, whats your name? And what are your last news?");
        
        // Testing keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
            TestScrollViewWithNPCs(5);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TestScrollViewWithNPCs(10);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            TestScrollViewWithNPCs(15);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            ClearAllNPCs();
    }

    #region NPC Creation Management
    public async void LoadNPCs()
    {
        if (isLoading) return;
        
        isLoading = true;
        
        NPCList npcList = await JsonLoader.LoadFromStreamingAssets(jsonFilePath);
        
        if (npcList != null)
        {
            ClearAllNPCs();
            
            foreach (NPCData npcData in npcList.npcs)
            {
                CreateNPC(npcData);
            }
            
            StartCoroutine(UpdateLayoutForWebGL());
        }
        else
        {
            Debug.LogError("Failed to load NPCs from StreamingAssets");
        }
        
        isLoading = false;
    }

    private System.Collections.IEnumerator UpdateLayoutForWebGL()
    {
        yield return new WaitForEndOfFrame();
        
        UpdateContentSize();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(npcCanvas.GetComponent<RectTransform>());
        
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(npcCanvas.GetComponent<RectTransform>());
    }

    private void UpdateContentSize()
    {
        if (npcCount == 0) return;
        
        int rows = Mathf.CeilToInt((float)npcCount / gridColumns);
        float totalHeight = Mathf.Abs(startOffset.y) + (rows * cellHeight) + 100f;
        
        RectTransform canvasRect = npcCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            Vector2 currentSize = canvasRect.sizeDelta;
            canvasRect.sizeDelta = new Vector2(currentSize.x, totalHeight);
            
            Debug.Log($"Updated content size to {canvasRect.sizeDelta}, NPCs: {npcCount}, Rows: {rows}");
        }
    }

    private void CreateNPC(NPCData npcData)
    {
        GameObject npcObject;
        
        if (useObjectPooling && npcPool.Count > 0)
        {
            npcObject = npcPool.Dequeue();
            npcObject.SetActive(true);
        }
        else
        {
            npcObject = Instantiate(npcPrefab, npcCanvas.transform);
        }
        
        activeNPCs.Add(npcObject);
        npcObject.name = npcData.name;

        RectTransform rectTransform = npcObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            int row = npcCount / gridColumns;
            int col = npcCount % gridColumns;

            float x = startOffset.x + col * cellWidth;
            float y = startOffset.y - row * cellHeight;

            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.localScale = Vector3.one;
        }

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

            npcs.Add(npcComponent);
            npcCount++;
        }
    }
    #endregion

    #region Testing and Management
    public void TestScrollViewWithNPCs(int count)
    {
        ClearAllNPCs();
        
        for (int i = 0; i < count; i++)
        {
            NPCData testData = new NPCData
            {
                id = $"test_{i}",
                name = $"Test NPC {i}",
                gender = "Unknown",
                age = 25,
                culture = "Test",
                appearance = new NPC_Appearance
                {
                    hair_color = "Brown",
                    eye_color = "Blue",
                    height_cm = 170,
                    build = "Average"
                },
                personality = new NPC_Personality
                {
                    openness = 0.5f,
                    conscientiousness = 0.5f,
                    extraversion = 0.5f,
                    agreeableness = 0.5f,
                    neuroticism = 0.5f
                },
                traits = new List<string> { "Test Trait" },
                brief_history = "Test history",
                portrait = "",
                goal = "Test goal",
                occupation = "Test occupation"
            };
            
            CreateNPC(testData);
        }
        
        StartCoroutine(UpdateLayoutForWebGL());
    }
    
    public void ClearAllNPCs()
    {
        foreach (GameObject npcObject in activeNPCs)
        {
            if (useObjectPooling)
            {
                npcObject.SetActive(false);
                npcPool.Enqueue(npcObject);
            }
            else
            {
                DestroyImmediate(npcObject);
            }
        }
        
        activeNPCs.Clear();
        npcs.Clear();
        npcCount = 0;
        
        RectTransform canvasRect = npcCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(canvasRect.sizeDelta.x, 0);
        }
    }
    #endregion
}


