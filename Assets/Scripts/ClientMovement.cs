using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Collections;

public class ClientMovement : NetworkBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private StarterAssetsInputs starterAssetsInputs;
    [SerializeField] private ThirdPersonController thirdPersonController;
    [SerializeField] private TextMeshProUGUI playerNameTxt;

    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        writePerm: NetworkVariableWritePermission.Server);

    private void Awake()
    {
        playerInput.enabled = false;
        starterAssetsInputs.enabled = false;
        thirdPersonController.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Update name UI whenever the variable changes
        playerName.OnValueChanged += OnPlayerNameChanged;
        OnPlayerNameChanged("", playerName.Value);

        if (IsOwner)
        {
            playerInput.enabled = true;
            starterAssetsInputs.enabled = true;
            SubmitNameServerRpc(AuthenticationService.Instance.PlayerName);
        }

        if (IsServer)
        {
            thirdPersonController.enabled = true;
        }
    }

    private void OnPlayerNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        playerNameTxt.text = newValue.ToString();
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string name)
    {
        playerName.Value = name;
    }

    [Rpc(SendTo.Server)]
    private void UpdateInputServerRpc(Vector2 move, Vector2 look, bool jump, bool sprint)
    {
        starterAssetsInputs.MoveInput(move);
        starterAssetsInputs.LookInput(look);
        starterAssetsInputs.SprintInput(sprint);
        starterAssetsInputs.JumpInput(jump);
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        UpdateInputServerRpc(starterAssetsInputs.move, starterAssetsInputs.look,
                             starterAssetsInputs.sprint, starterAssetsInputs.jump);
    }
}
