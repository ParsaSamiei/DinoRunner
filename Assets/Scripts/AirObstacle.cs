using UnityEngine;

/// <summary>
/// Flying enemy (bird). Placed at a height where a standing player passes underneath
/// but a jumping player collides with it. Bobs up and down for some life.
/// </summary>
public class AirObstacle : Obstacle
{
    [SerializeField] private float bobAmplitude = 0.25f;
    [SerializeField] private float bobFrequency = 4f;

    private float baseY;
    private float phase;

    private void Start()
    {
        baseY = transform.position.y;
        phase = Random.value * Mathf.PI * 2f;
    }

    protected override void Update()
    {
        base.Update();

        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * bobFrequency + phase) * bobAmplitude;
        transform.position = p;
    }
}
