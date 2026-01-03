using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

// Alias to avoid conflict with your gameplay Players class
using LobbyPlayer = Unity.Services.Lobbies.Models.Player;

public class LobbyPlayerItem : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] public TextMeshProUGUI roleText = null;
    [SerializeField] public TextMeshProUGUI statusText = null;
    [SerializeField] private Button kickButton = null;

    private string lobbyId = "";
    private LobbyPlayer player = null;

    private void Start()
    {
        kickButton.onClick.AddListener(Kick);
    }

    public void Initialize(LobbyPlayer player, string lobbyId, string hostId)
    {
        this.player = player;
        this.lobbyId = lobbyId;

        // Display player name
        nameText.text = player.Data.ContainsKey("name") ? player.Data["name"].Value : "Unknown";

        // Role: Host or Member
        string playerId = player.Data.ContainsKey("id") ? player.Data["id"].Value : "";
        roleText.text = playerId == hostId ? "Host" : "Member";

        // Ready status
        bool isReady = player.Data.ContainsKey("ready") && player.Data["ready"].Value == "1";
        statusText.text = isReady ? "Ready" : "Not Ready";

        // Kick button only visible if you are host and target is not host
        kickButton.gameObject.SetActive(playerId != hostId && AuthenticationService.Instance.PlayerId == hostId);
    }

    private async void Kick()
    {
        kickButton.interactable = false;
        try
        {
            string playerId = player.Data.ContainsKey("id") ? player.Data["id"].Value : "";
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
            Destroy(gameObject);
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
        kickButton.interactable = true;
    }
}
