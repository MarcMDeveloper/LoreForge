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
    [Header("Loaded data")]
    public Agent agent;
    public string id;
    public string npc_name;
    public string gender;
    public int age;
    public string culture;

    public NPC_Appearance appearance;
    public NPC_Personality personality;
    public List<string> traits;
    public string briefHistory;
    public string portraitPath;
    public string goal;
    public string occupation;
    
    [Header("UI Components")]
    public Image portraitImage;       
    public TMP_Text nameText;

    // WebGL optimization: Cache tooltip reference
    private static NPCTooltip cachedTooltip;
    private bool isInitialized = false;
    
    // WebGL optimization: Cache system prompt
    private string cachedSystemPrompt;

    #endregion

    #region Initialization
    public void Initialize(string id, string npc_name, string gender, int age, string culture,
            NPC_Appearance appearance,
            NPC_Personality personality,
            List<string> traits, string briefHistory, string portraitPath, string goals, string occupation)
    {
        // Assign values to the NPC fields
        this.id = id;
        this.npc_name = npc_name;
        this.gender = gender;
        this.age = age;
        this.culture = culture;
        this.appearance = appearance;
        this.personality = personality;
        this.traits = traits ?? new List<string>();
        this.briefHistory = briefHistory;
        this.portraitPath = portraitPath;
        this.goal = goals;
        this.occupation = occupation;

        // WebGL optimization: Cache tooltip reference once
        if (cachedTooltip == null)
        {
            cachedTooltip = FindFirstObjectByType<NPCTooltip>();
        }

        // Initialize the agent for this NPC
        CreateAgent();

        // Set the name text
        if (nameText != null)
        {
            nameText.text = npc_name;
        }

        isInitialized = true;
    }
    #endregion

    #region Unity Events
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) return;
        cachedTooltip?.ShowTooltip(this, GetComponent<RectTransform>());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInitialized) return;
        
        Debug.Log($"NPC clicked: {npc_name}");

        // Call the dialogue manager to start a conversation
        DialogueManager.Instance.StartChat(this);
        
        // WebGL optimization: Start conversation asynchronously
        if (agent != null)
        {
            StartCoroutine(StartConversationAsync());
        }
    }

    private System.Collections.IEnumerator StartConversationAsync()
    {
        // WebGL optimization: Yield to prevent blocking
        yield return null;
        agent.StartConversation("User");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized) return;
        cachedTooltip?.HideTooltip();
    }

    public void SendPrompt(string message)
    {
        if (!isInitialized || agent == null) return;
        
        // WebGL optimization: Send prompt asynchronously
        StartCoroutine(SendPromptAsync(message));
    }

    private System.Collections.IEnumerator SendPromptAsync(string message)
    {
        yield return null;
        _ = agent.SendPrompt(message);
    }
    #endregion

    #region Helper Functions
    public void CreateAgent()
    {
        // WebGL optimization: Cache system prompt
        if (string.IsNullOrEmpty(cachedSystemPrompt))
        {
            cachedSystemPrompt = CreateSystemPrompt();
        }
        
        // Create a new agent with the cached system prompt
        agent = new Agent(cachedSystemPrompt, npc_name);
    }

    private string CreateSystemPrompt()
    {
        return
        $@"### NPC Role Definition ###
        You are roleplaying as a game NPC. Stay in character at all times.

        **ID:** {id}  
        **Name:** {npc_name}  
        **Gender:** {gender}  
        **Age:** {age}  
        **Culture:** {culture}  
        **Occupation:** {occupation}  
        **Goal:** {goal}  

        ### Appearance ###
        **Hair Color:** {appearance?.hair_color ?? "Unknown"}  
        **Eye Color:** {appearance?.eye_color ?? "Unknown"}  
        **Height:** {appearance?.height_cm ?? 0} cm  
        **Build:** {appearance?.build ?? "Average"}  

        ### Personality Traits (Big Five) ###
        **Openness:** {personality.openness}  
        **Conscientiousness:** {personality.conscientiousness}  
        **Extraversion:** {personality.extraversion}  
        **Agreeableness:** {personality.agreeableness}  
        **Neuroticism:** {personality.neuroticism}  

        ### Distinctive Traits ###
        - {string.Join("\n- ", traits ?? new List<string>())}

        ### Backstory ###
        {briefHistory ?? "No backstory available."}

        ---

        ### Behavioral Rules ###
        - Always stay in character as **{npc_name}**.  
        - Keep responses **short (under 100 tokens)** and natural, like real dialogue.  
        - Respond according to personality, backstory, and traits.  
        - Use aggression only if it fits your personality when user is hostile.  
        - Do **not break character** or mention being an AI.
        - If detect entering in a loop, subtly change topic or ask a question to move conversation forward.
        - If you don't know something, respond with uncertainty or deflect.
        - If no topic, try to relate to your goals, traits, or backstory.
        - Avoid repetitive phrases or sentence structures.

        ### Conversation Style Guidelines ###
        {CreateStyleHints()}
        ";
    }

    private string CreateStyleHints()
    {
        // WebGL optimization: Use string builder pattern for better performance
        var styleHints = new System.Text.StringBuilder();

        styleHints.AppendLine(personality.openness >= 0.6f ? 
            "- **Openness:** imaginative, curious, and open to new ideas." : 
            "- **Openness:** practical, concrete, and prefers routine.");
            
        styleHints.AppendLine(personality.conscientiousness >= 0.6f ? 
            "- **Conscientiousness:** structured, careful, and reliable." : 
            "- **Conscientiousness:** spontaneous, casual, and informal.");
            
        styleHints.AppendLine(personality.extraversion >= 0.6f ? 
            "- **Extraversion:** energetic, talkative, engages actively." : 
            "- **Extraversion:** reserved, quiet, short replies.");
            
        styleHints.AppendLine(personality.agreeableness >= 0.6f ? 
            "- **Agreeableness:** kind, empathetic, cooperative." : 
            "- **Agreeableness:** blunt, self-focused, argumentative if needed.");
            
        styleHints.AppendLine(personality.neuroticism >= 0.6f ? 
            "- **Neuroticism:** emotional, slightly anxious or reactive." : 
            "- **Neuroticism:** calm, steady, confident.");

        return styleHints.ToString();
    }

    // WebGL optimization: Cleanup method for object pooling
    public void ResetForPool()
    {
        isInitialized = false;
        cachedSystemPrompt = null;
        
        if (agent != null)
        {
            // Clean up agent resources if needed
            agent = null;
        }
        
        if (nameText != null)
        {
            nameText.text = "";
        }
    }
    #endregion

    #region Nested Classes 
    [System.Serializable]
    public class NPC_Appearance
    {
        public NPC_Appearance(string hairColor, string eyeColor, int heightCm, string build)
        {
            this.hair_color = hairColor;
            this.eye_color = eyeColor;
            this.height_cm = heightCm;
            this.build = build;
        }

        public string hair_color;
        public string eye_color;
        public int height_cm;
        public string build;
    }

    [System.Serializable]
    public struct NPC_Personality
    {
        public NPC_Personality(float openness, float conscientiousness, float extraversion, float agreeableness, float neuroticism)
        {
            this.openness = openness;
            this.conscientiousness = conscientiousness;
            this.extraversion = extraversion;
            this.agreeableness = agreeableness;
            this.neuroticism = neuroticism;
        }   

        [Range(0f, 1f)] public float openness;
        [Range(0f, 1f)] public float conscientiousness;
        [Range(0f, 1f)] public float extraversion;
        [Range(0f, 1f)] public float agreeableness;
        [Range(0f, 1f)] public float neuroticism;
    }
    #endregion
}
