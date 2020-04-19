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

    public float knock;

    public float rof;
    public float roc;
    public bool invincible;

    // public float upright;
    // public float deadzone;

    public TextMesh shoutPrefab;

    [HideInInspector] public float fx_hp;
    [HideInInspector] public float lastHit;
    [HideInInspector] public float lastShot;
    [HideInInspector] public bool cocked;
    [HideInInspector] public bool down;
    [HideInInspector] public int downside;
    [HideInInspector] public int hp;
    [HideInInspector] public float look;
    [HideInInspector] public Quaternion manual;

    [HideInInspector] public float angv;
    [HideInInspector] public float angle;
    [HideInInspector] public float vel;
    float pos;
    float depth;
    float shootTimer;
    float cockTimer;
    AudioSource[] snd;
    Transform cast;

    enum Sound
    {
          Fire
        , Hit
        , Fall
        , Cock
        , Move
    };

    void Start()
    {
        depth = transform.position.z;
        manual = Quaternion.identity;

        var audio = transform.Find("_snd");
        Debug.Assert(audio != null);
        snd = audio.GetComponents<AudioSource>();

        cast = transform.Find("_cam").Find("_cast");
        Debug.Assert(cast != null);

        hp = 3;
        lastHit = -16;
    }

    void FixedUpdate()
    {
        vel *= friction;
        pos += vel * Time.deltaTime;

        if (pos < -Global.Values.width || pos > Global.Values.width)
        {
            vel *= -1;
            vel *= bounce;

            if (Mathf.Abs(vel) > 0.05f)
            {
                Sfx(Sound.Fall);
            }

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

            if (Mathf.Abs(angv) > 0.01f)
            {
                Sfx(Sound.Fall);
                // Camera.main.GetComponent<Cam>().shake += 0.1f;
            }
        }

        angle = Mathf.Max(-lim, Mathf.Min(lim, angle));
        down = Mathf.Abs(angle) > Global.Values.thresh * Mathf.PI * .5f;
        downside = angle > 0 ? -1 : 1;
    }

    public void TextPop(string[] msg, Color col)
    {
        string caps = msg[Random.Range(0, msg.Length)];
        var pos = transform.position + Vector3.up * 2 + Vector3.forward * 0.5f;
        pos += new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
        var orient = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        var pop = GameObject.Instantiate(shoutPrefab, pos, orient)
            .GetComponent<TextMesh>();
        pop.text = caps;
        pop.color = col;
        GameObject.Destroy(pop.gameObject, 0.5f);
    }

    void Sfx(Sound sound)
    {
        snd[(int)sound].Play();
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
        if (!invincible) --hp;
        if (hp < 0) hp = 0;

        if (0 == hp)
        {
            vel += side * Global.Values.deathKick;
            angv += side * Global.Values.deathKick * -0.75f;
            Global.Iter(Player);
        }
        else
        {
            vel += side * knock;
            angv += side * knock * -0.01f;
        }

        Sfx(Sound.Hit);

        if (!Player)
        {
            Global.invert += Global.Values.sleep;
            TextPop(
                new[]
                {
                      "OUCH"
                    , "OW"
                    , "#%!@&"
                },
                Color.magenta);
        }
        else
        {
            Camera.main.GetComponent<Cam>().shake += 0.3f;
            fx_hp += 0.5f;
        }

        lastHit = Time.time;
    }

    public RaycastHit? Cast(Vector3 dir)
    {
        RaycastHit hit;
        var start = cast.position;
        var ray = Physics.Raycast(start, dir, out hit, 128);

        // Debug.DrawRay(start, fwd * 128, ray ? Color.green : Color.red, 0.5f, false);

        RaycastHit? result = hit;
        return ray ? result : null;
    }

    public void Cock()
    {
        if (cockTimer > 0) return;

        if (!cocked)
        {
            Sfx(Sound.Cock);
        }

        cocked = true;
        cockTimer = roc;
    }

    public void Shoot()
    {
        if (shootTimer > 0) return;

        if (!cocked)
        {
            Sfx(Sound.Cock);
            return;
        }

        cocked = false;

        if (Player)
            Camera.main.GetComponent<Cam>().shake += 0.1f;

        var result = Cast(transform.rotation * Vector3.forward);
        if (result.HasValue)
        {
            var trans = result.Value.transform;
            if (trans.tag == "opponent" || trans.tag == "Player")
            {
                var diff = transform.position.x - trans.position.x;
                var sign = diff > 1 ? -1 : 1;
                // sign = Player ? sign * -1 : sign;
                trans.GetComponent<Char>().Hit(sign);
            }
        }

        Sfx(Sound.Fire);

        if (!Player)
        {
            TextPop(
                new[]
                {
                      "BANG"
                    , "POW"
                    , "POP"
                }, Color.white);
        }

        shootTimer = rof;
        lastShot = Time.time;
    }

    public void Tap(int dir)
    {
        Debug.Assert(dir != 0);
        angv += kick * -dir;

        if (!down)
        {
            vel += dash * dir; // Clamp?
        }

        Sfx(Sound.Move);
    }

    void Update()
    {
        Animate();
        shootTimer = Mathf.Max(0, shootTimer - Time.deltaTime);
        cockTimer = Mathf.Max(0, cockTimer - Time.deltaTime);
        fx_hp = Mathf.Max(fx_hp - Time.deltaTime, 0);
    }
}
