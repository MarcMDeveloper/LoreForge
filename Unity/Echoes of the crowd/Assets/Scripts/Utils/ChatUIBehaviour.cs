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
        if(messageType == MessageType.Left)
        {
            nameText.alignment = TextAlignmentOptions.Left;
            contentText.alignment = TextAlignmentOptions.Left;
        }
        else if(messageType == MessageType.Right)
        {
            nameText.alignment = TextAlignmentOptions.Right;
            contentText.alignment = TextAlignmentOptions.Right;
        }
        else if(messageType == MessageType.Middle)
        {
            nameText.alignment = TextAlignmentOptions.Center;
            contentText.alignment = TextAlignmentOptions.Center;
        }
    }
}
