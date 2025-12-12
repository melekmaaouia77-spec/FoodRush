// 09/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkManagerCleanup : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
