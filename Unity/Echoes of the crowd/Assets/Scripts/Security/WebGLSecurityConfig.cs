using UnityEngine;

/// <summary>
/// WebGL Security Configuration
/// Configure all security settings for your WebGL build
/// </summary>
[CreateAssetMenu(fileName = "WebGLSecurityConfig", menuName = "LoreForge/Security/WebGL Security Config")]
public class WebGLSecurityConfig : ScriptableObject
{
    [Header("API Key Security")]
    [SerializeField] private bool enableAPIKeyObfuscation = true;
    [SerializeField] private bool enableAPIKeyValidation = true;
    [SerializeField] private bool enableAPIKeyRateLimiting = true;
    [SerializeField] private int maxAPIRequestsPerMinute = 60;

    [Header("Anti-Debugging")]
    [SerializeField] private bool enableAntiDebug = true;
    [SerializeField] private bool enableDevelopmentBuildDetection = true;
    [SerializeField] private bool enableDebuggerDetection = true;
    [SerializeField] private float antiDebugCheckInterval = 5f;

    [Header("Memory Protection")]
    [SerializeField] private bool enableMemoryProtection = true;
    [SerializeField] private bool enableGarbageCollection = true;
    [SerializeField] private bool enableMemoryIntegrityChecks = true;
    [SerializeField] private float memoryProtectionInterval = 30f;

    [Header("Runtime Security")]
    [SerializeField] private bool enableRuntimeChecks = true;
    [SerializeField] private bool enableGameIntegrityValidation = true;
    [SerializeField] private bool enableSuspiciousBehaviorDetection = true;
    [SerializeField] private float runtimeCheckInterval = 10f;

    [Header("Code Obfuscation")]
    [SerializeField] private bool enableCodeObfuscation = true;
    [SerializeField] private bool enableStringObfuscation = true;
    [SerializeField] private bool enableVariableObfuscation = true;

    [Header("Response Actions")]
    [SerializeField] private SecurityResponseLevel securityResponseLevel = SecurityResponseLevel.GracefulDegradation;
    [SerializeField] private bool enableSecurityLogging = true;
    [SerializeField] private bool enableUserWarnings = true;
    [SerializeField] private bool enableAutomaticRecovery = false;

    [Header("Advanced Settings")]
    [SerializeField] private bool enableTimingAttacks = true;
    [SerializeField] private bool enablePatternDetection = true;
    [SerializeField] private bool enableProgressiveResponse = true;
    [SerializeField] private int maxSecurityViolations = 3;

    public enum SecurityResponseLevel
    {
        WarningOnly,
        GracefulDegradation,
        FeatureDisable,
        CompleteShutdown
    }

    // Public properties for access
    public bool EnableAPIKeyObfuscation => enableAPIKeyObfuscation;
    public bool EnableAPIKeyValidation => enableAPIKeyValidation;
    public bool EnableAPIKeyRateLimiting => enableAPIKeyRateLimiting;
    public int MaxAPIRequestsPerMinute => maxAPIRequestsPerMinute;
    public bool EnableAntiDebug => enableAntiDebug;
    public bool EnableDevelopmentBuildDetection => enableDevelopmentBuildDetection;
    public bool EnableDebuggerDetection => enableDebuggerDetection;
    public float AntiDebugCheckInterval => antiDebugCheckInterval;
    public bool EnableMemoryProtection => enableMemoryProtection;
    public bool EnableGarbageCollection => enableGarbageCollection;
    public bool EnableMemoryIntegrityChecks => enableMemoryIntegrityChecks;
    public float MemoryProtectionInterval => memoryProtectionInterval;
    public bool EnableRuntimeChecks => enableRuntimeChecks;
    public bool EnableGameIntegrityValidation => enableGameIntegrityValidation;
    public bool EnableSuspiciousBehaviorDetection => enableSuspiciousBehaviorDetection;
    public float RuntimeCheckInterval => runtimeCheckInterval;
    public bool EnableCodeObfuscation => enableCodeObfuscation;
    public bool EnableStringObfuscation => enableStringObfuscation;
    public bool EnableVariableObfuscation => enableVariableObfuscation;
    public SecurityResponseLevel ResponseLevel => securityResponseLevel;
    public bool EnableSecurityLogging => enableSecurityLogging;
    public bool EnableUserWarnings => enableUserWarnings;
    public bool EnableAutomaticRecovery => enableAutomaticRecovery;
    public bool EnableTimingAttacks => enableTimingAttacks;
    public bool EnablePatternDetection => enablePatternDetection;
    public bool EnableProgressiveResponse => enableProgressiveResponse;
    public int MaxSecurityViolations => maxSecurityViolations;

