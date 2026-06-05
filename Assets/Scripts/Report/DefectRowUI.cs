using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DefectRowUI : MonoBehaviour
{
    public void BuildRow(DefectRow data)
    {
        var bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 4;
        layout.padding = new RectOffset(8, 8, 4, 4);
        layout.childAlignment = TextAnchor.MiddleLeft;

        var le = gameObject.AddComponent<LayoutElement>();
        le.minHeight = 50;
        le.preferredHeight = 50;

        // Testi fissi
        AddLabel(data.CodDifetto, 90f);
        AddLabel(data.Descrizione, 180f);

        // Toggles
        AddToggle(data.Visto, 45f, v => data.Visto = v);
        AddToggle(data.Estensione02, 45f, v => data.Estensione02 = v);
        AddToggle(data.Estensione05, 45f, v => data.Estensione05 = v);
        AddToggle(data.Estensione1, 45f, v => data.Estensione1 = v);
        AddToggle(data.Intensita02, 45f, v => data.Intensita02 = v);
        AddToggle(data.Intensita05, 45f, v => data.Intensita05 = v);
        AddToggle(data.Intensita1, 45f, v => data.Intensita1 = v);
        AddToggle(data.PS, 45f, v => data.PS = v);
        AddToggle(data.NA, 45f, v => data.NA = v);
        AddToggle(data.NR, 45f, v => data.NR = v);
        AddToggle(data.NP, 45f, v => data.NP = v);
        AddToggle(data.FOTO, 45f, v => data.FOTO = v);

        // Campo note
        AddInputField(data.Note, 120f, v => data.Note = v);
    }

    private void AddLabel(string text, float width)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(transform, false);
        var txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = text;
        txt.fontSize = 11;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        var le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
    }

    private void AddToggle(bool initialValue, float width, System.Action<bool> onChange)
    {
        var go = new GameObject("Toggle");
        go.transform.SetParent(transform, false);

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = 30;

        // Background
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f);

        var toggle = go.AddComponent<Toggle>();

        // Checkmark
        var checkGO = new GameObject("Checkmark");
        checkGO.transform.SetParent(go.transform, false);
        var checkImg = checkGO.AddComponent<Image>();
        checkImg.color = new Color(0.2f, 0.8f, 0.2f);
        var checkRect = checkGO.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.1f, 0.1f);
        checkRect.anchorMax = new Vector2(0.9f, 0.9f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        toggle.targetGraphic = bg;
        toggle.graphic = checkImg;
        toggle.isOn = initialValue;
        toggle.onValueChanged.AddListener(v => onChange(v));
    }

    private void AddInputField(string initialValue, float width, System.Action<string> onChange)
    {
        var go = new GameObject("InputField");
        go.transform.SetParent(transform, false);

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = width;
        le.preferredWidth = width;
        le.minHeight = 40;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f);

        var inputField = go.AddComponent<TMP_InputField>();

        var textArea = new GameObject("Text Area");
        textArea.transform.SetParent(go.transform, false);
        var taRect = textArea.AddComponent<RectTransform>();
        taRect.anchorMin = Vector2.zero;
        taRect.anchorMax = Vector2.one;
        taRect.offsetMin = new Vector2(4, 2);
        taRect.offsetMax = new Vector2(-4, -2);
        textArea.AddComponent<RectMask2D>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(textArea.transform, false);
        var txt = textGO.AddComponent<TextMeshProUGUI>();
        txt.fontSize = 11;
        txt.color = Color.white;
        var txtRect = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        inputField.textViewport = taRect;
        inputField.textComponent = txt;
        inputField.text = initialValue;
        inputField.onEndEdit.AddListener(v => onChange(v));
    }
}