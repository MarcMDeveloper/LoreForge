using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    #region Variables
    public GameObject ChatScreen;
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

    public void OpenChatScreen()
    {
        ChatScreen.SetActive(true);
    }
    public void CloseChatScreen()
    {
        ChatScreen.SetActive(false);
    }
}
