using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUIBehaviour : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text contentText;


    // Add NPC message
    public void AddLeftMessage(string name, string content)
    {
        nameText.text = name;
        contentText.text = content;
    }

    // Add Player message
    public void AddRightMessage(string name, string content)
    {
        nameText.text = name;
        contentText.text = content;
    }
    public void AddMiddleMessage(string content)
    {
        contentText.text = content;
    }
}
