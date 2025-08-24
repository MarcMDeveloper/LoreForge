using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MessageType
{
    Left,
    Right,
    Middle
}

public class ChatUIBehaviour : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text contentText;

    // Add NPC message
    public void AddMessage(string name, string content, MessageType messageType)
    {
        nameText.text = name;
        contentText.text = content;
    }
}
