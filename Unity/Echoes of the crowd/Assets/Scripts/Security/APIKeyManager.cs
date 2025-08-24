using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

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

    /// <summary>
    /// Gets the API key from secure storage
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

            // Try to load from a separate config file (not in version control)
            string configPath = Application.dataPath + "/../api_config.txt";
            if (System.IO.File.Exists(configPath))
            {
                string configContent = System.IO.File.ReadAllText(configPath);
                string[] lines = configContent.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Trim().StartsWith("API_KEY="))
                    {
                        string rawKey = line.Trim().Substring(8).Trim();
                        
                        // Apply additional obfuscation if WebGL security is available
                        if (WebGLSecurityManager.Instance != null)
                        {
                            cachedAPIKey = WebGLSecurityManager.Instance.DeobfuscateData(rawKey);
                        }
                        else
                        {
                            cachedAPIKey = rawKey;
                        }
                        
                        return cachedAPIKey;
                    }
                }
            }

            // Fallback: try to load from Resources (for build)
            TextAsset configAsset = Resources.Load<TextAsset>("api_config");
            if (configAsset != null)
            {
                string[] lines = configAsset.text.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Trim().StartsWith("API_KEY="))
                    {
                        string rawKey = line.Trim().Substring(8).Trim();
                        
                        // Apply additional obfuscation if WebGL security is available
                        if (WebGLSecurityManager.Instance != null)
                        {
                            cachedAPIKey = WebGLSecurityManager.Instance.DeobfuscateData(rawKey);
                        }
                        else
                        {
                            cachedAPIKey = rawKey;
                        }
                        
                        return cachedAPIKey;
                    }
                }
            }

            Debug.LogError("No API key found. Please set up the API key using one of the methods in the README.");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error retrieving API key: {e.Message}");
            return null;
        }
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
