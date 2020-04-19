using UnityEngine;

public class Cam : MonoBehaviour
{
    public float shake;
    [Range(0, 1)] public float heart;
    public float decay;
    public float baserate;
    public float amp;

    [HideInInspector] public float h;

    Vector3 origin;
    float timer;

    void Start()
    {
        origin = transform.localPosition;
    }

    void Update()
    {
        // heart = Mathf.Max(0, Mathf.Min(1, heart - Time.deltaTime * decay));
        heart = 0;
        float rate = baserate + heart * baserate * 3;
        h = Mathf.Sin(Time.time * rate);
        float y = origin.y + h * amp;
        transform.localPosition = new Vector3(0, y, 0);

        shake -= Time.deltaTime;
        shake = Mathf.Max(0, shake);

        var offset = Vector2.zero;
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = 0.01f;
            offset = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1))
                * shake;
        }

        transform.localPosition = new Vector3(
            offset.x, offset.y + y, 0
        );
    }
}
