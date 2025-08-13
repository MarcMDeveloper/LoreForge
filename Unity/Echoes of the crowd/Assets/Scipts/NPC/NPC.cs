using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class NPC : MonoBehaviour
{
    #region Fields
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
    public string goals;
    public string occupation;
    #endregion

    #region Constructor
    public NPC(string id, string npc_name, string gender, int age, string culture,
            NPC_Appearance appearance, NPC_Personality personality,
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
        this.goals = goals;
        this.occupation = occupation;

        // Initialize the agent for this NPC
        this.agent = new Agent();
    }
    #endregion

    #region HelperFunctions

    #endregion


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Nested Classes 
    [System.Serializable]
    public class NPC_Appearance
    {
        public NPC_Appearance(string hairColor, string eyeColor, int heightCm, string build)
        {
            this.hairColor = hairColor;
            this.eyeColor = eyeColor;
            this.heightCm = heightCm;
            this.build = build;
        }

        public string hairColor;
        public string eyeColor;
        public int heightCm;
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
