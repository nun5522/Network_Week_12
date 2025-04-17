using UnityEngine;
using Unity.Netcode;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private int damage = 5;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.attachedRigidbody == null) { return; }

        if(projectile.TeamIndex != -1)
        {
            if(col.attachedRigidbody != null)
            {
                if (col.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player))
                {
                    if (player.TeamIndex.Value == projectile.TeamIndex)
                    {
                        return;
                    }
                }
            }            
        }        

        if(col.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage);
        }
    }
}