    /// <summary>
    /// Get recommended security settings for production
    /// </summary>
    public static WebGLSecurityConfig GetProductionConfig()
    {
        var config = CreateInstance<WebGLSecurityConfig>();
        
        // Maximum security for production
        config.enableAPIKeyObfuscation = true;
        config.enableAPIKeyValidation = true;
        config.enableAPIKeyRateLimiting = true;
        config.maxAPIRequestsPerMinute = 30;
        config.enableAntiDebug = true;
        config.enableDevelopmentBuildDetection = true;
        config.enableDebuggerDetection = true;
        config.antiDebugCheckInterval = 3f;
        config.enableMemoryProtection = true;
        config.enableGarbageCollection = true;
        config.enableMemoryIntegrityChecks = true;
        config.memoryProtectionInterval = 15f;
        config.enableRuntimeChecks = true;
        config.enableGameIntegrityValidation = true;
        config.enableSuspiciousBehaviorDetection = true;
        config.runtimeCheckInterval = 5f;
        config.enableCodeObfuscation = true;
        config.enableStringObfuscation = true;
        config.enableVariableObfuscation = true;
        config.securityResponseLevel = SecurityResponseLevel.GracefulDegradation;
        config.enableSecurityLogging = true;
        config.enableUserWarnings = true;
        config.enableAutomaticRecovery = false;
        config.enableTimingAttacks = true;
        config.enablePatternDetection = true;
        config.enableProgressiveResponse = true;
        config.maxSecurityViolations = 2;

        return config;
    }

    /// <summary>
    /// Get recommended security settings for development
    /// </summary>
    public static WebGLSecurityConfig GetDevelopmentConfig()
    {
        var config = CreateInstance<WebGLSecurityConfig>();
        
        // Relaxed security for development
        config.enableAPIKeyObfuscation = false;
        config.enableAPIKeyValidation = true;
        config.enableAPIKeyRateLimiting = false;
        config.maxAPIRequestsPerMinute = 100;
        config.enableAntiDebug = false;
        config.enableDevelopmentBuildDetection = false;
        config.enableDebuggerDetection = false;
        config.antiDebugCheckInterval = 30f;
        config.enableMemoryProtection = false;
        config.enableGarbageCollection = true;
        config.enableMemoryIntegrityChecks = false;
        config.memoryProtectionInterval = 60f;
        config.enableRuntimeChecks = false;
        config.enableGameIntegrityValidation = false;
        config.enableSuspiciousBehaviorDetection = false;
        config.runtimeCheckInterval = 60f;
        config.enableCodeObfuscation = false;
        config.enableStringObfuscation = false;
        config.enableVariableObfuscation = false;
        config.securityResponseLevel = SecurityResponseLevel.WarningOnly;
        config.enableSecurityLogging = true;
        config.enableUserWarnings = false;
        config.enableAutomaticRecovery = true;
        config.enableTimingAttacks = false;
        config.enablePatternDetection = false;
        config.enableProgressiveResponse = false;
        config.maxSecurityViolations = 10;

        return config;
    }

    /// <summary>
    /// Validate configuration settings
    /// </summary>
    public bool ValidateConfiguration()
    {
        bool isValid = true;

        if (maxAPIRequestsPerMinute <= 0)
        {
            Debug.LogError("Max API requests per minute must be greater than 0");
            isValid = false;
        }

        if (antiDebugCheckInterval <= 0)
        {
            Debug.LogError("Anti-debug check interval must be greater than 0");
            isValid = false;
        }

        if (memoryProtectionInterval <= 0)
        {
            Debug.LogError("Memory protection interval must be greater than 0");
            isValid = false;
        }

        if (runtimeCheckInterval <= 0)
        {
            Debug.LogError("Runtime check interval must be greater than 0");
            isValid = false;
        }

        if (maxSecurityViolations <= 0)
        {
            Debug.LogError("Max security violations must be greater than 0");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// Get security level description
    /// </summary>
    public string GetSecurityLevelDescription()
    {
        int securityScore = 0;
        
        if (enableAPIKeyObfuscation) securityScore += 10;
        if (enableAPIKeyValidation) securityScore += 5;
        if (enableAPIKeyRateLimiting) securityScore += 5;
        if (enableAntiDebug) securityScore += 15;
        if (enableMemoryProtection) securityScore += 10;
        if (enableRuntimeChecks) securityScore += 10;
        if (enableCodeObfuscation) securityScore += 15;
        if (enableTimingAttacks) securityScore += 5;
        if (enablePatternDetection) securityScore += 5;
        if (enableProgressiveResponse) securityScore += 5;
        if (securityResponseLevel == SecurityResponseLevel.CompleteShutdown) securityScore += 10;
        if (securityResponseLevel == SecurityResponseLevel.GracefulDegradation) securityScore += 5;

        if (securityScore >= 80) return "Maximum Security";
        if (securityScore >= 60) return "High Security";
        if (securityScore >= 40) return "Medium Security";
        if (securityScore >= 20) return "Low Security";
        return "Minimal Security";
    }
}
