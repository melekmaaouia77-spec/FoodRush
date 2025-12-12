using System;
using UnityEngine;
using UnityEngine.UI;

public class PizzaSelectionUI : MonoBehaviour
{
    public static PizzaSelectionUI Instance;

    public event Action<ObjectsSO> OnPizzaSelected;
    [SerializeField] private Button[] pizzaButtons;
    [SerializeField] private ObjectsSO[] pizzaOptions; // Assign 1-to-1 in Inspector

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // Hide by default

        for (int i = 0; i < pizzaButtons.Length; i++)
        {
            int index = i;
            pizzaButtons[i].onClick.AddListener(() => SelectPizza(index));
        }
    }

    private void SelectPizza(int index)
    {
        OnPizzaSelected?.Invoke(pizzaOptions[index]);
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
