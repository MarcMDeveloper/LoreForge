using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

[System.Serializable]
public class ConversationData
{
    public List<OpenAIMessage> messages = new List<OpenAIMessage>();
}

public class DialogueManager : MonoBehaviour
{
    #region Variables
    public GameObject ChatScreen;
    public TMP_InputField inputText;
    public List<NPC> npcsInChat;

    public Transform contentTransform;
    public GameObject bubbleMessagePrefab;
    public ScrollRect scrollRect;

    public List<ConversationData> allConversations;
    
    // New field to store current conversation messages
    private List<OpenAIMessage> currentConversationMessages;
    
    // Inspector helper properties to show conversation info
    [Header("Conversation Debug Info")]
    [SerializeField] private int totalConversations;
    [SerializeField] private string[] conversationPreviews;
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

        allConversations = new List<ConversationData>();
        currentConversationMessages = new List<OpenAIMessage>();

        // Initialize inspector debug info
        UpdateInspectorDebugInfo();

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
    public void AddMessage(string name, string message, MessageType messageType)
    {
        GameObject newMessage = Instantiate(bubbleMessagePrefab, contentTransform);
        newMessage.GetComponent<ChatUIBehaviour>().AddMessage(name, message, messageType);

        ScrollToBottom();
    }

    // Keep scroll at bottom when new message appears
    private void ScrollToBottom()
    {
        // Force layout rebuild to ensure proper stacking
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform as RectTransform);
        Canvas.ForceUpdateCanvases();

