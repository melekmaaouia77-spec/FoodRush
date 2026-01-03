/*using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private GameObject gameplayUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Make sure lobby UI is visible at start
        if (lobbyUI != null)
        {
            lobbyUI.SetActive(true);
        }

        // Make sure gameplay UI is hidden at start
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        // Subscribe to network events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public void StartGameplay()
    {
        // Disable lobby UI
        if (lobbyUI != null)
        {
            lobbyUI.SetActive(false);
        }

        // Enable gameplay UI
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(true);
        }

        Debug.Log($"Game started! Host: {NetworkManager.Singleton.IsHost}, Client: {NetworkManager.Singleton.IsClient}");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");

        // When fully connected, start gameplay
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            StartGameplay();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");

        // If disconnected, return to lobby
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            ReturnToLobby();
        }
    }

    public void ReturnToLobby()
    {
        // Enable lobby UI
        if (lobbyUI != null)
        {
            lobbyUI.SetActive(true);
        }

        // Disable gameplay UI
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        // Shutdown network
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}*/