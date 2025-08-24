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
    
    [Header("Dialogue")]
    [SerializeField] private List<string> greetingMessages = new List<string>
    {
        // Friendly greetings
        "Hello, how are you?",
        "Hey there! Nice to see you.",
        "Good day! How's everything going?",
        "Hi! What's new with you?",
        "Greetings! How have you been?",
        "Hello! Lovely weather we're having.",
        "Hey! Long time no see.",
        "Good morning! How are you doing?",
        "Hi there! What brings you here?",
        "Hello! It's great to see you again.",
        "Hey! How's your day going?",
        "Greetings! What's on your mind?",
        "Hi! Nice to meet you.",
        "Hello there! How are things?",
        "Hey! What's the latest news?",
        
        // Aggressive greetings
        "What do you want?",
        "Why are you bothering me?",
        "Get out of my way!",
        "What's your problem?",
        "Leave me alone!",
        "I don't have time for this!",
        "What are you staring at?",
        "Mind your own business!",
        "I'm not in the mood for chitchat!",
        "What's so important that you need to talk to me?",
        "I don't want to hear it!",
        "Save your breath!",
        "I'm busy, can't you see?",
        "What's with the attitude?",
        "I don't need your company!",
        
        // Nervous/Anxious greetings
        "Oh! You startled me...",
        "Um... hello there...",
        "I wasn't expecting anyone...",
        "Is everything okay?",
        "You look... concerned...",
        "I hope nothing's wrong...",
        "Are you sure you want to talk to me?",
        "I'm not very good at conversations...",
        "Please don't be mad...",
        "Did I do something wrong?",
        
        // Excited/Enthusiastic greetings
        "Oh my! It's you!",
        "Fantastic! Just the person I wanted to see!",
        "Amazing! What a coincidence!",
        "Brilliant! How are you doing?",
        "Excellent! Great timing!",
        "Wonderful! I'm so glad you're here!",
        "Incredible! What brings you by?",
        "Spectacular! How's your day been?",
        "Marvelous! I was just thinking about you!",
        "Outstanding! What a pleasant surprise!",
        
        // Sarcastic greetings
        "Well, well, well... look who decided to show up.",
        "Oh great, just what I needed.",
        "Fancy meeting you here...",
        "What a delightful surprise... not.",
        "Oh joy, another conversation.",
        "How absolutely thrilling to see you.",
        "What an unexpected pleasure... said no one ever.",
        "Oh look, it's the life of the party.",
        "How wonderful... I was just thinking about how much I wanted to talk to someone.",
        "What brings you to my humble abode?",
        
        // Suspicious greetings
        "What are you up to?",
        "Why are you really here?",
        "I don't trust you...",
        "What's your angle?",
        "You seem suspicious...",
        "What do you want from me?",
        "I'm watching you...",
        "Don't try anything funny.",
        "What's the catch?",
        "You're not fooling anyone..."
    };
    
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
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            // Select two random NPCs (ensuring they are different)
            int randomIndex1 = Random.Range(0, npcs.Count);
            int randomIndex2;
            do
            {
                randomIndex2 = Random.Range(0, npcs.Count);
            } while (randomIndex2 == randomIndex1 && npcs.Count > 1);
            
            // Get a random greeting message
            string randomGreeting = GetRandomGreeting();
            DialogueManager.Instance.StartNPCtoNPCChat(npcs[randomIndex1], npcs[randomIndex2], randomGreeting);
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

    #region Dialogue Helpers
    private string GetRandomGreeting()
    {
        if (greetingMessages.Count == 0)
        {
            return "Hello, how are you?"; // Fallback message
        }
        
        int randomIndex = Random.Range(0, greetingMessages.Count);
        return greetingMessages[randomIndex];
    }
    #endregion
}


