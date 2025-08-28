using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Secure API Key Manager for WebGL builds
/// 
/// Retrieves API keys from a secure worker endpoint
/// 
/// Alternative: Set OPENAI_API_KEY environment variable for development
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

    // API key will be loaded from worker or environment variable
    private string cachedAPIKey = null;
    private bool isLoadingAPIKey = false;
    private System.Action<string> pendingCallback = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[APIKeyManager] Instance created and set as DontDestroyOnLoad");
        }
        else if (instance != this)
        {
            Debug.Log("[APIKeyManager] Duplicate instance found, destroying this one");
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
                Debug.LogError($"[APIKeyManager] {errorMsg}");
                onError?.Invoke(errorMsg);
            }
            else
            {
                string response = req.downloadHandler.text;
                Debug.Log($"[APIKeyManager] Worker success: {response.Substring(0, Math.Min(100, response.Length))}...");
                Debug.Log($"[APIKeyManager] Full worker response: {response}");
                onSuccess?.Invoke(response);
            }
        }
    }
    
    /// <summary>
    /// Gets the API key from secure storage via worker (asynchronous)
    /// </summary>
    /// <param name="callback">Callback with the API key or null if failed</param>
    public void GetAPIKeyAsync(System.Action<string> callback)
    {        
        // If we already have the key cached, return it immediately
        if (cachedAPIKey != null)
        {
            callback?.Invoke(cachedAPIKey);
            return;
        }

        // Check if security is compromised
        if (WebGLSecurityManager.Instance != null && WebGLSecurityManager.Instance.IsSecurityCompromised())
        {
            Debug.LogError("[APIKeyManager] Security compromised - API key access denied");
            callback?.Invoke(null);
            return;
        }

        // Try to load from PlayerPrefs first (for development)
        string fallbackKey = PlayerPrefs.GetString("OPENAI_API_KEY", "");
        if (!string.IsNullOrEmpty(fallbackKey))
        {
            cachedAPIKey = fallbackKey;
            callback?.Invoke(cachedAPIKey);
            return;
        }

        // If already loading, add to pending callbacks
        if (isLoadingAPIKey)
        {
            pendingCallback = callback;
            return;
        }

        // Start loading from worker
        isLoadingAPIKey = true;
        StartCoroutine(GetAPIKeyFromWorker(callback));
    }

    /// <summary>
    /// Synchronous version - returns cached key or null (for backward compatibility)
    /// </summary>
    /// <returns>The cached API key or null if not loaded yet</returns>
    public string GetAPIKey()
    {
        return cachedAPIKey;
    }

    /// <summary>
    /// Coroutine to get API key from worker
    /// </summary>
    private IEnumerator GetAPIKeyFromWorker(System.Action<string> callback)
    {
        
        // Create request payload for API key
        string jsonPayload = "{\"action\":\"get_api_key\"}";
        
        yield return StartCoroutine(CallWorker(jsonPayload, 
            onSuccess: (response) => {
                try
                {
                    // Parse the response to extract the API key
                    if (response.Contains("api_key"))
                    {
                        Debug.Log("[APIKeyManager] Response contains api_key field");
                        // Simple parsing - you might want to use a proper JSON parser
                        int startIndex = response.IndexOf("\"api_key\":\"") + 11;
                        int endIndex = response.IndexOf("\"", startIndex);
                        if (startIndex > 10 && endIndex > startIndex)
                        {
                            string apiKey = response.Substring(startIndex, endIndex - startIndex);
                            cachedAPIKey = apiKey;
                            callback?.Invoke(apiKey);
                        }
                        else
                        {
                            Debug.LogError("[APIKeyManager] Failed to parse API key from response");
                            Debug.LogError($"[APIKeyManager] Response content: {response}");
                            callback?.Invoke(null);
                        }
                    }
                    else
                    {
                        Debug.LogError("[APIKeyManager] Response does not contain api_key field");
                        Debug.LogError($"[APIKeyManager] Full response: {response}");
                        callback?.Invoke(null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[APIKeyManager] Error parsing API key from worker response: {e.Message}");
                    Debug.LogError($"[APIKeyManager] Exception stack trace: {e.StackTrace}");
                    callback?.Invoke(null);
                }
                finally
                {
                    isLoadingAPIKey = false;
                    Debug.Log("[APIKeyManager] isLoadingAPIKey set to false");
                    // Handle any pending callbacks
                    if (pendingCallback != null)
                    {
                        Debug.Log("[APIKeyManager] Handling pending callback");
                        var pending = pendingCallback;
                        pendingCallback = null;
                        pending(cachedAPIKey);
                    }
                }
            },
            onError: (error) => {
                Debug.LogError($"[APIKeyManager] Worker error callback: {error}");
                isLoadingAPIKey = false;
                Debug.Log("[APIKeyManager] isLoadingAPIKey set to false due to error");
                callback?.Invoke(null);
                // Handle any pending callbacks
                if (pendingCallback != null)
                {
                    Debug.Log("[APIKeyManager] Handling pending callback after error");
                    var pending = pendingCallback;
                    pendingCallback = null;
                    pending(null);
                }
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
        bool hasKey = !string.IsNullOrEmpty(apiKey);
        return hasKey;
    }

    /// <summary>
    /// Validates if the API key is in correct format
    /// </summary>
    /// <returns>True if valid format, false otherwise</returns>
    public bool ValidateAPIKeyFormat()
    {
        string apiKey = GetAPIKey();
                
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.Log("[APIKeyManager] API key is null or empty - validation failed");
            return false;
        }

        // Check if it's a base64 encoded key (from worker) or raw OpenAI key
        if (apiKey.StartsWith("sk-") && apiKey.Length >= 50)
        {
            return true;
        }
        else if (apiKey.Length >= 50 && IsBase64String(apiKey))
        {
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Checks if a string is valid base64
    /// </summary>
    private bool IsBase64String(string base64)
    {
        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[APIKeyManager] Base64 validation failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clears the cached API key
    /// </summary>
    public void ClearAPIKey()
    {
        cachedAPIKey = null;
        isLoadingAPIKey = false;
        pendingCallback = null;
    }

    /// <summary>
    /// Test method to verify worker connectivity
    /// </summary>
    public void TestWorkerConnectivity()
    {
        Debug.Log("[APIKeyManager] TestWorkerConnectivity called");
        StartCoroutine(TestWorkerConnectivityCoroutine());
    }

    private IEnumerator TestWorkerConnectivityCoroutine()
    {
        Debug.Log("[APIKeyManager] TestWorkerConnectivityCoroutine started");
        
        // Test health check endpoint
        using (UnityWebRequest req = new UnityWebRequest(workerUrl, "GET"))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            
            Debug.Log($"[APIKeyManager] Testing worker health check at: {workerUrl}");
            yield return req.SendWebRequest();
            
            Debug.Log($"[APIKeyManager] Health check result: {req.result}");
            Debug.Log($"[APIKeyManager] Health check response code: {req.responseCode}");
            Debug.Log($"[APIKeyManager] Health check response: {req.downloadHandler.text}");
            
            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[APIKeyManager] Worker health check SUCCESS");
            }
            else
            {
                Debug.LogError($"[APIKeyManager] Worker health check FAILED: {req.error}");
            }
        }
    }

    /// <summary>
    /// Test method specifically for localhost debugging
    /// </summary>
    public void TestWorkerLocalhost()
    {
        Debug.Log("[APIKeyManager] TestWorkerLocalhost called");
        StartCoroutine(TestWorkerLocalhostCoroutine());
    }

    private IEnumerator TestWorkerLocalhostCoroutine()
    {
        Debug.Log("[APIKeyManager] TestWorkerLocalhostCoroutine started");
        
        // Test the new test endpoint
        string testUrl = workerUrl + "/test";
        using (UnityWebRequest req = new UnityWebRequest(testUrl, "GET"))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            
            Debug.Log($"[APIKeyManager] Testing worker test endpoint at: {testUrl}");
            yield return req.SendWebRequest();
            
            Debug.Log($"[APIKeyManager] Test endpoint result: {req.result}");
            Debug.Log($"[APIKeyManager] Test endpoint response code: {req.responseCode}");
            Debug.Log($"[APIKeyManager] Test endpoint response: {req.downloadHandler.text}");
            
            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[APIKeyManager] Worker test endpoint SUCCESS");
            }
            else
            {
                Debug.LogError($"[APIKeyManager] Worker test endpoint FAILED: {req.error}");
            }
        }
    }
}
