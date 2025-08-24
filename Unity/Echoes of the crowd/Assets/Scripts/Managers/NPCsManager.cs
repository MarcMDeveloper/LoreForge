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
    
    // Data
    public List<NPC> npcs = new List<NPC>();
    private int npcCount = 0;
    
    // Object pooling for WebGL performance
    private Queue<GameObject> npcPool = new Queue<GameObject>();
    private List<GameObject> activeNPCs = new List<GameObject>();
    
    // Events
    public static event System.Action OnAllNPCsLoaded;
    
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
        LoadAllNPCsAtStart();
    }

    private async void LoadAllNPCsAtStart()
    {
        var npcList = await JsonLoader.LoadFromStreamingAssets("NPC/NPCs.json");
        if (npcList?.npcs != null)
        {
            foreach (var npcData in npcList.npcs)
            {
                CreateNPC(npcData);
            }
            OnAllNPCsLoaded?.Invoke();
        }
    }

    private void Update()
    {
        // Empty for now - can be used for future functionality
        if(Input.GetKeyDown(KeyCode.Space))
        {
            DialogueManager.Instance.StartNPCtoNPCChat(npcs[0], npcs[1], "Hello, how are you?");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            DialogueManager.Instance.DisplaySavedConversation(0);
        }
    }

    private void OnDestroy()
    {
        // Since NPCs are static, just clear references
        npcs.Clear();
        activeNPCs.Clear();
        npcPool.Clear();
    }

    #region NPC Creation Management
    private void CreateNPC(NPCData npcData)
    {
        GameObject npcObject = Instantiate(npcPrefab, npcCanvas.transform);
        
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
            npcComponent.Initialize(npcData);

            npcs.Add(npcComponent);
            npcCount++;
        }
    }
    #endregion
}


