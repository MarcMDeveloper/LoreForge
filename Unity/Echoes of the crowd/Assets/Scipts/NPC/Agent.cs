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
#endregion

public class Agent
{
    #region Variables
    private string systemPrompt;
    private List<OpenAIMessage> conversation;
    private List<string> conversationsSummary;

    // DANGEROUS TAKE OUT OF HERE
    private string apiKey = " ";

    // See if can be done different
    private static readonly HttpClient httpClient = new HttpClient();
    #endregion


    public Agent(string sysPr)
    {
        // Save system for now (Check later if really need it)
        systemPrompt = sysPr;

        Debug.Log("Agent created with system prompt: " + systemPrompt);

        // Create conversation and set up the first message
        conversation = new List<OpenAIMessage>();
        conversation.Add(new OpenAIMessage
        {
            role = "system",
            content = "### \n Role:\n" +
            systemPrompt +
            "\n ###\n" +
            "- Your behaviour will be defined by your role data \n" +
            "- If user uses agresivity or hate you can change the behaviour to agressive one if your personality goes with it\n  " +
            "- Your answers should be less than 100 tokens\n"
        });
    }

    public async Task SendPrompt(string message)
    {
        await SendMessageToChatGPT(message);
    }
    private async Task SendMessageToChatGPT(string message)
    {
         conversation.Add(new OpenAIMessage { role = "user", content = message });

        var requestBody = new OpenAIRequest
        {
            model = "gpt-4o-mini",
            messages = conversation
        };

        string jsonBody = JsonConvert.SerializeObject(requestBody);

        using (var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions"))
        {
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            Debug.Log("Raw Response: " + responseText);

            var parsed = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
            string answer = parsed?.choices?[0]?.message?.content ?? "No response";

            conversation.Add(new OpenAIMessage { role = "assistant", content = answer });

            ManageAnswer(answer);
        }
    }

    private void ManageAnswer(string answer)
    {
        // Add to memory
        conversation.Add(new OpenAIMessage { role = "assistant", content = answer });

        // Call DialogueManager to update the dialogue UI
        Debug.Log("Answer received: " + answer);
    }


    private void FinishChat()
    { 
        // Take the conversation and send a request to the AI to summarize it
    }
}
