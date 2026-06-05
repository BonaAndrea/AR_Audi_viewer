using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private ModelPlacer modelPlacer;
    [SerializeField] private Button toggleModeButton;
    [SerializeField] private TextMeshProUGUI toggleModeLabel;

    void Start()
    {
        toggleModeButton.onClick.AddListener(OnToggleModePressed);
        UpdateLabel();
    }

    private void OnToggleModePressed()
    {
        modelPlacer.ToggleMode();
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        toggleModeLabel.text = modelPlacer.GetModeLabel();
    }
}