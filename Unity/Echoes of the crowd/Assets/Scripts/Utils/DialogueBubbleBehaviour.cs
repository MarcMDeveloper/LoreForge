using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueBubbleBehaviour : MonoBehaviour
{
    [Header("UI Components")]
    public Button bubbleButton;
    public TMP_Text conversationText; // Single text component for the conversation participants
    
    [Header("Data")]
    public int conversationIndex;
    
    private void Start()
    {
        // Set up the button click event
        if (bubbleButton != null)
        {
            bubbleButton.onClick.AddListener(OnBubbleClicked);
        }
    }
    
    /// <summary>
    /// Initialize the dialogue bubble with conversation data
    /// </summary>
    /// <param name="index">Index of the conversation in DialogueManager</param>
    /// <param name="participants">Participants in the conversation (e.g., "Name1 to Name2")</param>
    public void InitializeBubble(int index, string participants)
    {
        conversationIndex = index;
        
        if (conversationText != null)
        {
            conversationText.text = participants;
        }
    }
    
    /// <summary>
    /// Handle button click to display the saved conversation
    /// </summary>
    private void OnBubbleClicked()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.DisplaySavedConversation(conversationIndex);
        }
        else
        {
            Debug.LogError("DialogueManager instance not found!");
        }
    }
    
    private void OnDestroy()
    {
        // Clean up button listener
        if (bubbleButton != null)
        {
            bubbleButton.onClick.RemoveListener(OnBubbleClicked);
        }
    }
}
