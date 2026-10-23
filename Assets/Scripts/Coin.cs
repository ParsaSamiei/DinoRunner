using UnityEngine;

/// <summary>Collectible bonus item that adds points.</summary>
public class Coin : Interactable
{
    [SerializeField] private int value = 50;

    protected override void OnInteract(PlayerController player)
    {
        GameManager.Instance.AddScore(value);
        Destroy(gameObject);
    }
}
