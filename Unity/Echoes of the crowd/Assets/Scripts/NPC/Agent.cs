using System;
using System.Collections.Generic;
using System.Net.Http;    
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

#region Nested Classes
[System.Serializable]
public class OpenAIMessage
{
    public string role;
    public string content;
}
public class OpenAIRequest
{
    public string model;
    public List<OpenAIMessage> messages;
}
[System.Serializable]
public class OpenAIResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public OpenAIMessage message;
}

public class SummaryConversation
{
    public string otherName;
    public string summary;
}
#endregion

public class Agent
{
    #region Variables
    private string systemPrompt;
    private string character_name;
    private List<OpenAIMessage> conversation;
    public List<SummaryConversation> conversationsSummary;
    private string talkintToName;

    // Secure API key management using PlayerPrefs with encryption
    private string apiKey;

    // See if can be done different
    private static readonly HttpClient httpClient = new HttpClient();
    #endregion


    public Agent(string sysPr, string characterName)
    {
        character_name = characterName;

        // Save system for now (Check later if really need it)
        systemPrompt = sysPr;

        // System prompt will be set later
        conversationsSummary = new List<SummaryConversation>();
        
        // Initialize API key from secure storage
        InitializeAPIKey();
    }

    #region API Key Management
    private void InitializeAPIKey()
    {
        if (APIKeyManager.Instance != null)
        {
            apiKey = APIKeyManager.Instance.GetAPIKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("No API key found. Please set up the API key in APIKeyManager.");
            }
            else if (!APIKeyManager.Instance.ValidateAPIKeyFormat())
            {
                Debug.LogError("Invalid API key format. Please check your API key setup.");
                apiKey = null;
            }
        }
        else
        {
            Debug.LogError("APIKeyManager not found. Please ensure APIKeyManager is in the scene.");
        }
    }

    public bool HasValidAPIKey()
    {
        return !string.IsNullOrEmpty(apiKey) && APIKeyManager.Instance != null && 
               APIKeyManager.Instance.ValidateAPIKeyFormat();
    }
    #endregion

    #region Start/Finish Chat
    public void StartConversation(string Nameother)
    {
        // Add previous conversations
        int length = conversationsSummary.Count;
        string previousConversations = $@"
        ### Past conversations ###

        ";

        for (int i = 0; i < length; i++)
        {
            previousConversations += $"\nCharacter = **{conversationsSummary[i].otherName}**: ";
            previousConversations += $"{conversationsSummary[i].summary}\n";
        }


        // Create conversation and set up the first message
        conversation = new List<OpenAIMessage>();
        conversation.Add(new OpenAIMessage
        {
            role = "system",
            content = systemPrompt + previousConversations
        });

        // Must know with wwho are talking for future summary
        talkintToName = Nameother;
    }

    // Reset chat
    public async void FinishChat()
    {
        if (conversation == null || conversation.Count <2)
        {
            conversation.Clear();
            talkintToName = null;
            return;
        }
        // Take the conversation and send a request to the AI to summarize it
        string summary = await SummarizeConversation();

        bool found = false;

        for(int i = 0; i < conversationsSummary.Count; i++)
        {
            if (conversationsSummary[i].otherName == talkintToName)
            {
                conversationsSummary[i].summary += "\n" + summary;
                found = true;
                break;
            }
        }

        if (!found)
        {
            conversationsSummary.Add(new SummaryConversation
            {
                otherName = talkintToName,
                summary = summary
            });
        }

        conversation.Clear();
        talkintToName = null;
    }
    public string GetLastMessage()
    {
        if (conversation == null || conversation.Count == 1) return null;
        return conversation[conversation.Count - 1].content;
    }
    #endregion

    #region Summarize Conversation
    private async Task<string> SummarizeConversation()
    {
        // Build the conversation text (user + assistant only)
        StringBuilder sb = new StringBuilder();
        foreach (var msg in conversation)
        {
            if (msg.role == "user")
                sb.AppendLine($"User: {msg.content}");
            else if (msg.role == "assistant")
                sb.AppendLine($"Assistant: {msg.content}");
        }

        var summarizeRequest = new OpenAIRequest
        {
            model = "gpt-4o-mini",
            messages = new List<OpenAIMessage>
            {
                new OpenAIMessage
                {
                    role = "system",
                    content = "Summarize the following conversation in 2-3 sentences. " +
                            "Keep only important context that will help continue the discussion later."
                },
                new OpenAIMessage
                {
                    role = "user",
                    content = sb.ToString()
                }
            }
        };

        string jsonBody = JsonConvert.SerializeObject(summarizeRequest);

        using (var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions"))
        {
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            var parsed = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
            return parsed?.choices?[0]?.message?.content ?? "No summary available.";
        }
    }
    public string GetSummary(string nameOther)
    {
        int length = conversationsSummary.Count;
        string output = "";

        for (int i = 0; i < length; i++)
        {
            if (conversationsSummary[i].otherName == nameOther)
            {
                output = conversationsSummary[i].summary;
                break;
            }
        }

        return output;
    }
    #endregion

    #region Send Recive Prompts
    public async Task SendPrompt(string message)
    {
        await SendMessageToChatGPT(message);
    }
    private async Task SendMessageToChatGPT(string message)
    {
        // Check if API key is valid before making request
        if (!HasValidAPIKey())
        {
            Debug.LogError("No valid API key found. Please check your API key setup in APIKeyManager.");
            ManageAnswer("Error: API key not configured. Please contact support.");
            return;
        }

        conversation.Add(new OpenAIMessage { role = "user", content = message });

        var requestBody = new OpenAIRequest
        {
            model = "gpt-4o-mini",
            messages = conversation
        };

        string jsonBody = JsonConvert.SerializeObject(requestBody);

        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions"))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                string responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogError($"API request failed: {response.StatusCode} - {responseText}");
                    ManageAnswer($"Error: API request failed. Please check your API key and try again.");
                    return;
                }

                var parsed = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
                string answer = parsed?.choices?[0]?.message?.content ?? "No response";

                conversation.Add(new OpenAIMessage { role = "assistant", content = answer });
                ManageAnswer(answer);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error sending message to ChatGPT: {e.Message}");
            ManageAnswer("Error: Unable to connect to OpenAI. Please check your internet connection and try again.");
        }
    }

    private void ManageAnswer(string answer)
    {
        // Add to memory
        conversation.Add(new OpenAIMessage { role = "assistant", content = answer });

        // Call DialogueManager to update the dialogue UI
        DialogueManager.Instance.MessageRecived(character_name, answer);
    }
    #endregion

}
