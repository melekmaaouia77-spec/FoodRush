using UnityEngine;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class MenuManager : MonoBehaviour
{
    private bool initialized = false;
    private bool eventInitialized = false;
    private static MenuManager singleton = null;
    private string lastUserName = "";
    private bool isInChannel = false;
    private bool isTalking = false;

    [SerializeField] private Canvas canvs;

    public static MenuManager Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindAnyObjectByType<MenuManager>();
                singleton.Initialize();
            }
            return singleton;
        }
    }

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;
    }

    private void Awake()
    {
        Application.runInBackground = true;
        StartClientService();
    }

    private void OnDestroy()
    {
        if (singleton == this)
            singleton = null;
    }

    public void CloseCanvas()
    {
        canvs.gameObject.SetActive(false);
    }

    public async void StartClientService()
    {
        Debug.Log("[UI] Starting client services...");

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                var options = new InitializationOptions();
                options.SetProfile("default_profile");
                await UnityServices.InitializeAsync();
                await VivoxService.Instance.InitializeAsync();
            }

            if (!eventInitialized)
                SetupEvents();

            if (AuthenticationService.Instance.SessionTokenExists)
                await SignInAnonymouslyAsync();
            else
                Debug.Log("[UI] Please sign in");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ERROR] Failed to connect to network: {ex.Message}");
            ShowError("OpenAuthMenu", "Failed to connect to the network", "retry");
        }
    }

    private void SetupEvents()
    {
        eventInitialized = true;
        AuthenticationService.Instance.SignedIn += SignInConfirmAsync;
        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("[UI] Signed out");
        };
        AuthenticationService.Instance.Expired += async () => await SignInAnonymouslyAsync();
    }

    public async Task SignInAnonymouslyAsync()
    {
        Debug.Log("[AUTH] Signing in anonymously...");
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await OnSignedIn();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[ERROR] Failed to sign in: {ex.Message}");
            ShowError("OpenAuthMenu", "Failed to sign in", "ok");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[ERROR] Network connection failed: {ex.Message}");
            ShowError("OpenAuthMenu", "Failed to connect to the network", "ok");
        }
    }

    public async void SignInWithUserNameAndPasswordAsync(string userName, string password)
    {
        lastUserName = userName.Trim();
        Debug.Log($"[AUTH] Signing in as: {userName}");
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(userName, password);
            await OnSignedIn();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[ERROR] Wrong username/password: {ex.Message}");
            ShowError("OpenAuthMenu", "Username or password is wrong", "ok");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[ERROR] Network connection failed: {ex.Message}");
            ShowError("OpenAuthMenu", "Failed to connect to the network", "ok");
        }
    }

    public async void SignUpWithUserNameAndPasswordAsync(string userName, string password)
    {
        Debug.Log($"[AUTH] Signing up as: {userName}");
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(userName, password);
            await OnSignedIn();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[ERROR] Failed to sign up: {ex.Message}");
            ShowError("OpenAuthMenu", "Failed to sign you up", "ok");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[ERROR] Network connection failed: {ex.Message}");
            ShowError("OpenAuthMenu", "Failed to connect to the network", "ok");
        }
    }

    private async Task OnSignedIn()
    {
        Debug.Log("✅ Unity Authentication complete, now logging into Vivox...");

        // Login to Vivox
        await VivoxService.Instance.LoginAsync();

        // Join a default channel
        string channelName = "Lobby";
        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);

        isInChannel = true;
        Debug.Log("🎤 Joined Vivox channel: " + channelName);

        await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.None, "Lobby");
        Debug.Log("🔇 Voice chat ready - Press P to talk");
    }

    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();
        isInChannel = false;
        isTalking = false;
        Debug.Log("[AUTH] Signed out");
    }

    public async void LogoutOfVivoxAsync()
    {
        await VivoxService.Instance.LogoutAsync();
        isInChannel = false;
        isTalking = false;
    }

    private void ShowError(string action = "None", string error = "", string button = "")
    {
        Debug.Log($"[ERROR] {error}");

        if (button == "retry")
        {
            Debug.Log("[ACTION] Retrying connection...");
            StartClientService();
        }
    }

    private async void SignInConfirmAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync("Player");
            }
            Debug.Log("[AUTH] Sign-in confirmed");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ERROR] in SignInConfirm: {ex.Message}");
        }
    }

    private async void Update()
    {
        if (!isInChannel) return;

        if (Input.GetKeyDown(KeyCode.P) && !isTalking)
        {
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.All, "Lobby");
            isTalking = true;
            Debug.Log("🎤 TALKING...");
        }

        if (Input.GetKeyUp(KeyCode.P) && isTalking)
        {
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.None, "Lobby");
            isTalking = false;
            Debug.Log("🔇 MUTED");
        }
    }
    public void OnClickSignInAnonymously()
    {
        // Fire and forget
        _ = SignInAnonymouslyAsync();
    }

}