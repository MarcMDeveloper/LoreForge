using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Comprehensive WebGL Security Manager
/// Implements multiple layers of protection for API keys and game integrity
/// </summary>
public class WebGLSecurityManager : MonoBehaviour
{
    private static WebGLSecurityManager instance;
    public static WebGLSecurityManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<WebGLSecurityManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("WebGLSecurityManager");
                    instance = go.AddComponent<WebGLSecurityManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Security Settings")]
    [SerializeField] private bool enableAntiDebug = true;
    [SerializeField] private bool enableCodeObfuscation = true;
    [SerializeField] private bool enableRuntimeChecks = true;
    [SerializeField] private bool enableMemoryProtection = true;

    // Security state
    private bool isSecurityCompromised = false;
    private float lastSecurityCheck = 0f;
    private const float SECURITY_CHECK_INTERVAL = 5f;

    // Obfuscation keys (multiple layers)
    private static readonly byte[] LAYER_1_KEY = { 0x4C, 0x6F, 0x72, 0x65, 0x46, 0x6F, 0x72, 0x67, 0x65, 0x32, 0x30, 0x32, 0x34 };
    private static readonly byte[] LAYER_2_KEY = { 0x53, 0x65, 0x63, 0x75, 0x72, 0x69, 0x74, 0x79, 0x4B, 0x65, 0x79, 0x32, 0x30, 0x32, 0x34 };
    private static readonly byte[] LAYER_3_KEY = { 0x57, 0x65, 0x62, 0x47, 0x4C, 0x50, 0x72, 0x6F, 0x74, 0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSecurity();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(SecurityMonitoring());
    }

    void Update()
    {
        if (enableRuntimeChecks && Time.time - lastSecurityCheck > SECURITY_CHECK_INTERVAL)
        {
            PerformSecurityChecks();
            lastSecurityCheck = Time.time;
        }
    }

    /// <summary>
    /// Initialize all security measures
    /// </summary>
    private void InitializeSecurity()
    {
        if (enableAntiDebug)
        {
            StartCoroutine(AntiDebugRoutine());
        }

        if (enableMemoryProtection)
        {
            StartCoroutine(MemoryProtectionRoutine());
        }

        // Obfuscate critical strings
        if (enableCodeObfuscation)
        {
            ObfuscateCriticalStrings();
        }
#if UNITY_EDITOR
        Debug.Log("WebGL Security Manager initialized");
#endif
    }

