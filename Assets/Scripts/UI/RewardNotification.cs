
using System.Collections;
using TMPro;
using UnityEngine;

public class RewardNotification : MonoBehaviour
{
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private float _displayDuration = 2.5f;

    private Coroutine _hideCoroutine;

    public void Show(string message)
    {
        _rewardText.text = message;
        gameObject.SetActive(true);

        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);

        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(_displayDuration);

        gameObject.SetActive(false);
        _hideCoroutine = null;
    }
}
