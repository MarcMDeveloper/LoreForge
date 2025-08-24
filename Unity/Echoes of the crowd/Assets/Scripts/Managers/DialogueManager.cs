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
    public GameObject bubbleMessagePrefab;
    public ScrollRect scrollRect;

    public List<List<SummaryConversation>> allConversations;
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

        allConversations = new List<List<SummaryConversation>>();

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

        AddMessage("User", currenMessage, MessageType.Left);
        inputText.text = string.Empty;

    }

    public void MessageRecived(string name, string message)
    {
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

        // Start conversation for both
        npcA.agent.StartConversation(npcB.npc_name);
        npcB.agent.StartConversation(npcA.npc_name);

        // Kick off with initial message from NPC A
        AddMessage(npcA.npc_name, initialMessage, MessageType.Left);
        npcA.agent.SendPrompt(initialMessage);


        // Then let NPC B reply
        StartCoroutine(NPCtoNPCLoop(npcA, npcB));
    }

    private IEnumerator NPCtoNPCLoop(NPC npcA, NPC npcB)
    {
        NPC speaker = npcA;
        NPC listener = npcB;

        int exchanges = 0;
        int maxExchanges = 6;

        string firstMessage = "Hello, how are you today?";
        AddMessage(speaker.npc_name, firstMessage, MessageType.Left);
        speaker.agent.SendPrompt(firstMessage);

        yield return new WaitForSeconds(2f);

        while (exchanges < maxExchanges)
        {
            // Get the last thing speaker said
            string lastMessage = speaker.agent.GetLastMessage();

            if (!string.IsNullOrEmpty(lastMessage))
            {
                // Show message on correct side
                if (speaker == npcA)
                    AddMessage(speaker.npc_name, lastMessage, MessageType.Left);
                else
                    AddMessage(speaker.npc_name, lastMessage, MessageType.Right);

                // Tell the other NPC what was just said (with speaker name!)
                string formatted = $"{speaker.npc_name} said: {lastMessage}";
                listener.agent.SendPrompt(formatted);
            }

            // Swap turns
            NPC temp = speaker;
            speaker = listener;
            listener = temp;

            exchanges++;
            yield return new WaitForSeconds(2f);
        }

        AddMessage(speaker.npc_name + " " + listener.npc_name, "Conversation ended.", MessageType.Middle);
    }
    #endregion

    #region Dialogues management
    public void LoadChat(int firstID, int secondID)
    {
        // Load the chat history between two NPCs
        // This could involve fetching data from a database or a file
        // For now, we will just log the IDs

    }
    

    #endregion
}
