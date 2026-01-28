using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SyntheticHeartOverlay : MonoBehaviour
{
    private const string OverlayRootName = "SyntheticHeartOverlayRoot";

    private Canvas overlayCanvas;
    private GameObject buttonRoot;
    private GameObject panelRoot;
    private Text statusText;
    private Toggle enableToggle;
    private Toggle showToggle;
    private InputField baseUrlField;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<SyntheticHeartOverlay>() != null)
            return;

        var root = new GameObject(OverlayRootName);
        DontDestroyOnLoad(root);
        root.AddComponent<SyntheticHeartOverlay>();
    }

    private void Start()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        while (SaveLoadHandler.Instance == null)
            yield return null;

        BuildUi();
        ApplySettingsToUi();
    }

    private void BuildUi()
    {
        overlayCanvas = CreateCanvas();
        buttonRoot = CreateHeartButton(overlayCanvas.transform as RectTransform, out Button heartButton);
        panelRoot = CreatePanel(overlayCanvas.transform as RectTransform);

        heartButton.onClick.AddListener(TogglePanelVisibility);

        panelRoot.SetActive(false);
    }

    private void ApplySettingsToUi()
    {
        var data = SaveLoadHandler.Instance.data;
        enableToggle.isOn = data.enableSyntheticHeart;
        showToggle.isOn = data.showSyntheticHeartOverlay;
        baseUrlField.text = data.syntheticHeartBaseUrl;

        overlayCanvas.gameObject.SetActive(data.showSyntheticHeartOverlay);
    }

    private Canvas CreateCanvas()
    {
        var canvasObject = new GameObject("SyntheticHeartCanvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    private GameObject CreateHeartButton(RectTransform parent, out Button button)
    {
        var buttonObject = new GameObject("SyntheticHeartButton");
        var rect = buttonObject.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, -16f);
        rect.sizeDelta = new Vector2(140f, 36f);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.15f, 0.05f, 0.08f, 0.9f);

        button = buttonObject.AddComponent<Button>();

        var label = CreateText("❤ SyntH", rect, 16, TextAnchor.MiddleCenter);
        label.color = new Color(1f, 0.4f, 0.6f);

        return buttonObject;
    }

    private GameObject CreatePanel(RectTransform parent)
    {
        var panelObject = new GameObject("SyntheticHeartPanel");
        var rect = panelObject.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, -60f);
        rect.sizeDelta = new Vector2(360f, 240f);

        var image = panelObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);

        var layout = panelObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperLeft;

        var fitter = panelObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateHeader(rect);
        CreateEnableToggle(panelObject.transform as RectTransform);
        CreateShowToggle(panelObject.transform as RectTransform);
        CreateBaseUrlInput(panelObject.transform as RectTransform);
        CreateTestButton(panelObject.transform as RectTransform);
        CreateStatus(panelObject.transform as RectTransform);

        return panelObject;
    }

    private void CreateHeader(RectTransform parent)
    {
        var header = CreateText("Synthetic Heart", parent, 20, TextAnchor.MiddleLeft);
        header.color = new Color(1f, 0.55f, 0.7f);
    }

    private void CreateEnableToggle(RectTransform parent)
    {
        var toggleObject = CreateToggleRow(parent, "Enable integration", out enableToggle);
        enableToggle.onValueChanged.AddListener(OnEnableChanged);
    }

    private void CreateShowToggle(RectTransform parent)
    {
        var toggleObject = CreateToggleRow(parent, "Show heart button", out showToggle);
        showToggle.onValueChanged.AddListener(OnShowChanged);
    }

    private void CreateBaseUrlInput(RectTransform parent)
    {
        var label = CreateText("Base URL", parent, 14, TextAnchor.MiddleLeft);
        label.color = new Color(0.8f, 0.8f, 0.85f);

        var inputObject = new GameObject("SyntheticHeartBaseUrlInput");
        var rect = inputObject.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(0f, 32f);

        var image = inputObject.AddComponent<Image>();
        image.color = new Color(0.16f, 0.16f, 0.2f, 1f);

        baseUrlField = inputObject.AddComponent<InputField>();
        baseUrlField.textComponent = CreateText("", rect, 14, TextAnchor.MiddleLeft);
        baseUrlField.textComponent.color = Color.white;
        baseUrlField.placeholder = CreateText("http://localhost:11434", rect, 14, TextAnchor.MiddleLeft, 0.5f);
        baseUrlField.onEndEdit.AddListener(OnBaseUrlChanged);

        var textRect = baseUrlField.textComponent.rectTransform;
        textRect.offsetMin = new Vector2(10f, 6f);
        textRect.offsetMax = new Vector2(-10f, -6f);
        baseUrlField.placeholder.rectTransform.offsetMin = new Vector2(10f, 6f);
        baseUrlField.placeholder.rectTransform.offsetMax = new Vector2(-10f, -6f);
    }

    private void CreateTestButton(RectTransform parent)
    {
        var buttonObject = new GameObject("SyntheticHeartTestButton");
        var rect = buttonObject.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(0f, 30f);

        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.1f, 0.15f, 1f);

        var button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(OnTestConnection);

        var label = CreateText("Test connection", rect, 14, TextAnchor.MiddleCenter);
        label.color = new Color(1f, 0.6f, 0.75f);
    }

    private void CreateStatus(RectTransform parent)
    {
        statusText = CreateText("Status: idle", parent, 12, TextAnchor.MiddleLeft);
        statusText.color = new Color(0.7f, 0.7f, 0.75f);
    }

    private Text CreateText(string text, RectTransform parent, int size, TextAnchor anchor, float alpha = 1f)
    {
        var textObject = new GameObject("Text");
        var rect = textObject.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(0f, size + 8f);

        var label = textObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = size;
        label.alignment = anchor;
        label.text = text;
        label.color = new Color(1f, 1f, 1f, alpha);
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;

        return label;
    }

    private GameObject CreateToggleRow(RectTransform parent, string label, out Toggle toggle)
    {
        var rowObject = new GameObject("ToggleRow");
        var rect = rowObject.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(0f, 24f);

        var layout = rowObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleLeft;

        var toggleObject = new GameObject("Toggle");
        var toggleRect = toggleObject.AddComponent<RectTransform>();
        toggleRect.SetParent(rect, false);
        toggleRect.sizeDelta = new Vector2(20f, 20f);

        var toggleImage = toggleObject.AddComponent<Image>();
        toggleImage.color = new Color(0.9f, 0.4f, 0.6f, 1f);

        toggle = toggleObject.AddComponent<Toggle>();
        toggle.targetGraphic = toggleImage;

        var checkmarkObject = new GameObject("Checkmark");
        var checkRect = checkmarkObject.AddComponent<RectTransform>();
        checkRect.SetParent(toggleRect, false);
        checkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;

        var checkImage = checkmarkObject.AddComponent<Image>();
        checkImage.color = new Color(0.2f, 0.05f, 0.1f, 1f);
        toggle.graphic = checkImage;

        var labelText = CreateText(label, rect, 13, TextAnchor.MiddleLeft);
        labelText.color = new Color(0.85f, 0.85f, 0.9f);

        return rowObject;
    }

    private void TogglePanelVisibility()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(!panelRoot.activeSelf);
    }

    private void OnEnableChanged(bool value)
    {
        SaveLoadHandler.Instance.data.enableSyntheticHeart = value;
        SaveLoadHandler.Instance.SaveToDisk();
    }

    private void OnShowChanged(bool value)
    {
        SaveLoadHandler.Instance.data.showSyntheticHeartOverlay = value;
        SaveLoadHandler.Instance.SaveToDisk();

        overlayCanvas.gameObject.SetActive(value);
    }

    private void OnBaseUrlChanged(string value)
    {
        SaveLoadHandler.Instance.data.syntheticHeartBaseUrl = value?.Trim();
        SaveLoadHandler.Instance.SaveToDisk();
    }

    private void OnTestConnection()
    {
        if (!SaveLoadHandler.Instance.data.enableSyntheticHeart)
        {
            statusText.text = "Status: disabled";
            return;
        }

        var baseUrl = SaveLoadHandler.Instance.data.syntheticHeartBaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            statusText.text = "Status: missing URL";
            return;
        }

        StartCoroutine(TestConnectionCoroutine(baseUrl));
    }

    private IEnumerator TestConnectionCoroutine(string baseUrl)
    {
        statusText.text = "Status: testing...";
        string url = baseUrl.TrimEnd('/') + "/api/prompt_override";
        using (var request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "Status: failed";
                Debug.LogWarning("[SyntheticHeartOverlay] Connection failed: " + request.error);
                yield break;
            }

            statusText.text = "Status: ok";
        }
    }
}
