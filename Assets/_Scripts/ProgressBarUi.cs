using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUi : MonoBehaviour
{
    [SerializeField] private Image barImge;
    [SerializeField] private FoodContainer foodContainer;
    private void Start()
    {
        foodContainer.OnProgressChanged += FoodCountainer_OnProgressChanged;
        barImge.fillAmount = 0f;

        Hide();
        
    }

    private void FoodCountainer_OnProgressChanged(object sender, FoodContainer.OnprogressChangedEventArgs e)
    {
        barImge.fillAmount = e.progressNormalized;
        if (e.progressNormalized <= 0.01f || e.progressNormalized >= 0.99f)
        {
            Hide();
        }
        else {
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
