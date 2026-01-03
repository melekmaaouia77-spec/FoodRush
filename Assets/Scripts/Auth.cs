using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System;
using System.Threading.Tasks;

public class Auth : MonoBehaviour
{

    private async Task Awake()
    {
        Application.runInBackground = true;
        await UnityServices.InitializeAsync();
    }
    public async void SignAnonnymouslyAsync()
    {
        try
        {
await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception ex)
        {
Debug.LogError(ex); 
        }
    }
    public async void SignInWithNameAndPasswordAsync(string username, string password)
    {

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username,password);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

    }
    public async void SignUpWithNameAndPasswordAsync(string username, string password)
    {

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

    }
    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();  

    }

}
