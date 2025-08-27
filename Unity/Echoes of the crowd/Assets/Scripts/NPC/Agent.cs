using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking; // Added for UnityWebRequest

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

[System.Serializable]
public class APIKeyResponse
{
    public bool success;
    public string api_key;
    public string message;
    public string timestamp;
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

    // Secure API key management - shared across all Agent instances
    private static string sharedApiKey;
    private static bool isRetrievingSharedAPIKey = false;
    
    // Instance property that uses shared key
    private string apiKey
    {
        get { return sharedApiKey; }
        set { sharedApiKey = value; }
    }
    
    // Instance property that uses shared flag
    private bool isRetrievingAPIKey
    {
        get { return isRetrievingSharedAPIKey; }
        set { isRetrievingSharedAPIKey = value; }
    }

    // See if can be done different

    #endregion


    // Default constructor (might be used by Unity serialization)
    public Agent()
    {
        character_name = "Unknown";
        InitializeAgent();
    }

    public Agent(string sysPr, string characterName)
    {
        character_name = characterName;
        systemPrompt = sysPr;
        InitializeAgent();
    }

    private void InitializeAgent()
    {
        // Initialize collections
        if (conversation == null)
            conversation = new List<OpenAIMessage>();
        
        if (conversationsSummary == null)
            conversationsSummary = new List<SummaryConversation>();
        
        // Initialize API key from secure storage
        InitializeAPIKey();
    }

    #region API Key Management
    private void InitializeAPIKey()
    {
        // In WebGL mode, the Worker automatically injects the API key
        // No need for separate API key retrieval - just mark as "ready"
        apiKey = "worker-managed";
    }

    private void GetAPIKeyFromWorker(System.Action<string> callback)
    {
        Debug.LogError($"[Agent] {character_name}: Cannot retrieve API key due to Unity WebGL CORS limitations");
        callback?.Invoke(null);
    }
    


    public bool HasValidAPIKey()
    {
        // Safety check: if apiKey is null and we haven't initialized yet, try to initialize
        if (apiKey == null && conversation == null)
        {
            Debug.LogWarning($"[Agent] {character_name}: HasValidAPIKey called but Agent not initialized! Initializing now...");
            InitializeAgent();
        }
        
        return !string.IsNullOrEmpty(apiKey);
    }

