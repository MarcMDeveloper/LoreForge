using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Secure API Key Manager for WebGL builds
/// 
/// Setup Instructions:
/// 1. Copy api_config_template.txt to api_config.txt
/// 2. Add your OpenAI API key: API_KEY=sk-your-key-here
/// 3. The api_config.txt file is automatically ignored by git
/// 
/// Alternative: Set OPENAI_API_KEY environment variable
/// </summary>
public class APIKeyManager : MonoBehaviour
{
    private static APIKeyManager instance;
    public static APIKeyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<APIKeyManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("APIKeyManager");
                    instance = go.AddComponent<APIKeyManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    // API key will be loaded from external source or environment
    private string cachedAPIKey = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private string workerUrl = "https://my-worker.loreforge.workers.dev";

     public IEnumerator CallWorker(string jsonPayload, System.Action<string> onSuccess = null, System.Action<string> onError = null)
    {
        using (UnityWebRequest req = new UnityWebRequest(workerUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"Worker call failed: {req.error} | code: {req.responseCode} | body: {req.downloadHandler.text}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
            else
            {
                string response = req.downloadHandler.text;
                Debug.Log($"Worker success: {response}");
                onSuccess?.Invoke(response);
            }
        }
    }
    
    /// <summary>
    /// Gets the API key from secure storage via worker
    /// </summary>
    /// <returns>The API key</returns>
    public string GetAPIKey()
    {
        try
        {
            // Check if security is compromised
            if (WebGLSecurityManager.Instance != null && WebGLSecurityManager.Instance.IsSecurityCompromised())
            {
                Debug.LogError("Security compromised - API key access denied");
                return null;
            }

            if (cachedAPIKey != null)
                return cachedAPIKey;

            // Try to load from environment variable first (for development)
            string envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrEmpty(envKey))
            {
                cachedAPIKey = envKey;
                return cachedAPIKey;
            }

            // Use worker to retrieve API key
            StartCoroutine(GetAPIKeyFromWorker());
            
            // Return cached key if available, otherwise return null
            // The worker will update cachedAPIKey when it completes
            return cachedAPIKey;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error retrieving API key: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Coroutine to get API key from worker
    /// </summary>
    private IEnumerator GetAPIKeyFromWorker()
    {
        // Create request payload for API key
        string jsonPayload = "{\"action\":\"get_api_key\"}";
        
        yield return StartCoroutine(CallWorker(jsonPayload, 
            onSuccess: (response) => {
                try
                {
                    // Parse the response to extract the API key
                    // Assuming the worker returns JSON like: {"api_key": "sk-..."}
                    // You may need to adjust this based on your worker's response format
                    if (response.Contains("api_key"))
                    {
                        // Simple parsing - you might want to use a proper JSON parser
                        int startIndex = response.IndexOf("\"api_key\":\"") + 11;
                        int endIndex = response.IndexOf("\"", startIndex);
                        if (startIndex > 10 && endIndex > startIndex)
                        {
                            string apiKey = response.Substring(startIndex, endIndex - startIndex);
                            cachedAPIKey = apiKey;
                            Debug.Log("API key retrieved from worker successfully");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing API key from worker response: {e.Message}");
                }
            },
            onError: (error) => {
                Debug.LogError($"Failed to get API key from worker: {error}");
            }
        ));
    }

    /// <summary>
    /// Checks if an API key is available
    /// </summary>
    /// <returns>True if API key exists, false otherwise</returns>
    public bool HasAPIKey()
    {
        string apiKey = GetAPIKey();
        return !string.IsNullOrEmpty(apiKey);
    }

    /// <summary>
    /// Validates if the API key is in correct format
    /// </summary>
    /// <returns>True if valid format, false otherwise</returns>
    public bool ValidateAPIKeyFormat()
    {
        string apiKey = GetAPIKey();
        if (string.IsNullOrEmpty(apiKey))
            return false;

        // OpenAI API keys typically start with "sk-" and are 51 characters long
        return apiKey.StartsWith("sk-") && apiKey.Length >= 50;
    }

    // Legacy methods for compatibility
    public void SetAPIKey(string apiKey)
    {
        Debug.LogWarning("SetAPIKey is not supported in this build. API key must be set externally.");
    }

    public void ClearAPIKey()
    {
        cachedAPIKey = null;

    }
}
