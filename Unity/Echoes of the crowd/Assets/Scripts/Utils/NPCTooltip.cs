using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCTooltip : MonoBehaviour
{
    public GameObject panel;          // The tooltip panel
    public Image portraitImage;       // Portrait UI element
    public TMP_Text nameText;
    public TMP_Text detailsText;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    
    [Header("Styling")]
    public Color nameTextColor = Color.white;
    public Color labelColor = new Color(0.8f, 0.8f, 0.8f);
    public Color valueColor = Color.white;
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    public int nameFontSize = 18;
    public int detailsFontSize = 14;
    
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        // Get or add CanvasGroup for fade animations
        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();
            
        // Apply initial styling
        ApplyStyling();
        
        HideTooltip();
    }
    
    private void ApplyStyling()
    {
        // Style the panel background
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = backgroundColor;
            
        // Style the name text
        if (nameText != null)
        {
            nameText.color = nameTextColor;
            nameText.fontSize = nameFontSize;
            nameText.fontStyle = FontStyles.Bold;
        }
        
        // Style the details text
        if (detailsText != null)
        {
            detailsText.color = valueColor;
            detailsText.fontSize = detailsFontSize;
        }
    }

    public void ShowTooltip(NPC npcData, RectTransform buttonTransform)
    {
        if (npcData == null) return;

        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // Fill in data with enhanced formatting
        nameText.text = npcData.npc_name;
        detailsText.text = FormatNPCDetails(npcData);

        // Load and display portrait
        LoadPortrait(npcData);

        // Show panel and start fade in
        panel.SetActive(true);
        fadeCoroutine = StartCoroutine(FadeIn());
    }
    
    private string FormatNPCDetails(NPC npcData)
    {
        string labelColorHex = ColorUtility.ToHtmlStringRGB(labelColor);
        string valueColorHex = ColorUtility.ToHtmlStringRGB(valueColor);
        
        return $"<b><color=#{labelColorHex}>Gender:</color></b> <color=#{valueColorHex}>{npcData.gender}</color>\n" +
               $"<b><color=#{labelColorHex}>Age:</color></b> <color=#{valueColorHex}>{npcData.age}</color>\n" +
               $"<b><color=#{labelColorHex}>Culture:</color></b> <color=#{valueColorHex}>{npcData.culture}</color>\n" +
               $"<b><color=#{labelColorHex}>Traits:</color></b> <color=#{valueColorHex}>{FormatTraits(npcData.traits)}</color>\n" +
               $"<b><color=#{labelColorHex}>Goal:</color></b> <color=#{valueColorHex}>{TruncateText(npcData.goal, 50)}</color>\n" +
               $"<b><color=#{labelColorHex}>Occupation:</color></b> <color=#{valueColorHex}>{npcData.occupation}</color>\n" +
               $"<b><color=#{labelColorHex}>History:</color></b> <color=#{valueColorHex}>{TruncateText(npcData.briefHistory, 300)}</color>";
    }
    
    private string FormatTraits(List<string> traits)
    {
        if (traits == null || traits.Count == 0)
            return "None";
            
        return string.Join(", ", traits);
    }
    
    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return "None";
            
        if (text.Length <= maxLength)
            return text;
            
        return text.Substring(0, maxLength) + "...";
    }
    
    private void LoadPortrait(NPC npcData)
    {
        if (portraitImage == null) return;
        
        // Try to load portrait from Resources
        if (!string.IsNullOrEmpty(npcData.portraitPath))
        {
            Sprite portraitSprite = Resources.Load<Sprite>(npcData.portraitPath);
            if (portraitSprite != null)
            {
                portraitImage.sprite = portraitSprite;
                portraitImage.gameObject.SetActive(true);
                return;
            }
        }
        
        // If no portrait found, hide the portrait image
        portraitImage.gameObject.SetActive(false);
    }

    public void HideTooltip()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        fadeCoroutine = StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        fadeCoroutine = null;
    }
    
    private IEnumerator FadeOut()
    {
        float startAlpha = canvasGroup.alpha;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        panel.SetActive(false);
        fadeCoroutine = null;
    }
}