    /// <summary>
    /// Get the API key - now properly retrieved from Worker
    /// </summary>
    private string GetDecodedAPIKey()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogWarning($"[Agent] {character_name}: GetDecodedAPIKey called but apiKey is null/empty");
        }
        return apiKey;
    }

    
    #endregion

    #region Start/Finish Chat
    public void StartConversation(string Nameother)
    {

        talkintToName = Nameother;
        
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


        // Wait for API key if needed
        if (!HasValidAPIKey())
        {

            await WaitForAPIKey();
            
            if (!HasValidAPIKey())
            {
                Debug.LogError($"[WebGL] {character_name}: Still no API key after waiting (Summarization)");
                return "No summary available - API key unavailable.";
            }
        }

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

        try
        {
            string decodedApiKey = GetDecodedAPIKey();
            


            string responseText = await SendWebRequest(jsonBody, decodedApiKey);
            
            if (string.IsNullOrEmpty(responseText))
            {
                Debug.LogError($"[WebGL] {character_name}: Summarization request returned null response");
                return "No summary available - API request failed.";
            }

            var parsed = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
            string summary = parsed?.choices?[0]?.message?.content ?? "No summary available.";

            return summary;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WebGL] {character_name}: Error summarizing conversation: {e.Message}");
            return "No summary available - API error.";
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

    public async Task<string> SendPromptSilent(string message)
    {

        return await SendMessageToChatGPTSilent(message);
    }

    private async Task SendMessageToChatGPT(string message)
    {

        
        // Wait for API key if needed
        if (!HasValidAPIKey())
        {

            await WaitForAPIKey();
            
            if (!HasValidAPIKey())
            {
                Debug.LogError($"[WebGL] {character_name}: Still no API key after waiting");
                ManageAnswer("Error: Unable to retrieve API key. Please check your connection and try again.");
                return;
            }
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
            string decodedApiKey = GetDecodedAPIKey();
            




            string responseText = await SendWebRequest(jsonBody, decodedApiKey);
            
            if (string.IsNullOrEmpty(responseText))
            {
                Debug.LogError($"[WebGL] {character_name}: Request returned null response");
                ManageAnswer("Error: Unable to connect to OpenAI. Please check your internet connection and try again.");
                return;
            }


            var parsed = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
            string answer = parsed?.choices?[0]?.message?.content ?? "No response";



            conversation.Add(new OpenAIMessage { role = "assistant", content = answer });
            ManageAnswer(answer);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WebGL] {character_name}: Exception during API call: {e.Message}");
            Debug.LogError($"[WebGL] {character_name}: Exception type: {e.GetType().Name}");
            Debug.LogError($"[WebGL] {character_name}: Stack trace: {e.StackTrace}");
            ManageAnswer("Error: Unable to connect to OpenAI. Please check your internet connection and try again.");
        }
    }
    
    private async Task WaitForAPIKey()
    {

        
        int maxWaitTime = 10; // 10 seconds max
        int waitedTime = 0;
        
        while (!HasValidAPIKey() && waitedTime < maxWaitTime)
        {
            await Task.Delay(500); // Wait 500ms
            waitedTime++;

        }
        
        if (HasValidAPIKey())
        {

        }
        else
        {
            Debug.LogError($"[Agent] {character_name}: API key timeout after {maxWaitTime * 0.5f}s");
        }
    }

    private async Task<string> SendMessageToChatGPTSilent(string message)
    {


        // Wait for API key if needed
        if (!HasValidAPIKey())
        {

            await WaitForAPIKey();
            
            if (!HasValidAPIKey())
            {
                Debug.LogError($"[WebGL] {character_name}: Still no API key after waiting (Silent)");
                return "Error: Unable to retrieve API key.";
            }
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
            string decodedApiKey = GetDecodedAPIKey();
            




            string responseText = await SendWebRequest(jsonBody, decodedApiKey);
            
            if (string.IsNullOrEmpty(responseText))
            {
                Debug.LogError($"[WebGL] {character_name}: Request returned null response (Silent)");
                return "Error: Unable to connect to OpenAI.";
            }


            var parsed = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
            string answer = parsed?.choices?[0]?.message?.content ?? "No response";



            conversation.Add(new OpenAIMessage { role = "assistant", content = answer });
            return answer;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WebGL] {character_name}: Exception during API call (Silent): {e.Message}");
            Debug.LogError($"[WebGL] {character_name}: Exception type (Silent): {e.GetType().Name}");
            Debug.LogError($"[WebGL] {character_name}: Stack trace (Silent): {e.StackTrace}");
            return "Error: Unable to connect to OpenAI.";
        }
    }

    // WebGL-compatible HTTP client using Coroutine approach
    private async Task<string> SendWebRequest(string jsonBody, string apiKey)
    {



        
        // Use a TaskCompletionSource to convert coroutine to async/await
        var tcs = new TaskCompletionSource<string>();
        
        // Start the coroutine using a MonoBehaviour (we'll need to find one in the scene)
        var coroutineRunner = UnityEngine.Object.FindObjectOfType<MonoBehaviour>();
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(SendWebRequestCoroutine(jsonBody, tcs));
        }
        else
        {
            Debug.LogError($"[WebGL] {character_name}: No MonoBehaviour found to run coroutine!");
            return null;
        }
        
        return await tcs.Task;
    }
    
    private System.Collections.IEnumerator SendWebRequestCoroutine(string jsonBody, TaskCompletionSource<string> tcs)
    {

        
        string workerUrl = "https://my-worker.loreforge.workers.dev";

        
        // Create UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(workerUrl, "POST"))
        {
            // Set up request body
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Set headers - DO NOT include Authorization, let Worker handle it
            request.SetRequestHeader("Content-Type", "application/json");
            
            // Add the same headers that work for API key requests (CORS compatibility)
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Cache-Control", "no-cache");


            
            // Send request using coroutine with timeout

            
            var operation = request.SendWebRequest();
            float startTime = Time.time;
            float timeoutSeconds = 10f;
            
            while (!operation.isDone && (Time.time - startTime) < timeoutSeconds)
            {
                yield return null;
            }
            
            if (!operation.isDone)
            {
                Debug.LogError($"[WebGL] {character_name}: Request TIMEOUT after {timeoutSeconds}s");
                request.Abort();
            }
            



            
            // Check for errors
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;



                
                tcs.SetResult(responseText);
            }
            else
            {
                Debug.LogError($"[WebGL] {character_name}: COROUTINE FAILED");
                Debug.LogError($"[WebGL] {character_name}: Result: {request.result}");
                Debug.LogError($"[WebGL] {character_name}: Response Code: {request.responseCode}");
                Debug.LogError($"[WebGL] {character_name}: Error: {request.error}");
                Debug.LogError($"[WebGL] {character_name}: Response text: {request.downloadHandler?.text}");
                
                tcs.SetResult(null);
            }
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
