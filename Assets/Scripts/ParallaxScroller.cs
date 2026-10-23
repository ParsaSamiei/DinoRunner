using UnityEngine;

/// <summary>
/// Endless horizontal scrolling layer. Put on a GameObject with a SpriteRenderer whose sprite
/// is WIDER than the camera view. Far layers: small factor; ground: factor 1.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxScroller : MonoBehaviour
{
    [SerializeField] private float speedFactor = 0.5f;

    private Transform first;
    private Transform second;
    private float width;
    private float startX;

    private void Start()
    {
        width = GetComponent<SpriteRenderer>().bounds.size.x;
        startX = transform.position.x;

        first = transform;
        GameObject clone = Instantiate(gameObject, transform.parent);
        Destroy(clone.GetComponent<ParallaxScroller>());   // clone is moved by this script
        clone.transform.position = transform.position + Vector3.right * width;
        second = clone.transform;
    }

    private void Update()
    {
        if (GameManager.Instance == null || second == null) return;

        float dx = GameManager.Instance.CurrentSpeed * speedFactor * Time.deltaTime;
        Move(first, dx);
        Move(second, dx);
    }

    private void Move(Transform t, float dx)
    {
        Vector3 p = t.position;
        p.x -= dx;
        if (p.x <= startX - width) p.x += width * 2f;
        t.position = p;
    }
}
