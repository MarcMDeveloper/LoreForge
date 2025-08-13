using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class NPC : MonoBehaviour
{
    #region Fields
    public Agent agent;
    public string id;
    public string name;
    public string gender;
    public int age;
    public string culture;

    public Appearance appearance;
    public Personality personality;
    public List<string> traits;
    public string briefHistory;
    public string portraitPath;
    public List<string> goals;
    public string occupation;
    #endregion

    #region Constructor
    public NPC(string id, string name, string gender, int age, string culture,
            Appearance appearance, Personality personality,
            List<string> traits, string briefHistory, string portraitPath, List<string> goals, string occupation)
    {
        // Assign values to the NPC fields
        this.id = id;
        this.name = name;
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
        this.agent = new Agent(this);
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
    private class NPC_Appearance
    {
        public string hairColor;
        public string eyeColor;
        public int heightCm;
        public string build;
    }

    [System.Serializable]
    private struct NPC_Personality
    {
        [Range(0f, 1f)] public float openness;
        [Range(0f, 1f)] public float conscientiousness;
        [Range(0f, 1f)] public float extraversion;
        [Range(0f, 1f)] public float agreeableness;
        [Range(0f, 1f)] public float neuroticism;
    }
    #endregion
}
