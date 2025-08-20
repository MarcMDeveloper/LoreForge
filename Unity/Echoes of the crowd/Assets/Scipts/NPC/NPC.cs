using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public class NPC : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler ,IPointerExitHandler
{
    #region Fields
    [Header("Loaded data")]
    // Saved data
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
    
    [Header("UI related")]
    // UI related fields
    public Image portraitImage;       
    public TMP_Text nameText;

    // Tooltip reference
    private NPCTooltip tooltip;

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
        this.traits = traits;
        this.briefHistory = briefHistory;
        this.portraitPath = portraitPath;
        this.goal = goals;
        this.occupation = occupation;

        // Initialize the agent for this NPC
        CreateAgent();

        tooltip = FindFirstObjectByType<NPCTooltip>();

        // Set the sprite and the name
        // Sprite portraitSprite = Resources.Load<Sprite>(npcData.portraitPath);
        nameText.text = npc_name;

    }
    #endregion

    #region Unity Events
    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip?.ShowTooltip(this, GetComponent<RectTransform>());
    }

    public void OnClick()
    { 
        DialogueManager.Instance.StartChat(this);
    }
    // Future check wht does not work
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"NPC clicked: {npc_name}");

        // Call the dialogue manager to start a conversation
        DialogueManager.Instance.StartChat(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.HideTooltip();
    }

    public void SendPrompt(string message)
    {
        // Send the message to the agent
        _ =agent.SendPrompt(message);
    }
    
    #endregion

    #region HelperFunctions
    public void CreateAgent()
    {
        // Create a new agent with the system prompt
        agent = new Agent(CreateSystemPrompt());
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
        **Hair Color:** {appearance.hair_color}  
        **Eye Color:** {appearance.eye_color}  
        **Height:** {appearance.height_cm} cm  
        **Build:** {appearance.build}  

        ### Personality Traits (Big Five) ###
        **Openness:** {personality.openness}  
        **Conscientiousness:** {personality.conscientiousness}  
        **Extraversion:** {personality.extraversion}  
        **Agreeableness:** {personality.agreeableness}  
        **Neuroticism:** {personality.neuroticism}  

        ### Distinctive Traits ###
        - {string.Join("\n- ", traits)}

        ### Backstory ###
        {briefHistory}

        ---

        ### Behavioral Rules ###
        - Always stay in character as **{npc_name}**.  
        - Keep responses **short (under 100 tokens)** and natural, like real dialogue.  
        - Respond according to personality, backstory, and traits.  
        - Use aggression only if it fits your personality when user is hostile.  
        - Do **not break character** or mention being an AI.

        ### Conversation Style Guidelines ###
        {CreateStyleHints()}
        ";
    }

    private string CreateStyleHints()
    {
        // Generate personality style hints
        string styleHints = "";

        styleHints += personality.openness >= 0.6f ? "- **Openness:** imaginative, curious, and open to new ideas.\n" : "- **Openness:** practical, concrete, and prefers routine.\n";
        styleHints += personality.conscientiousness >= 0.6f ? "- **Conscientiousness:** structured, careful, and reliable.\n" : "- **Conscientiousness:** spontaneous, casual, and informal.\n";
        styleHints += personality.extraversion >= 0.6f ? "- **Extraversion:** energetic, talkative, engages actively.\n" : "- **Extraversion:** reserved, quiet, short replies.\n";
        styleHints += personality.agreeableness >= 0.6f ? "- **Agreeableness:** kind, empathetic, cooperative.\n" : "- **Agreeableness:** blunt, self-focused, argumentative if needed.\n";
        styleHints += personality.neuroticism >= 0.6f ? "- **Neuroticism:** emotional, slightly anxious or reactive.\n" : "- **Neuroticism:** calm, steady, confident.\n";

        return styleHints;
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