        // Add a small delay to ensure layout is processed
        StartCoroutine(DelayedScrollToBottom());
    }

    private IEnumerator DelayedScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform as RectTransform);
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

        // Clear current conversation messages for new chat
        currentConversationMessages.Clear();

        // Add summmary if exist
        string summary = npc.agent.GetSummary(npc.npc_name);
        if (!string.IsNullOrEmpty(summary))
        {
            AddMessage(npc.npc_name, $"{summary}", MessageType.Middle);
        }
        else
        {
            AddMessage(npc.npc_name, $"Let's start a conversation!", MessageType.Middle);
        }
    }

    public void EndChat()
    {
        // Save the conversation before clearing
        if (npcsInChat.Count > 0 && currentConversationMessages.Count > 0)
        {
            SaveConversation();
        }
        
        // Clear the chat history if needed        
        // Tell the agent (only if there are NPCs in chat)
        if (npcsInChat.Count > 0)
        {
            npcsInChat[0].agent.FinishChat();
        }
        npcsInChat.Clear();
        inputText.text = string.Empty;

        // Clear current conversation messages
        currentConversationMessages.Clear();

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

        // Check if there are NPCs in chat
        if (npcsInChat.Count == 0)
        {
            Debug.LogWarning("No NPCs in chat to send message to.");
            return;
        }

        // Add user message to conversation history with actual speaker name
        currentConversationMessages.Add(new OpenAIMessage 
        { 
            role = "User", 
            content = currenMessage 
        });

        npcsInChat[0].SendPrompt(currenMessage);

        AddMessage("User", currenMessage, MessageType.Left);
        inputText.text = string.Empty;

    }

    public void MessageRecived(string name, string message)
    {
        // Add NPC message to conversation history with actual speaker name
        currentConversationMessages.Add(new OpenAIMessage 
        { 
            role = name, 
            content = message 
        });
        
        AddMessage(name, message, MessageType.Right);
    }
    #endregion

    #region Chat NPC - NPC
    public void StartNPCtoNPCChat(NPC npcA, NPC npcB, string initialMessage)
    {
        OpenChatScreen();

        npcsInChat.Clear();
        npcsInChat.Add(npcA);
        npcsInChat.Add(npcB);

        // Clear current conversation messages for new chat
        currentConversationMessages.Clear();

        // Start conversation for both NPCs with proper context
        npcA.agent.StartConversation(npcB.npc_name);
        npcB.agent.StartConversation(npcA.npc_name);

        // Add context about the conversation topic
        string contextA = $"You are having a conversation with {npcB.npc_name}. {npcB.npc_name} is a {npcB.occupation} from {npcB.culture}. Keep responses natural and in character.";
        string contextB = $"You are having a conversation with {npcA.npc_name}. {npcA.npc_name} is a {npcA.occupation} from {npcA.culture}. Keep responses natural and in character.";
        
        _ = npcA.agent.SendPromptSilent(contextA);
        _ = npcB.agent.SendPromptSilent(contextB);

        // Start the conversation with the initial message from NPC A
        AddMessage(npcA.npc_name, initialMessage, MessageType.Left);
        
        // Add initial message to conversation history with actual speaker name
        currentConversationMessages.Add(new OpenAIMessage 
        { 
            role = npcA.npc_name, 
            content = initialMessage 
        });
        
        _ = npcA.agent.SendPromptSilent(initialMessage);

        // Start the conversation loop
        StartCoroutine(NPCtoNPCLoop(npcA, npcB, initialMessage));
    }

    private IEnumerator NPCtoNPCLoop(NPC npcA, NPC npcB, string firstMessage)
    {
        NPC speaker = npcA;
        NPC listener = npcB;
        string currentMessage = firstMessage;

        int exchanges = 0;
        int maxExchanges = 8; // Increased for more natural conversation length
        int consecutiveNullResponses = 0;
        const int maxNullResponses = 3;

        while (exchanges < maxExchanges && consecutiveNullResponses < maxNullResponses)
        {
            // Give the listener time to process the current message
            yield return new WaitForSeconds(1.5f);

            // Get response from the listener
            yield return StartCoroutine(GetNPCResponseAsync(listener, speaker.npc_name, currentMessage, (response) =>
            {
                if (!string.IsNullOrEmpty(response) && !response.StartsWith("Error:"))
                {
                    // Display the response on the correct side
                    MessageType messageSide = (listener == npcA) ? MessageType.Left : MessageType.Right;
                    AddMessage(listener.npc_name, response, messageSide);
                    
                    // Add response to conversation history with actual speaker name
                    currentConversationMessages.Add(new OpenAIMessage 
                    { 
                        role = listener.npc_name, 
                        content = response 
                    });
                    
                    // Reset null response counter
                    consecutiveNullResponses = 0;
                    
                    // Swap roles for next exchange
                    NPC temp = speaker;
                    speaker = listener;
                    listener = temp;
                    currentMessage = response;
                    
                    exchanges++;
                }
                else
                {
                    consecutiveNullResponses++;
                    Debug.LogWarning($"NPC {listener.npc_name} gave null or error response. Attempt {consecutiveNullResponses}/{maxNullResponses}");
                    
                    // Try to prompt the NPC with a more direct question
                    string fallbackPrompt = $"Please respond to what {speaker.npc_name} just said: '{currentMessage}'";
                    _ = listener.agent.SendPromptSilent(fallbackPrompt);
                }
            }));
            
            yield return new WaitForSeconds(1f);
        }

        // End conversation gracefully
        string endMessage = "The conversation has come to a natural conclusion.";
        AddMessage("System", endMessage, MessageType.Middle);
        
        // Save the conversation before cleaning up
        if (currentConversationMessages.Count > 0)
        {
            SaveConversation();
        }
        
        // Clean up the conversation
        npcA.agent.FinishChat();
        npcB.agent.FinishChat();
        
        // Clear current conversation messages
        currentConversationMessages.Clear();
    }

    private IEnumerator GetNPCResponseAsync(NPC npc, string speakerName, string message, System.Action<string> callback)
    {
        // Format the message to include who is speaking
        string formattedMessage = $"{speakerName} says: {message}";
        
        // Send the prompt silently and get the response
        var responseTask = npc.agent.SendPromptSilent(formattedMessage);
        
        // Wait for the response
        while (!responseTask.IsCompleted)
        {
            yield return null;
        }
        
        string response = null;
        try
        {
            response = responseTask.Result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting response from {npc.npc_name}: {e.Message}");
            callback?.Invoke(null);
            yield break;
        }
        
        // Validate the response
        if (string.IsNullOrEmpty(response) || response.Length < 3)
        {
            callback?.Invoke(null);
        }
        else
        {
            callback?.Invoke(response);
        }
    }

    private string GetNPCResponse(NPC npc, string speakerName, string message)
    {
        try
        {
            // Format the message to include who is speaking
            string formattedMessage = $"{speakerName} says: {message}";
            npc.agent.SendPrompt(formattedMessage);
            
            // Wait a moment for the agent to process
            System.Threading.Thread.Sleep(500);
            
            // Get the response
            string response = npc.agent.GetLastMessage();
            
            // Validate the response
            if (string.IsNullOrEmpty(response) || response.Length < 3)
            {
                return null;
            }
            
            return response;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting response from {npc.npc_name}: {e.Message}");
            return null;
        }
    }
    #endregion

    #region Dialogues management
    public void LoadChat(int firstID, int secondID)
    {
        // Load the chat history between two NPCs
        // This could involve fetching data from a database or a file
        // For now, we will just log the IDs

    }
    
    /// <summary>
    /// Saves the current conversation without system messages
    /// </summary>
    private void SaveConversation()
    {
        if (currentConversationMessages.Count == 0)
            return;

        // Create a new conversation data object
        ConversationData conversationData = new ConversationData();
        
        // Copy all messages to the new conversation
        foreach (var message in currentConversationMessages)
        {
            conversationData.messages.Add(new OpenAIMessage
            {
                role = message.role,
                content = message.content
            });
        }
        
        // Add to all conversations
        allConversations.Add(conversationData);
        
        // Update inspector debug info
        UpdateInspectorDebugInfo();
        
        Debug.Log($"Saved conversation. Total conversations: {allConversations.Count}");
    }
    
    /// <summary>
    /// Updates the inspector debug information
    /// </summary>
    private void UpdateInspectorDebugInfo()
    {
        totalConversations = allConversations.Count;
        conversationPreviews = new string[allConversations.Count];
        
        for (int i = 0; i < allConversations.Count; i++)
        {
            if (allConversations[i].messages.Count > 0)
            {
                string preview = allConversations[i].messages[0].content;
                if (preview.Length > 50)
                {
                    preview = preview.Substring(0, 50) + "...";
                }
                conversationPreviews[i] = $"Conversation {i}: {preview}";
            }
            else
            {
                conversationPreviews[i] = $"Conversation {i}: Empty";
            }
        }
    }
    
    /// <summary>
    /// Displays a saved conversation in the chat UI
    /// </summary>
    /// <param name="conversationIndex">Index of the conversation to display</param>
    public void DisplaySavedConversation(int conversationIndex)
    {
        if (conversationIndex >= 0 && conversationIndex < allConversations.Count)
        {
            List<OpenAIMessage> conversation = allConversations[conversationIndex].messages;
            
            // Open the chat screen first
            OpenChatScreen();
            
            // Clear current chat display
            foreach (Transform child in contentTransform)
            {
                Destroy(child.gameObject);
            }
            
            // Display each message from the saved conversation
            foreach (var message in conversation)
            {
                if (message.role == "User")
                {
                    AddMessage("User", message.content, MessageType.Left);
                }
                else
                {
                    // Use the actual speaker name from the saved conversation
                    AddMessage(message.role, message.content, MessageType.Right);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the total number of saved conversations
    /// </summary>
    /// <returns>Total number of conversations</returns>
    public int GetTotalConversations()
    {
        return allConversations.Count;
    }
    
    /// <summary>
    /// Gets a specific conversation by index
    /// </summary>
    /// <param name="index">Index of the conversation</param>
    /// <returns>List of messages in the conversation</returns>
    public List<OpenAIMessage> GetConversation(int index)
    {
        if (index >= 0 && index < allConversations.Count)
        {
            return allConversations[index].messages;
        }
        return null;
    }
    
    /// <summary>
    /// Gets a preview of a conversation (first few messages) for UI display
    /// </summary>
    /// <param name="index">Index of the conversation</param>
    /// <returns>Preview text of the conversation</returns>
    public string GetConversationPreview(int index)
    {
        if (index >= 0 && index < allConversations.Count)
        {
            List<OpenAIMessage> conversation = allConversations[index].messages;
            if (conversation.Count > 0)
            {
                // Return the first message as preview
                return conversation[0].content.Length > 50 
                    ? conversation[0].content.Substring(0, 50) + "..." 
                    : conversation[0].content;
            }
        }
        return "No conversation available";
    }
    
    /// <summary>
    /// Gets the number of messages in a specific conversation
    /// </summary>
    /// <param name="index">Index of the conversation</param>
    /// <returns>Number of messages</returns>
    public int GetConversationMessageCount(int index)
    {
        if (index >= 0 && index < allConversations.Count)
        {
            return allConversations[index].messages.Count;
        }
        return 0;
    }
    
    /// <summary>
    /// Manually refresh the inspector debug information
    /// </summary>
    [ContextMenu("Refresh Inspector Debug Info")]
    public void RefreshInspectorDebugInfo()
    {
        UpdateInspectorDebugInfo();
    }
    
    /// <summary>
    /// Clear all saved conversations (for testing)
    /// </summary>
    [ContextMenu("Clear All Conversations")]
    public void ClearAllConversations()
    {
        allConversations.Clear();
        UpdateInspectorDebugInfo();
        Debug.Log("All conversations cleared.");
    }
    
    /// <summary>
    /// Get detailed info about a specific conversation for debugging
    /// </summary>
    /// <param name="index">Index of the conversation</param>
    /// <returns>Detailed string representation of the conversation</returns>
    public string GetConversationDebugInfo(int index)
    {
        if (index >= 0 && index < allConversations.Count)
        {
            List<OpenAIMessage> conversation = allConversations[index].messages;
            string debugInfo = $"Conversation {index} ({conversation.Count} messages):\n";
            
            for (int i = 0; i < conversation.Count; i++)
            {
                debugInfo += $"[{i}] {conversation[i].role}: {conversation[i].content}\n";
            }
            
            return debugInfo;
        }
        return "Invalid conversation index";
    }

    #endregion
}
