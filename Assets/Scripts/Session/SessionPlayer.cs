using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SessionPlayer : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private MeshRenderer meshRenderer = null;

    private CharacterController controller = null;
    private string _colorHex = "";
    private string _id = "";

    // Set by SessionManager when instantiated
    [HideInInspector] public int characterIndex = 0;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Auto-assign meshRenderer if not set
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        controller.Move(new Vector3(moveInput.x, 0, moveInput.y) * Time.deltaTime * moveSpeed);
    }

    [Rpc(SendTo.Everyone)]
    public void ApplyDataRpc(string id, string colorHex)
    {
        _id = id;
        _colorHex = colorHex;

        // Only apply color if meshRenderer exists and NOT the special character
        bool isSpecialCharacter = characterIndex == SessionManager.Singleton.CharactersPrefabCount - 1;

        if (meshRenderer != null && !isSpecialCharacter && !string.IsNullOrEmpty(colorHex))
        {
            if (ColorUtility.TryParseHtmlString(colorHex, out Color color))
            {
                meshRenderer.material.color = color;
            }
        }
    }

    // Reapply stored data
    public void ApplyDataRpc()
    {
        ApplyDataRpc(_id, _colorHex);
    }
}
