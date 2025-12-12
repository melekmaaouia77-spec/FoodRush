using System;
using UnityEngine;
using UnityEngine.UI;

public class SodaProgressBarUi : MonoBehaviour
{
    [SerializeField] private Image barImge;
    [SerializeField] private SodaContainer sodaContainer;

    private void Start()
    {
        sodaContainer.OnProgressChanged += SodaContainer_OnProgressChanged;
        barImge.fillAmount = 0f;
        Hide();
    }

    private void SodaContainer_OnProgressChanged(object sender, SodaContainer.OnprogressChangedEventArgs e)
    {
        barImge.fillAmount = e.progressNormalized;

        if (e.progressNormalized <= 0.01f || e.progressNormalized >= 0.99f)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
