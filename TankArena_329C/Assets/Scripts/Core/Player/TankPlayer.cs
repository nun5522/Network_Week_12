using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;
using Unity.Collections;
using System;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private SpriteRenderer minimapIconRenderer;
    [SerializeField] private Texture2D crosshair;
    [field:SerializeField] public Health Health { get; private set; }
    [field:SerializeField] public CoinWallet Wallet { get; private set; }

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;
    [SerializeField] private Color ownerColorOnMap;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<int> PlayerColorIndex = new NetworkVariable<int>();
    public NetworkVariable<int> TeamIndex = new NetworkVariable<int>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = null;
            if (IsHost)
            {
                userData =
                HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            else
            {
                userData =
                    ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            

            PlayerName.Value = userData.userName;
            PlayerColorIndex.Value = userData.userColorIndex;
            TeamIndex.Value = userData.teamIndex;

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            virtualCamera.Priority = ownerPriority;

            minimapIconRenderer.color = ownerColorOnMap;

            Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}