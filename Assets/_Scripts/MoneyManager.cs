using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance; // singleton for easy access

    [SerializeField] private TMP_Text moneyText; // assign in inspector
    private int money = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = $"$ {money}";
    }
}
