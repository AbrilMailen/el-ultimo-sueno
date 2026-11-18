
using TMPro;
using UnityEngine;

public class SeedCounterUI : MonoBehaviour
{
    [SerializeField] private PlayerCombat _playerCombat;
    [SerializeField] private TMP_Text _seedsText;

    private void OnEnable()
    {
        if (_playerCombat == null || _seedsText == null)
            return;

        _playerCombat.OnSeedsChanged += UpdateSeedsText;

        // Mostrar los valores actuales al iniciar
        UpdateSeedsText(
            _playerCombat.CurrentSeeds,
            _playerCombat.MaxSeeds
        );
    }

    private void OnDisable()
    {
        if (_playerCombat != null)
            _playerCombat.OnSeedsChanged -= UpdateSeedsText;
    }

    private void UpdateSeedsText(int current, int max)
    {
        _seedsText.text = $"Semillas: {current} / {max}";
    }
}
