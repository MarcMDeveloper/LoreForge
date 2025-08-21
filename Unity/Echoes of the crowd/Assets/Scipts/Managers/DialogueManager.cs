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

    public Transform contentTransform;  
    public GameObject rightMessagePrefab;
    public GameObject leftMessagePrefab;
    public GameObject middleMessagePrefab;
    public ScrollRect scrollRect;      

    
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
        EndChat();
        ChatScreen.SetActive(false);
    }
    #endregion


    #region Chat UI 
    // Add NPC message
    public void AddLeftMessage(string name, string message)
    {
        GameObject newMessage = Instantiate(rightMessagePrefab, contentTransform);
        newMessage.GetComponent<ChatUIBehaviour>().AddLeftMessage(name,message);
        ScrollToBottom();
    }

    // Add Player message
    public void AddRightMessage(string name,string message)
    {
        GameObject newMessage = Instantiate(leftMessagePrefab, contentTransform);
        newMessage.GetComponent<ChatUIBehaviour>().AddLeftMessage(name,message);
        ScrollToBottom();
    }

    public void AddMiddleMessage(string message)
    {
        GameObject newMessage = Instantiate(middleMessagePrefab, contentTransform);
        newMessage.GetComponent<ChatUIBehaviour>().AddMiddleMessage(message);
        ScrollToBottom();
    }
    // Keep scroll at bottom when new message appears
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    #endregion

    #region Chat USER - NPC 
    public void StartChat(NPC npc)
    {
        // Agent already created not necessary to create it again
        //Debug.Log($"Starting chat with NPC: {npc.npc_name}");
        // Initialize the agent for this NPC
        // npc.agent = new Agent(npc.CreateSystemPrompt());

        // Open the chat screen
        OpenChatScreen();

        // Add the NPC to the chat list
        npcsInChat.Add(npc);

        // Add summmary if exist
        string summary = npc.agent.GetSummary(npc.npc_name);
        if (!string.IsNullOrEmpty(summary))
        {
            AddMiddleMessage($"**{npc.npc_name}**: {summary}");
        }
        else
        {
            AddMiddleMessage($"**{npc.npc_name}**: Let's start a conversation!");
        }
        
    }

    public void EndChat()
    {
        // Clear the chat history if needed        
        // Tell the agent
        npcsInChat[0].agent.FinishChat();
        npcsInChat.Clear();
        inputText.text = string.Empty;

        // Erase all messages in the chat
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SendMessage()
    {
        string currenMessage = inputText.text;
        if (string.IsNullOrEmpty(currenMessage))
        {
            Debug.LogWarning("Cannot send an empty message.");
            return;
        }

        npcsInChat[0].SendPrompt(currenMessage);

        AddLeftMessage("User",currenMessage);     
        inputText.text = string.Empty;          

    }
    
    public void MessageRecived(string name, string message)
    {
        AddRightMessage(name,message);
    }
    #endregion

    #region Chat load management
    public void LoadChat(int firstID, int secondID)
    {
        // Load the chat history between two NPCs
        // This could involve fetching data from a database or a file
        // For now, we will just log the IDs
        Debug.Log($"Loading chat between NPCs with IDs: {firstID} and {secondID}");
    }
    #endregion
}
