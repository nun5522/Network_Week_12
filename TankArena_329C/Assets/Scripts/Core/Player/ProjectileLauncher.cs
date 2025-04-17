using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")] 
    [SerializeField] private TankPlayer player;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CoinWallet wallet;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private int costToFire;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;

    private bool isPointerOverUI;
    private bool shouldFire;
    //private float previousFireTime;
    private float muzzleFlashTimer;
    private float timer;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void Update()
    {
        if (muzzleFlashTimer > 0f)
        { 
            muzzleFlashTimer -= Time.deltaTime;

            if (muzzleFlashTimer <= 0f) 
            { 
                muzzleFlash.SetActive(false);
            }
        }

        if (!IsOwner) { return; }
        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (wallet.TotalCoins.Value < costToFire)
        {
            return;
        }



        if (!shouldFire) { return; }

        if(timer > 0) { return; }

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up,player.TeamIndex.Value);

        timer = 1 / fireRate;
    }
        
    private void HandlePrimaryFire(bool shouldFire)
    {
        if (shouldFire)
        {
            if (isPointerOverUI) { return; }
        }
        this.shouldFire = shouldFire;
    }

    [Rpc(SendTo.Server)]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (wallet.TotalCoins.Value < costToFire) { return; }

        wallet.SpendCoins(costToFire);

        GameObject projectileInstance = Instantiate(
            serverProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        projectileInstance.transform.up = direction;
        Physics2D.IgnoreCollision(playerCollider,projectileInstance.GetComponent<Collider2D>());

        if (projectileInstance.TryGetComponent<Projectile>(out Projectile projectile))
        {
            projectile.Initialise(player.TeamIndex.Value);
        }

        if(projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }

        SpawnDummyProjectileClientRpc(spawnPos, direction,player.TeamIndex.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction,int teamIndex)
    {
        if (IsOwner) { return; }

        SpawnDummyProjectile(spawnPos, direction,teamIndex);
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction,int teamIndex)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        GameObject projectileInstance = Instantiate(
            clientProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        projectileInstance.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        if (projectileInstance.TryGetComponent<Projectile>(out Projectile projectile))
        {
            projectile.Initialise(teamIndex);
        }
            
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }
    }
}