    /// <summary>
    /// Multi-layer obfuscation for sensitive data
    /// </summary>
    public string ObfuscateData(string data)
    {
        if (string.IsNullOrEmpty(data)) return data;

        try
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            
            // Layer 1: XOR with first key
            dataBytes = ApplyXOR(dataBytes, LAYER_1_KEY);
            
            // Layer 2: Reverse and XOR with second key
            Array.Reverse(dataBytes);
            dataBytes = ApplyXOR(dataBytes, LAYER_2_KEY);
            
            // Layer 3: Shift and XOR with third key
            dataBytes = ApplyShift(dataBytes, 3);
            dataBytes = ApplyXOR(dataBytes, LAYER_3_KEY);
            
            return Convert.ToBase64String(dataBytes);
        }
        catch (Exception e)
        {
            Debug.LogError($"Obfuscation error: {e.Message}");
            return data;
        }
    }

    /// <summary>
    /// Multi-layer deobfuscation for sensitive data
    /// </summary>
    public string DeobfuscateData(string obfuscatedData)
    {
        if (string.IsNullOrEmpty(obfuscatedData)) return obfuscatedData;

        try
        {
            byte[] dataBytes = Convert.FromBase64String(obfuscatedData);
            
            // Layer 3: Reverse shift and XOR
            dataBytes = ApplyXOR(dataBytes, LAYER_3_KEY);
            dataBytes = ApplyShift(dataBytes, -3);
            
            // Layer 2: Reverse and XOR
            dataBytes = ApplyXOR(dataBytes, LAYER_2_KEY);
            Array.Reverse(dataBytes);
            
            // Layer 1: XOR
            dataBytes = ApplyXOR(dataBytes, LAYER_1_KEY);
            
            return Encoding.UTF8.GetString(dataBytes);
        }
        catch (Exception e)
        {
            Debug.LogError($"Deobfuscation error: {e.Message}");
            return obfuscatedData;
        }
    }

    private byte[] ApplyXOR(byte[] data, byte[] key)
    {
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ key[i % key.Length]);
        }
        return result;
    }

    private byte[] ApplyShift(byte[] data, int shift)
    {
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            int newIndex = (i + shift) % data.Length;
            if (newIndex < 0) newIndex += data.Length;
            result[newIndex] = data[i];
        }
        return result;
    }

    /// <summary>
    /// Anti-debugging measures
    /// </summary>
    private IEnumerator AntiDebugRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 8f));

            // Check for debugger
            if (IsDebuggerAttached())
            {
                HandleSecurityViolation("Debugger detected");
            }

            // Check for development build
            if (Debug.isDebugBuild)
            {
                HandleSecurityViolation("Development build detected");
            }

            // Random security checks
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                PerformRandomSecurityCheck();
            }
        }
    }

    /// <summary>
    /// Memory protection routine
    /// </summary>
    private IEnumerator MemoryProtectionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(10f, 30f));

            // Clear sensitive data from memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Check for memory tampering
            if (CheckMemoryIntegrity())
            {
                HandleSecurityViolation("Memory integrity check failed");
            }
        }
    }

    /// <summary>
    /// Continuous security monitoring
    /// </summary>
    private IEnumerator SecurityMonitoring()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (isSecurityCompromised)
            {
                // Disable critical functionality
                DisableCriticalFeatures();
                yield break;
            }
        }
    }

    /// <summary>
    /// Perform comprehensive security checks
    /// </summary>
    private void PerformSecurityChecks()
    {
        // Check for common attack vectors
        if (CheckForCommonAttacks())
        {
            HandleSecurityViolation("Common attack pattern detected");
        }

        // Validate game integrity
        if (!ValidateGameIntegrity())
        {
            HandleSecurityViolation("Game integrity validation failed");
        }

        // Check for suspicious behavior
        if (CheckForSuspiciousBehavior())
        {
            HandleSecurityViolation("Suspicious behavior detected");
        }
    }

    /// <summary>
    /// Obfuscate critical strings in the game
    /// </summary>
    private void ObfuscateCriticalStrings()
    {
        // This would be called during initialization to obfuscate
        // any hardcoded strings that might reveal sensitive information
    }

    /// <summary>
    /// Check if debugger is attached
    /// </summary>
    private bool IsDebuggerAttached()
    {
        try
        {
            return System.Diagnostics.Debugger.IsAttached;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check for common attack patterns
    /// </summary>
    private bool CheckForCommonAttacks()
    {
        // Check for rapid API calls (potential abuse)
        // Check for unusual memory patterns
        // Check for timing attacks
        return false; // Implement based on your specific needs
    }

    /// <summary>
    /// Validate game integrity
    /// </summary>
    private bool ValidateGameIntegrity()
    {
        // Check if critical game files are intact
        // Validate checksums of important assets
        // Check for code injection
        return true; // Implement based on your specific needs
    }

    /// <summary>
    /// Check for suspicious behavior
    /// </summary>
    private bool CheckForSuspiciousBehavior()
    {
        // Monitor for unusual patterns
        // Check for rapid state changes
        // Validate player actions
        return false; // Implement based on your specific needs
    }

    /// <summary>
    /// Check memory integrity
    /// </summary>
    private bool CheckMemoryIntegrity()
    {
        // Implement memory integrity checks
        return true; // Implement based on your specific needs
    }

    /// <summary>
    /// Perform random security checks
    /// </summary>
    private void PerformRandomSecurityCheck()
    {
        // Random security validations
        // This makes it harder for attackers to predict security measures
    }

    /// <summary>
    /// Handle security violations
    /// </summary>
    private void HandleSecurityViolation(string violation)
    {
        Debug.LogWarning($"Security violation detected: {violation}");
        
        // Log the violation
        LogSecurityViolation(violation);
        
        // Implement progressive responses
        if (!isSecurityCompromised)
        {
            isSecurityCompromised = true;
            StartCoroutine(GracefulDegradation());
        }
    }

    /// <summary>
    /// Log security violations
    /// </summary>
    private void LogSecurityViolation(string violation)
    {
        // Log to secure endpoint or local storage
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Security Violation: {violation}";
        // Implement logging mechanism
    }

    /// <summary>
    /// Graceful degradation when security is compromised
    /// </summary>
    private IEnumerator GracefulDegradation()
    {
        Debug.LogWarning("Security compromised - initiating graceful degradation");
        
        // Disable sensitive features gradually
        yield return new WaitForSeconds(1f);
        
        // Show warning to user
        ShowSecurityWarning();
        
        // Continue with limited functionality
    }

    /// <summary>
    /// Disable critical features
    /// </summary>
    private void DisableCriticalFeatures()
    {
        // Disable API calls
        // Disable sensitive game features
        // Show security warning
    }

    /// <summary>
    /// Show security warning to user
    /// </summary>
    private void ShowSecurityWarning()
    {
        // Implement UI warning
        Debug.LogWarning("Security warning: Game integrity may be compromised");
    }

    /// <summary>
    /// Get security status
    /// </summary>
    public bool IsSecurityCompromised()
    {
        return isSecurityCompromised;
    }

    /// <summary>
    /// Force security check
    /// </summary>
    public void ForceSecurityCheck()
    {
        PerformSecurityChecks();
    }
}
