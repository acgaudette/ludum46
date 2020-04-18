using UnityEngine;
using System.Collections;

public class Char : MonoBehaviour
{
    public bool Player
    {
        get
        {
            return GetComponent<Player>() != null;
        }
    }

    public float kick;
    public float dash;

    [Range(0, 1)] public float bounce;
    [Range(0, 1)] public float friction;
    [Range(0, 1)] public float slap;
    public float damp;
    public float fall;

    public float rof;

    // public float upright;
    // public float deadzone;

    [HideInInspector] public bool down;
    [HideInInspector] public int hp;
    float angv;
    float angle;
    float vel;
    float pos;
    float depth;
    [HideInInspector] public float look;
    [HideInInspector] public Quaternion manual;
    float timer;

    void Start()
    {
        depth = transform.position.z;
        manual = Quaternion.identity;
        hp = 3;
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
        // var flip = Quaternion.AngleAxis(Player ? 0 : 180, Vector3.up);
        var aim = manual == Quaternion.identity ?
            Quaternion.AngleAxis(look * Global.Values.spin * 90, Vector3.up) :
            manual;

        transform.rotation = /* flip * */ aim * dive;
        transform.position = new Vector3(pos, 0, depth);
    }

    public void Hit(int side)
    {
        --hp;

        if (0 == hp)
        {
            vel += side * 10;
            angv += side * 10;
        }

        var audio = GetComponents<AudioSource>();
        audio[1].Play();

        if (!Player)
            Global.invert += Global.Values.sleep;
    }

    public RaycastHit? Cast(Vector3 dir)
    {
        RaycastHit hit;
        var start = transform.Find("_cast").position;
        var ray = Physics.Raycast(start, dir, out hit, 128);

        // Debug.DrawRay(start, fwd * 128, ray ? Color.green : Color.red, 0.5f, false);

        RaycastHit? result = hit;
        return ray ? result : null;
    }

    public void Shoot()
    {
        if (timer > 0) return;

        var result = Cast(transform.rotation * Vector3.forward);
        if (result.HasValue)
        {
            var trans = result.Value.transform;
            if (trans.tag == "opponent" || trans.tag == "Player")
            {
                var diff = transform.position.x - trans.position.x;
                trans.GetComponent<Char>().Hit(diff > 1 ? 1 : -1);
            }
        }

        var audio = GetComponent<AudioSource>();
        audio.Play();

        timer = rof;
    }

    public void Tap(int dir)
    {
        Debug.Assert(dir != 0);
        angv += kick * -dir;

        if (!down)
        {
            vel += dash * dir; // Clamp?
        }
    }

    void Update()
    {
        Animate();
        timer = Mathf.Max(0, timer - Time.deltaTime);
    }
}
