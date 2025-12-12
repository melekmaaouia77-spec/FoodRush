using System;
using UnityEngine;
using UnityEngine.UI;

public class IceCreamProgressBarUi : MonoBehaviour
{
    [SerializeField] private Image barImge;
    [SerializeField] private IceCreamContainer iceCreamContainer;

    private void Start()
    {
        iceCreamContainer.OnProgressChanged += IceCreamContainer_OnProgressChanged;
        barImge.fillAmount = 0f;
        Hide();
    }

    private void IceCreamContainer_OnProgressChanged(object sender, IceCreamContainer.OnprogressChangedEventArgs e)
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
