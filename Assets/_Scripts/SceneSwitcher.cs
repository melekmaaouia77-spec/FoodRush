// 26/11/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Méthode pour changer de scène
    public void SwitchToBurgers()
    {
        SceneManager.LoadScene("Burgers");
    }
}