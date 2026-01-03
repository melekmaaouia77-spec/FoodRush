using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class VivoxManager : MonoBehaviour
{
    private string channelName = "test-channel";

    private async void Awake()
    {
        await UnityServices.InitializeAsync();

        // Sign in anonymously before using Vivox
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Initialize Vivox
        await VivoxService.Instance.InitializeAsync();

        // Login to Vivox
        await VivoxService.Instance.LoginAsync();

        // Join a voice channel
        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);
    }

    private async void Update()
    {
        // Push-to-talk on P key
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Push-to-talk: TALKING");
            await VivoxService.Instance.SetChannelTransmissionModeAsync(
                TransmissionMode.Single, channelName);
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            Debug.Log("Push-to-talk: STOP TALKING");
            await VivoxService.Instance.SetChannelTransmissionModeAsync(
                TransmissionMode.None, null);
        }
    }
}
