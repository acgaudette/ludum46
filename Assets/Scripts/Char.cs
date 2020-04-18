using UnityEngine;
using System.Collections;

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

    // public float upright;
    // public float deadzone;

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

        if (pos < -Global.Values.width || pos > Global.Values.width)
        {
            vel *= -1;
            vel *= bounce;

            /*
            if (player)
                Camera.main.GetComponent<Cam>().shake += 0.2f;
            */
        }

        pos = Mathf.Max(-Global.Values.width, Mathf.Min(Global.Values.width, pos));

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

        float lim = Global.Values.flat * Mathf.PI * 0.5f;
        if (angle < -lim || angle > lim)
        {
            angv *= -1;
            angv *= slap;

            /*
            if (player && Mathf.Abs(angv) > 0.01f)
                Camera.main.GetComponent<Cam>().shake += 0.1f;
            */
        }

        angle = Mathf.Max(-lim, Mathf.Min(lim, angle));
        down = Mathf.Abs(angle) > Global.Values.thresh * Mathf.PI * .5f;
    }

    void Animate()
    {
        var dive = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        var flip = Quaternion.AngleAxis(player ? 1 : 180, Vector3.up);
        var aim = Quaternion.AngleAxis(look * Global.Values.spin * 90, Vector3.up);
        transform.rotation = flip * aim * dive;
        transform.position = new Vector3(pos, 0, depth);
    }

    void Hit()
    {
        var audio = GetComponents<AudioSource>();
        audio[1].Play();
        Global.invert += Global.Values.sleep;
    }

    void Shoot()
    {
        if (player)
            Camera.main.GetComponent<Cam>().shake += 0.1f;

        RaycastHit hit;
        var start = transform.Find("_cast").position;
        var fwd = transform.rotation * Vector3.forward;
        var ray = Physics.Raycast(start, fwd, out hit, 128);

        // Debug.DrawRay(start, fwd * 128, ray ? Color.green : Color.red, 0.5f, false);

        if (ray)
        {
            if (hit.transform.tag == "opponent")
            {
                hit.transform.GetComponent<Char>().Hit();
            }
        }

        var audio = GetComponent<AudioSource>();
        audio.Play();
    }

    void Tap(int dir)
    {
        Debug.Assert(dir != 0);
        angv += kick * -dir;

        if (!down)
        {
            vel += dash * dir; // Clamp?
        }

        if (player)
        {
            Camera.main.GetComponent<Cam>().heart += 0.1f;
        }
    }

    void Control()
    {
        if (!player || Global.invert > 0) return;

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

        var fire = Input.GetKeyDown(KeyCode.Space)
            || Input.GetMouseButtonDown(0);
        if (fire)
        {
            Shoot();
        }
    }

    void Update()
    {
        Animate();
        Control();
    }
}
