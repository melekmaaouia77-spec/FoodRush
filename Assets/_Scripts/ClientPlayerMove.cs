using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField] private PlayerInput m_PlayerInput;

    private void Awake()
    {
        m_PlayerInput.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        { 
            m_PlayerInput.enabled = true;
        }
    }
}

