using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUIBehaviour : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text contentText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
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
}
