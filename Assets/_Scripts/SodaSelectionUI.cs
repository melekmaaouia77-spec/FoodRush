using System;
using UnityEngine;
using UnityEngine.UI;

public class SodaSelectionUI : MonoBehaviour
{
    public static SodaSelectionUI Instance;

    public event Action<ObjectsSO> OnSodaSelected;
    [SerializeField] private Button[] sodaButtons;
    [SerializeField] private ObjectsSO[] sodaOptions; // Coke, Sprite etc.

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        for (int i = 0; i < sodaButtons.Length; i++)
        {
            int index = i;
            sodaButtons[i].onClick.AddListener(() => SelectSoda(index));
        }
    }

    private void SelectSoda(int index)
    {
        OnSodaSelected?.Invoke(sodaOptions[index]);
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
