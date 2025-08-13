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

    public Vector2 offset = new Vector2(150f, -100f);

    private void Start()
    {
        HideTooltip();
    }

    public void ShowTooltip(NPC npcData, RectTransform buttonTransform)
    {
        if (npcData == null) return;

        // Fill in data
        nameText.text = npcData.npc_name;
        detailsText.text =
            $"<b><color=#000000>Gender:</color></b> {npcData.gender}\n" +
            $"<b><color=#000000>Age:</color></b> {npcData.age}\n" +
            $"<b><color=#000000>Culture:</color></b> {npcData.culture}\n" +
            $"<b><color=#000000>Traits:</color></b> {string.Join(", ", npcData.traits)}\n" +
            $"<b><color=#000000>Goal:</color></b> {npcData.goal}\n" +
            $"<b><color=#000000>Occupation:</color></b> {npcData.occupation}\n" +
            $"<b><color=#000000>History:</color></b> {npcData.briefHistory}";

        // Load portrait (assuming it's a Resources image)
        /*Sprite portraitSprite = Resources.Load<Sprite>(npcData.portraitPath);
        if (portraitSprite != null)
            portraitImage.sprite = portraitSprite;*/

        // Reparent tooltip to the same parent as the button (same coordinate space)
        panel.transform.SetParent(buttonTransform.parent, false);

        // Position the tooltip right next to the button in local space
        panel.transform.SetParent(buttonTransform.parent, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = buttonTransform.anchoredPosition + offset;

        panel.SetActive(true);
    }

    public void HideTooltip()
    {
        panel.SetActive(false);
    }
}
