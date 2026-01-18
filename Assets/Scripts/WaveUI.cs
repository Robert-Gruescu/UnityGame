using UnityEngine;
using UnityEngine.UI;

// Simple in-game UI for waves and kill counts.
public class WaveUI : MonoBehaviour
{
    public Text infoText; // shows "Wave X/Y"
    public Text killsText; // shows "X/Y killed"
    
    private int totalEnemiesThisWave = 0;

    void Awake()
    {
        if (infoText == null || killsText == null)
        {
            // create a simple canvas + two texts
            GameObject canvasGO = new GameObject("WaveUI_Canvas");
            Canvas c = canvasGO.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // info text (top-right) - shows "Wave X din Y"
            GameObject infoGO = new GameObject("InfoText");
            infoGO.transform.SetParent(canvasGO.transform);
            infoText = infoGO.AddComponent<Text>();
            infoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            infoText.fontSize = 28;
            infoText.alignment = TextAnchor.UpperRight;
            infoText.rectTransform.anchorMin = new Vector2(1f, 1f);
            infoText.rectTransform.anchorMax = new Vector2(1f, 1f);
            infoText.rectTransform.pivot = new Vector2(1f, 1f);
            infoText.rectTransform.anchoredPosition = new Vector2(-10, -10);
            infoText.text = "";

            // kills text (top-right, below info text) - shows kill count
            GameObject killsGO = new GameObject("KillsText");
            killsGO.transform.SetParent(canvasGO.transform);
            killsText = killsGO.AddComponent<Text>();
            killsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            killsText.fontSize = 24;
            killsText.alignment = TextAnchor.UpperRight;
            killsText.rectTransform.anchorMin = new Vector2(1f, 1f);
            killsText.rectTransform.anchorMax = new Vector2(1f, 1f);
            killsText.rectTransform.pivot = new Vector2(1f, 1f);
            killsText.rectTransform.anchoredPosition = new Vector2(-10, -45);
            killsText.text = "";
        }
    }

    // Called when a wave starts - set total enemies for THIS wave
    public void ShowWaveStart(int waveIndex, int totalWaves, int totalEnemiesThisWave)
    {
        if (infoText != null)
            infoText.text = $"Wave {waveIndex}/{totalWaves}";
        this.totalEnemiesThisWave = totalEnemiesThisWave;
        UpdateKillCount(0);
    }

    // Called often to update the kills counter - ONLY pass killed count
    public void UpdateKillCount(int killed)
    {
        if (killsText != null)
            killsText.text = $"{killed}/{totalEnemiesThisWave} killed";
    }

    // Called when a wave ends
    public void ShowWaveEnd(int waveIndex, string waveName = null)
    {
        // no action on wave end; next wave will update the text
    }
}
