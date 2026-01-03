using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Friends;
using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Linq;

public class MainMenu : Panel
{

    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] private Button logoutButton = null;
    [SerializeField] private Button hostButton = null;
    [SerializeField] private Button clientButton = null;
    [SerializeField] private Button leaderBoardButton = null;
    [SerializeField] private Button friendsButton = null;
    [SerializeField] private Button renameButton = null;
    [SerializeField] private Button customizationButton = null;
    [SerializeField] private Button lobbyButton = null;


    private bool isFriendsServiceInitialized = false;
    private List<string> joinedLobbyIds = new List<string>();

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }
        logoutButton.onClick.AddListener(SignOut);
        hostButton.onClick.AddListener(OnPlayHost);
        clientButton.onClick.AddListener(OnPlayerClient);
        leaderBoardButton.onClick.AddListener(LeaderBoards);
        friendsButton.onClick.AddListener(Friends);
        renameButton.onClick.AddListener(RenamePlayer);
        customizationButton.onClick.AddListener(Customization);
        lobbyButton.onClick.AddListener(Lobby);
        base.Initialize();
    }


    public override void Open()
    {
        friendsButton.interactable = isFriendsServiceInitialized;
        if (isFriendsServiceInitialized == false) {
            InitializeFriendsServiceAsync();
        }
        UpdatePlayerNameUI();
        base.Open();
    }
    private async void Lobby()
    {
        // Debug.Log("testttt");
        PanelManager.Open("loading");
        //PanelManager.Open("lobby_search");
        try
        {

            var lobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync();
            joinedLobbyIds = lobbyIds;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);

        }
        Lobby lobby = null;
        if (joinedLobbyIds.Count > 0)
        {
            try
            {
                lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobbyIds.Last());
            }
            catch (Exception e)
            {

                Debug.Log(e.Message);
            }
        }
        if (lobby == null)
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            if (panel.JoinedLobby != null && joinedLobbyIds.Count > 0 && panel.JoinedLobby.Id == joinedLobbyIds.Last())
            {


                lobby = panel.JoinedLobby;
            }
        }
        if (lobby != null)
        {
            Debug.Log("testttt");
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            panel.Open(lobby);
        }
        else
        {
            // Debug.Log("testttt");
            PanelManager.Open("lobby_search");
        }
        PanelManager.Close("loading");
    } 
       
    


    private async void InitializeFriendsServiceAsync()
    {
        try
        {
            Debug.Log("initializing friends");
await FriendsService.Instance.InitializeAsync();
            isFriendsServiceInitialized= true;
            friendsButton.interactable = true;  

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);  
        }

    }

    private void SignOut()
    {
        ActionConfirmMenu panel = (ActionConfirmMenu)PanelManager.GetSingleton("action_confirm");
        panel.Open(SignOutResult, "are you sure you want to sign out ?","Yes","No");
      
    }
    private void SignOutResult(ActionConfirmMenu.Result result) {
        if (result == ActionConfirmMenu.Result.Positive)
        {
            MenuManager.Singleton.SignOut();
            MenuManager.Singleton.LogoutOfVivoxAsync();
            isFriendsServiceInitialized = false;
        }
    }

    private void UpdatePlayerNameUI()
    {
        nameText.text = AuthenticationService.Instance.PlayerName;
    }
    private void OnPlayHost()
    {
       
        PanelManager.CloseAll();
        MenuManager.Singleton.CloseCanvas();    

      
        NetworkManager.Singleton.StartHost();
    }
    private void OnPlayerClient()
    {

        PanelManager.CloseAll();
        MenuManager.Singleton.CloseCanvas();

        
        NetworkManager.Singleton.StartClient();
    }
    private void LeaderBoards()
    {
        PanelManager.Open("leaderboards");
    }
    private void Friends()
    {
        PanelManager.Open("friends");
    }
    private void Customization()
    {
        PanelManager.Open("customization");
    }
    private void RenamePlayer()
    {
        GetInputMenu panel = (GetInputMenu)PanelManager.GetSingleton("input");
        panel.Open(RenamePlayerConfirm, GetInputMenu.Type.String, 20, "Enter a name for your account", "Send","Cancel");
    }
    private async void RenamePlayerConfirm(string input)
    {
        renameButton.interactable = false ;
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(input);  
            UpdatePlayerNameUI();
        }
        catch (Exception e) { 
        ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to update players name ","ok");
        
        }
        renameButton.interactable = true ;  
    }
    

    

}