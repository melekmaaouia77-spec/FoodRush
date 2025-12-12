using System;
using UnityEngine;
using UnityEngine.UI;

public class IceCreamSelectionUI : MonoBehaviour
{
    public static IceCreamSelectionUI Instance;

    public event Action<ObjectsSO> OnIceCreamSelected;
    [SerializeField] private Button[] iceCreamButtons;
    [SerializeField] private ObjectsSO[] iceCreamOptions; // assign 1-to-1 in Inspector

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        for (int i = 0; i < iceCreamButtons.Length; i++)
        {
            int index = i;
            iceCreamButtons[i].onClick.AddListener(() => SelectIceCream(index));
        }
    }

    private void SelectIceCream(int index)
    {
        OnIceCreamSelected?.Invoke(iceCreamOptions[index]);
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
