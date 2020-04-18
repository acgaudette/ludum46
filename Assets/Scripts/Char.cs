using UnityEngine;

public class Char : MonoBehaviour
{
    public bool player;

    public float kick;
    public float dash;

    [Range(0, 1)] public float bounce;
    [Range(0, 1)] public float friction;
    [Range(0, 1)] public float slap;
    public float damp;
    public float fall;
    [Range(0, 1)] public float thresh;
    // public float upright;
    // public float deadzone;
    [Range(0, 1)] public float flat;
    [Range(0, 1)] public float spin;

    public float k_width;

    [HideInInspector] public bool down;
    float angv;
    float angle;
    float vel;
    float pos;
    float depth;
    float look;

    void Start()
    {
        depth = transform.position.z;
    }

    void FixedUpdate()
    {
        vel *= friction;
        pos += vel * Time.deltaTime;

        if (pos < -k_width || pos > k_width)
        {
            vel *= -1;
            vel *= bounce;
        }

        pos = Mathf.Max(-k_width, Mathf.Min(k_width, pos));

        int dir = angle > 0 ? 1 : -1;

        if (!down)
        {
            /*
            float t = Mathf.Clamp01(Mathf.Abs(angle) / deadzone);
            angv += -dir * upright * t;
            float drag = damp + (1 - damp) * (1 - t) * (1 - t);
            angv *= drag;
            */

            // angv += -dir * fall;
            angle = Mathf.Lerp(angle, 0, Time.deltaTime * damp);
            angv *= 0.9f;
        }
        else
        {
            angv += dir * fall;
        }

        angle += angv;

        float lim = flat * Mathf.PI * 0.5f;
        if (angle < -lim || angle > lim)
        {
            angv *= -1;
            angv *= slap;
        }

        angle = Mathf.Max(-lim, Mathf.Min(lim, angle));
        down = Mathf.Abs(angle) > thresh * Mathf.PI * .5f;
    }

    void Animate()
    {
        var dive = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        var flip = Quaternion.AngleAxis(player ? 1 : 180, Vector3.up);
        var aim = Quaternion.AngleAxis(look * spin * 90, Vector3.up);
        transform.rotation = flip * aim * dive;
        transform.position = new Vector3(pos, 0, depth);
    }

    void Tap(int dir)
    {
        Debug.Assert(dir != 0);
        angv += kick * Time.deltaTime * -dir;

        // TODO: fix force bounce

        if (!down)
        {
            vel += dash * dir; // Clamp?
        }
    }

    void Control()
    {
        if (!player) return;

        var right = (
               Input.GetKeyDown(KeyCode.D)
            || Input.GetKeyDown(KeyCode.L)
        ) ? 1 : 0;

        var left = (
               Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.H)
        ) ? -1 : 0;

        int dir = left + right;
        if (dir != 0)
        {
            Tap(dir);
        }

        look = Input.mousePosition.x / Screen.width;
        look = 2 * look - 1;
    }

    void Update()
    {
        Animate();
        Control();
    }
}
