using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReportFormUI : MonoBehaviour
{
    private ReportData _reportData;
    private string _excelPath;
    private GameObject _formPanel;
    private TextMeshProUGUI _saveStatusText;


    void Start()
    {
        try
        {

            string fileName = "Scheda_01.xlsx";
            string persistentPath = Path.Combine(Application.persistentDataPath, fileName);

            // Copia il file da StreamingAssets a persistentDataPath se non esiste
            if (!File.Exists(persistentPath))
            {
                StartCoroutine(CopyAndLoad(fileName, persistentPath));
            }
            else
            {
                LoadAndBuild(persistentPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Errore in Start: {e.Message}");
        }
    }

    private System.Collections.IEnumerator CopyAndLoad(string fileName, string persistentPath)
    {
        string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);

        using var request = UnityEngine.Networking.UnityWebRequest.Get(streamingPath);
        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
            LoadAndBuild(persistentPath);
        }
        else
        {
            Debug.LogError($"Errore copia file: {request.error}");
        }
    }

    private void LoadAndBuild(string path)
    {
        _excelPath = path;
        _reportData = ExcelParser.Parse(_excelPath);
        BuildUI();
    }
    private void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("ReportCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.matchWidthOrHeight = 0.5f;

        // Bottone Su
        var upBtn = CreateButton(canvasGO.transform, "▲", new Vector2(60, 60),
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 40));

        upBtn.onClick.AddListener(() =>
        {
            var placer = Object.FindAnyObjectByType<ModelPlacer>();
            if (placer != null && placer.SpawnedModel != null)
                placer.SpawnedModel.transform.position += Vector3.up * 0.05f;
            else
            {
                // Fallback editor: muovi qualsiasi oggetto con TouchHandler
                var handler = Object.FindAnyObjectByType<TouchHandler>();
                if (handler != null)
                    handler.transform.position += Vector3.up * 0.05f;
            }
        });

        // Bottone Giù
        var downBtn = CreateButton(canvasGO.transform, "▼", new Vector2(60, 60),
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, -40));

        downBtn.onClick.AddListener(() =>
        {
            var placer = Object.FindAnyObjectByType<ModelPlacer>();
            if (placer != null && placer.SpawnedModel != null)
                placer.SpawnedModel.transform.position -= Vector3.up * 0.05f;
            else
            {
                // Fallback editor: muovi qualsiasi oggetto con TouchHandler
                var handler = Object.FindAnyObjectByType<TouchHandler>();
                if (handler != null)
                    handler.transform.position -= Vector3.up * 0.05f;
            }
        });

        // Bottone apri form
        var openBtn = CreateButton(canvasGO.transform, "📋 Report", new Vector2(120, 40),
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(70, 30));
        openBtn.onClick.AddListener(OpenForm);

        // Riferimento al ModelPlacer
        var modelPlacer = Object.FindAnyObjectByType<ModelPlacer>();

        // Bottone toggle modalità
        var toggleBtn = CreateButton(canvasGO.transform, "Modalità: Posiziona", new Vector2(200, 40),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30));
        toggleBtn.onClick.AddListener(() =>
        {
            var placer = Object.FindAnyObjectByType<ModelPlacer>();
            if (placer == null)
            {
                Debug.Log("ModelPlacer non trovato");
                return;
            }

            placer.ToggleMode();
            var label = toggleBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            label.text = placer.GetModeLabel();
        });
        
        // Pannello form (inizialmente nascosto)
        _formPanel = CreatePanel(canvasGO.transform, new Color(0, 0, 0, 0.92f));
        _formPanel.SetActive(false);

        // Titolo
        var title = CreateText(_formPanel.transform, "SCHEDA ISPEZIONE VEICOLO",
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, -20), new Vector2(0, 30));
        title.fontSize = 18;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Top;

        // Bottone chiudi
        var closeBtn = CreateButton(_formPanel.transform, "✕", new Vector2(80, 80),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-60, -60));
        closeBtn.onClick.AddListener(CloseForm);

        // ScrollView
        var scrollView = CreateScrollView(_formPanel.transform);
        var content = scrollView.transform.Find("Viewport/Content");

        // Header colonne
        CreateHeaderRow(content);

        // Righe difetti
        foreach (var defect in _reportData.Rows)
        {
            var rowGO = new GameObject("Row_" + defect.CodDifetto);
            rowGO.transform.SetParent(content, false);
            var rowUI = rowGO.AddComponent<DefectRowUI>();
            rowUI.BuildRow(defect);
        }

        // Bottone salva
        var saveBtn = CreateButton(_formPanel.transform, "💾 Salva Report", new Vector2(180, 45),
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30));
        saveBtn.onClick.AddListener(SaveReport);

        // Testo stato salvataggio
        var statusGO = new GameObject("SaveStatus");
        statusGO.transform.SetParent(_formPanel.transform, false);
        _saveStatusText = statusGO.AddComponent<TextMeshProUGUI>();
        _saveStatusText.fontSize = 14;
        _saveStatusText.color = Color.green;
        _saveStatusText.alignment = TextAlignmentOptions.Center;
        var statusRect = statusGO.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 0);
        statusRect.anchorMax = new Vector2(0.5f, 0);
        statusRect.sizeDelta = new Vector2(300, 30);
        statusRect.anchoredPosition = new Vector2(0, 75);

    }

    private void CreateHeaderRow(Transform parent)
    {
        var headers = new[] { "Codice", "Descrizione", "Visto", "E.02", "E.05", "E.1",
                               "I.02", "I.05", "I.1", "PS", "NA", "NR", "NP", "FOTO", "Note" };
        var widths = new[] { 90f, 180f, 45f, 45f, 45f, 45f, 45f, 45f, 45f, 45f, 45f, 45f, 45f, 45f, 120f };

        var rowGO = new GameObject("HeaderRow");
        rowGO.transform.SetParent(parent, false);

        var layout = rowGO.AddComponent<HorizontalLayoutGroup>();
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 4;
        layout.padding = new RectOffset(8, 8, 4, 4);

        var le = rowGO.AddComponent<LayoutElement>();
        le.minHeight = 35;
        le.preferredHeight = 35;

        var bg = rowGO.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.4f, 0.8f, 1f);

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = new GameObject(headers[i]);
            cell.transform.SetParent(rowGO.transform, false);
            var txt = cell.AddComponent<TextMeshProUGUI>();
            txt.text = headers[i];
            txt.fontSize = 11;
            txt.fontStyle = FontStyles.Bold;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            var cellLE = cell.AddComponent<LayoutElement>();
            cellLE.minWidth = widths[i];
            cellLE.preferredWidth = widths[i];
        }
    }

    private GameObject CreateScrollView(Transform parent)
    {


        var svGO = new GameObject("ScrollView");
        svGO.transform.SetParent(parent, false);

        var svRect = svGO.AddComponent<RectTransform>();
        svRect.anchorMin = new Vector2(0, 0.08f);
        svRect.anchorMax = new Vector2(1, 0.92f);
        svRect.offsetMin = new Vector2(10, 0);
        svRect.offsetMax = new Vector2(-10, -50);

        var scrollRect = svGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = true;

        var svImage = svGO.AddComponent<Image>();
        svImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

        // Mask/Viewport
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(svGO.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        viewport.AddComponent<Image>();
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;


        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 2;
        vlg.padding = new RectOffset(4, 4, 4, 4);

        scrollRect.viewport = vpRect;
        scrollRect.content = contentRect;

        return svGO;
    }

    private GameObject CreatePanel(Transform parent, Color color)
    {
        var go = new GameObject("FormPanel");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private Button CreateButton(Transform parent, string label, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position)
    {
        var go = new GameObject(label + "_Btn");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f);
        var btn = go.AddComponent<Button>();
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 14;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        return btn;
    }

    private TextMeshProUGUI CreateText(Transform parent, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        var txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = content;
        txt.color = Color.white;
        return txt;
    }

    private void OpenForm() => _formPanel.SetActive(true);
    private void CloseForm() => _formPanel.SetActive(false);

    private void SaveReport()
    {
        ReportSaver.Save(_reportData, _excelPath);
        _saveStatusText.text = "✓ Report salvato!";
        Invoke(nameof(ClearStatus), 3f);
    }

    private void ClearStatus() => _saveStatusText.text = "";
}