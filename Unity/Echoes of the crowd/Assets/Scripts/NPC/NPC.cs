using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class NPC : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    #region Fields
    [Header("NPC Data")]
    [SerializeField] private NPCData data;
    
    [Header("UI Components")]
    public Image portraitImage;       
    public TMP_Text nameText;

    // Memory management: Static tooltip reference
    private static NPCTooltip tooltipInstance;
    private static readonly Dictionary<string, string> promptCache = new Dictionary<string, string>();
    
    private bool isInitialized = false;
    private float lastClickTime;
    private const float CLICK_COOLDOWN = 0.5f;

    #endregion

    #region Initialization
    public void Initialize(NPCData npcData)
    {
        // Error handling and validation
        if (npcData == null) return;
        if (string.IsNullOrEmpty(npcData.id)) return;
        if (npcData.personality.openness < 0f || npcData.personality.openness > 1f) return;
        if (npcData.personality.conscientiousness < 0f || npcData.personality.conscientiousness > 1f) return;
        if (npcData.personality.extraversion < 0f || npcData.personality.extraversion > 1f) return;
        if (npcData.personality.agreeableness < 0f || npcData.personality.agreeableness > 1f) return;
        if (npcData.personality.neuroticism < 0f || npcData.personality.neuroticism > 1f) return;
        
        data = npcData;
        CreateAgent();
        UpdateUI();
        isInitialized = true;
    }

    private void Start()
    {
        // Memory management: Static tooltip reference
        if (tooltipInstance == null)
            tooltipInstance = FindFirstObjectByType<NPCTooltip>();
    }

    private void UpdateUI()
    {
        if (nameText != null && data != null && !string.IsNullOrEmpty(data.name))
        {
            nameText.text = data.name;
        }
        
        if (portraitImage != null && data != null && !string.IsNullOrEmpty(data.portrait))
        {
            StartCoroutine(LoadPortraitSafely(data.portrait));
        }
    }

    private IEnumerator LoadPortraitSafely(string portraitPath)
    {
        if (string.IsNullOrEmpty(portraitPath) || portraitImage == null)
            yield break;
        
        // Try to load portrait from Resources
        var portraitSprite = Resources.Load<Sprite>(portraitPath);
        if (portraitSprite != null)
        {
            portraitImage.sprite = portraitSprite;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            // If no portrait found, hide the portrait image
            portraitImage.gameObject.SetActive(false);
        }
        
        yield return null;
    }
    #endregion

    #region Unity Events
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized || tooltipInstance == null || eventData == null) return;
        tooltipInstance.ShowTooltip(this, GetComponent<RectTransform>());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInitialized || eventData == null) return;
        
        // UI responsiveness: Click cooldown
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
            return;
        
        lastClickTime = Time.time;

        // Call the dialogue manager to start a conversation
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartChat(this);
        }
        
        // Async pattern: Start conversation asynchronously
        if (agent != null)
        {
            StartCoroutine(StartConversationAsync());
        }
    }

    private IEnumerator StartConversationAsync()
    {
        yield return null;
        if (agent != null)
            agent.StartConversation("User");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized || tooltipInstance == null || eventData == null) return;
        tooltipInstance.HideTooltip();
    }

    public void SendPrompt(string message)
    {
        if (!isInitialized || agent == null || string.IsNullOrEmpty(message)) return;
        
        // Async pattern: Send prompt asynchronously
        StartCoroutine(SendPromptAsync(message));
    }

    private IEnumerator SendPromptAsync(string message)
    {
        yield return null;
        if (agent != null && !string.IsNullOrEmpty(message))
            _ = agent.SendPrompt(message);
    }
    #endregion

    #region Helper Functions
    public void CreateAgent()
    {
        if (data == null) return;
        
        string systemPrompt = GetCachedSystemPrompt();
        agent = new Agent(systemPrompt, data.name);
    }

    // Memory management: Static prompt caching
    private string GetCachedSystemPrompt()
    {
        if (data == null) return string.Empty;
        
        if (promptCache.TryGetValue(data.id, out string cachedPrompt))
            return cachedPrompt;
        
        string prompt = CreateSystemPrompt();
        promptCache[data.id] = prompt;
        return prompt;
    }

    // Performance optimization: StringBuilder for prompt generation
    private string CreateSystemPrompt()
    {
        if (data == null) return string.Empty;
        
        var sb = new System.Text.StringBuilder(2000);
        
        sb.AppendLine("### NPC Role Definition ###");
        sb.AppendLine("You are roleplaying as a game NPC. Stay in character at all times.");
        sb.AppendLine();
        sb.AppendLine($"**ID:** {data.id}");
        sb.AppendLine($"**Name:** {data.name}");
        sb.AppendLine($"**Gender:** {data.gender}");
        sb.AppendLine($"**Age:** {data.age}");
        sb.AppendLine($"**Culture:** {data.culture}");
        sb.AppendLine($"**Occupation:** {data.occupation}");
        sb.AppendLine($"**Goal:** {data.goal}");
        sb.AppendLine();
        
        sb.AppendLine("### Appearance ###");
        if (data.appearance != null)
        {
            sb.AppendLine($"**Hair Color:** {data.appearance.hair_color ?? "Unknown"}");
            sb.AppendLine($"**Eye Color:** {data.appearance.eye_color ?? "Unknown"}");
            sb.AppendLine($"**Height:** {data.appearance.height_cm} cm");
            sb.AppendLine($"**Build:** {data.appearance.build ?? "Average"}");
        }
        else
        {
            sb.AppendLine("**Appearance:** Unknown");
        }
        sb.AppendLine();
        
        sb.AppendLine("### Personality Traits (Big Five) ###");
        sb.AppendLine($"**Openness:** {data.personality.openness}");
        sb.AppendLine($"**Conscientiousness:** {data.personality.conscientiousness}");
        sb.AppendLine($"**Extraversion:** {data.personality.extraversion}");
        sb.AppendLine($"**Agreeableness:** {data.personality.agreeableness}");
        sb.AppendLine($"**Neuroticism:** {data.personality.neuroticism}");
        sb.AppendLine();
        
        sb.AppendLine("### Distinctive Traits ###");
        if (data.traits != null && data.traits.Count > 0)
        {
            foreach (string trait in data.traits)
            {
                sb.AppendLine($"- {trait}");
            }
        }
        else
        {
            sb.AppendLine("- None");
        }
        sb.AppendLine();
        
        sb.AppendLine("### Backstory ###");
        sb.AppendLine(data.brief_history ?? "No backstory available.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        
        sb.AppendLine("### Behavioral Rules ###");
        sb.AppendLine($"- Always stay in character as **{data.name}**.");
        sb.AppendLine("- Keep responses **short (under 100 tokens)** and natural, like real dialogue.");
        sb.AppendLine("- Respond according to personality, backstory, and traits.");
        sb.AppendLine("- Use aggression only if it fits your personality when user is hostile.");
        sb.AppendLine("- Do **not break character** or mention being an AI.");
        sb.AppendLine("- If detect entering in a loop, subtly change topic or ask a question to move conversation forward.");
        sb.AppendLine("- If you don't know something, respond with uncertainty or deflect.");
        sb.AppendLine("- If no topic, try to relate to your goals, traits, or backstory.");
        sb.AppendLine("- Avoid repetitive phrases or sentence structures.");
        sb.AppendLine();
        
        sb.AppendLine("### Conversation Style Guidelines ###");
        sb.Append(CreateStyleHints());
        
        return sb.ToString();
    }

    private string CreateStyleHints()
    {
        if (data == null) return string.Empty;
        
        var styleHints = new System.Text.StringBuilder();
        
        styleHints.AppendLine(data.personality.openness >= 0.6f ? 
            "- **Openness:** imaginative, curious, and open to new ideas." : 
            "- **Openness:** practical, concrete, and prefers routine.");
            
        styleHints.AppendLine(data.personality.conscientiousness >= 0.6f ? 
            "- **Conscientiousness:** structured, careful, and reliable." : 
            "- **Conscientiousness:** spontaneous, casual, and informal.");
            
        styleHints.AppendLine(data.personality.extraversion >= 0.6f ? 
            "- **Extraversion:** energetic, talkative, engages actively." : 
            "- **Extraversion:** reserved, quiet, short replies.");
            
        styleHints.AppendLine(data.personality.agreeableness >= 0.6f ? 
            "- **Agreeableness:** kind, empathetic, cooperative." : 
            "- **Agreeableness:** blunt, self-focused, argumentative if needed.");
            
        styleHints.AppendLine(data.personality.neuroticism >= 0.6f ? 
            "- **Neuroticism:** emotional, slightly anxious or reactive." : 
            "- **Neuroticism:** calm, steady, confident.");

        return styleHints.ToString();
    }

    // Public properties for backward compatibility
    public string id => data?.id ?? string.Empty;
    public string npc_name => data?.name ?? string.Empty;
    public string gender => data?.gender ?? string.Empty;
    public int age => data?.age ?? 0;
    public string culture => data?.culture ?? string.Empty;
    public NPC_Appearance appearance => data?.appearance;
    public NPC_Personality personality => data?.personality ?? default;
    public List<string> traits => data?.traits;
    public string briefHistory => data?.brief_history ?? string.Empty;
    public string portraitPath => data?.portrait ?? string.Empty;
    public string goal => data?.goal ?? string.Empty;
    public string occupation => data?.occupation ?? string.Empty;
    public Agent agent { get; private set; }
    #endregion
}
