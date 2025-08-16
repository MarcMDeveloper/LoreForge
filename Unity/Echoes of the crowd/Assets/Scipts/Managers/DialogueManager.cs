using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    #region Variables
    public GameObject ChatScreen;
    public TMP_InputField inputText;
    public List<NPC> npcsInChat;
    #endregion

    #region Singleton Setup
    public static DialogueManager Instance { get; private set; }
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

        ChatScreen.SetActive(false);

        // Keep this object alive between scenes
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Generic
    public void OpenChatScreen()
    {
        ChatScreen.SetActive(true);
    }
    public void CloseChatScreen()
    {
        ChatScreen.SetActive(false);
    }
    #endregion

    #region Chat USER - NPC 
    public void StartChat(NPC npc)
    {
        Debug.Log($"Starting chat with NPC: {npc.npc_name}");
        // Initialize the agent for this NPC
        // npc.agent = new Agent(npc.CreateSystemPrompt());

        // Open the chat screen
        OpenChatScreen();

        // Add the NPC to the chat list
        npcsInChat.Add(npc);

    }

    public void EndChat()
    {
        // Close the chat screen
        CloseChatScreen();

        // Clear the chat history if needed
        // This could involve resetting UI elements or clearing data structures
    }

    public void SendMessage()
    {
        string currenMessage = inputText.text;
        if (string.IsNullOrEmpty(currenMessage))
        {
            Debug.LogWarning("Cannot send an empty message.");
            return;
        }

        

    }
    public void MessageSend()
    {

    }

    public void MessageRecived()
    {

    }
    #endregion

    #region Chat load management
    public void ClearChat()
    {

    }

    public void LoadChat(int firstID, int secondID)
    {
        // Load the chat history between two NPCs
        // This could involve fetching data from a database or a file
        // For now, we will just log the IDs
        Debug.Log($"Loading chat between NPCs with IDs: {firstID} and {secondID}");
    }
    #endregion
}
