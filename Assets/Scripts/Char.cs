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

    public float knock;
    public bool invincible;
    public int maxHp = 3;

    // public float upright;
    // public float deadzone;

    public TextMesh shoutPrefab;
    public MeshRenderer splatPrefab;
    public SpriteAnim flashPrefab;

    [HideInInspector] public float fx_hp;
    [HideInInspector] public float lastHit;
    [HideInInspector] public float lastMiss;
    [HideInInspector] public Char lastAggr;
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
    SpriteAnim revolver;

    enum Sound
    {
          Fire
        , Hit
        , Fall
        , Cock
        , Move
        , Misfire
        , Win
        , Lose
        , Begin
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

        revolver = Camera.main.transform.Find("_revolver")
            .GetComponent<SpriteAnim>();

        hp = maxHp;
        lastHit = -16;

        Sfx(Sound.Begin);
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
            angle = Mathf.Lerp(angle, 0, Time.deltaTime * Global.Values.damp);
            angv *= 0.9f;
        }
        else
        {
            angv += dir * Global.Values.fall;
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

        var pos = transform.position + Vector3.up * 2 + Vector3.forward * 0.75f;
        pos += new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);

        var orient = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        orient *= Quaternion.AngleAxis(Random.Range(-30, 30), Camera.main.transform.forward);

        var pop = GameObject.Instantiate(shoutPrefab, pos, orient)
            .GetComponent<TextMesh>();
        pop.text = caps;
        pop.color = col;
        GameObject.Destroy(pop.gameObject, Global.Values.shoutTime);
    }

    void Sfx(Sound sound)
    {
        var clip = snd[(int)sound];
        clip.pitch = Random.Range(0.9f, 1.1f);
        clip.Play();
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

    public void Miss(Char src, float amt)
    {
        if (amt < 0.98f) return;
        // Debug.DrawRay(src.transform.position, src.transform.forward * 128, Color.cyan, 1);
        lastMiss = Time.time;
        lastAggr = src;
    }

    public void Hit(Char src, int side)
    {
        if (!invincible) --hp;
        if (hp < 0) hp = 0;

        if (0 == hp)
        {
            vel += side * Global.Values.deathKick;
            angv += side * Global.Values.deathKick * -0.75f;
            Global.Iter(Player);
            Sfx(Player ? Sound.Lose : Sound.Win);
        }
        else
        {
            vel += side * knock;
            angv += side * knock * -0.005f;
            Sfx(Sound.Hit);
        }

        if (!Player)
        {
            Global.invert += Global.Values.sleep;
            TextPop(
                new[]
                {
                      "OUCH"
                    , "OW"
                    , "#%!@&"
                    , "&#!@$"
                },
                Color.red);
        }
        else
        {
            Camera.main.GetComponent<Cam>().shake += 0.3f;
            fx_hp += 0.5f;
        }

        lastHit = Time.time;
        lastAggr = src;
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
        if (Player && revolver.play) return;

        if (!cocked)
        {
                Sfx(Sound.Cock);
                if (Player)
                    revolver.Reset(1);
        }

        cocked = true;
        cockTimer = Global.Values.roc;
    }

    public void Shoot()
    {
        if (shootTimer > 0) return;

        if (!cocked)
        {
            Sfx(Sound.Misfire);
            // TODO: position dependent
            // Global.Dispatch(transform);
            shootTimer = Global.Values.rof;
            return;
        }

        cocked = false;
        Global.Dispatch(transform);

        if (Player)
        {
            Camera.main.GetComponent<Cam>().shake += 0.1f;
            revolver.Play();
        }

        var result = Cast(transform.rotation * Vector3.forward);
        if (result.HasValue)
        {
            var hit = result.Value;
            var body = hit.transform;
            if (body.tag == "opponent" || body.tag == "Player")
            {
                var diff = transform.position.x - body.position.x;
                var sign = diff > 1 ? -1 : 1;
                // sign = Player ? sign * -1 : sign;
                body.GetComponent<Char>().Hit(this, sign);

                /*
                if (!Player)
                {
                    TextPop(
                        new[]
                        {
                              "GET SOME"
                            , "EAT IT"
                        },
                        Color.magenta);
                }
                */
            }
            else
            {
                if (Player && splatPrefab)
                {
                    var orient = Quaternion.LookRotation(hit.normal, Vector3.up);
                    var pos = hit.point + orient * Vector3.forward * 0.01f;
                    var splat = GameObject.Instantiate(splatPrefab, hit.point, orient);
                }
            }

            if (Player)
            {
                var orient = Quaternion.LookRotation(-Camera.main.transform.forward, Vector3.up);
                var pos = hit.point + orient * Vector3.forward * 0.1f;
                var flash = GameObject.Instantiate(flashPrefab, hit.point, orient);
                GameObject.Destroy(flash.gameObject, 1);
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
                }, Color.yellow);
        }

        shootTimer = Global.Values.rof;
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
