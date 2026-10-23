using UnityEngine;

/// <summary>Anything that hurts the player on contact (costs one life).</summary>
public abstract class Obstacle : Interactable
{
    protected override void OnInteract(PlayerController player)
    {
        player.TakeDamage();
    }
}
