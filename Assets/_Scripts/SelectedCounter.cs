using UnityEngine;

public class SelectedCounter : MonoBehaviour
{

    [SerializeField] ClearCounter clearCounter;
    [SerializeField] GameObject VisualCounter;
    private void Start()
    {


        players.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
    }
    private void Player_OnSelectedCounterChanged(object sender, players.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter == clearCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
    private void Show()
    {
        VisualCounter.SetActive(true);
    }
    private void Hide()
    {
        VisualCounter.SetActive(false);
    }
}
 



