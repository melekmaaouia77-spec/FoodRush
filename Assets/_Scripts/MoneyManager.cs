using UnityEngine;
using TMPro;
using Unity.Netcode;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance; // singleton for easy access

    [SerializeField] private TMP_Text moneyText; // assign in inspector
    private int money = 0;

    private void Awake()
    {
        // Each client has their own MoneyManager instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        UpdateUI();
    }

    private void Start()
    {
        // Double-check UI is visible on start
        UpdateUI();
        Debug.Log($"MoneyManager started on Client {(NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId.ToString() : "NoNetwork")}");
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
        Debug.Log($"[LOCAL CLIENT] Money added: {amount}. Total: {money}");
    }

    public int GetMoney()
    {
        return money;
    }

    public void ResetMoney()
    {
        money = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"$ {money}";
            Debug.Log($"UI Updated: ${money}");
        }
        else
        {
            Debug.LogWarning("MoneyText UI reference is missing!");
        }
    }
}