using UnityEngine;

/// <summary>
/// Base class for everything that scrolls toward the player and reacts on contact
/// (obstacles, coins, ...). Subclasses only implement OnInteract.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private float despawnX = -15f;
    [SerializeField] private float speedMultiplier = 1f;

    protected virtual void Update()
    {
        if (GameManager.Instance == null) return;

        transform.Translate(Vector3.left * GameManager.Instance.CurrentSpeed * speedMultiplier * Time.deltaTime, Space.World);

        if (transform.position.x < despawnX)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
            OnInteract(player);
    }

    protected abstract void OnInteract(PlayerController player);
}
