using UnityEngine;

[RequireComponent(typeof(Char))]
public class Opp : MonoBehaviour
{
    public Char enemy;
    public Bot bot;

    Vector3 lastTarget;
    Vector3 lastSelf;

    bool los;
    bool shot;
    float locked;
    float exposure;
    float discovered;
    float seenDelay;
    float underFire;

    float[] scores;
    Action action;

    Char self;
    Cover cover;
    Quaternion target;
    float timer;
    int switchSide = 1;
    float seen;
    float brainTimer;

    enum Action
    {
          Cover
        , Peek
        , Aim
        , Fire
        , Switch
        , Hide
        , Cock
        , Recover
        , Scan
    };

    void Start()
    {
        self = GetComponent<Char>();
        lastSelf = -Vector3.up;
        self.manual = target = Quaternion.AngleAxis(180, Vector3.up);

        cover = new Cover();
        cover.Init(transform);
    }

    void Move(int side, float urg)
    {
        urg *= 0.95f;
        float val = 1 - urg;
        if (timer > val)
        {
            self.Tap(side);
            timer = 0;
        }
    }

    void Peek(float urg)
    {
        if (cover.Covered(lastTarget.x))
        {
            Move(cover.NearestCover(transform.position.x) > 0 ? -1 : 1, 0.5f);
            return;
        }

        var side = transform.position.x > lastTarget.x ? -1 : 1;
        Move(side, urg);
    }

    void Recover()
    {
        if (timer > 0.1f)
        {
            self.Tap(-self.downside);
            timer = 0;
        }
    }

    void Collect()
    {
        los = false;
        var dir = enemy.transform.position - transform.position;
        dir.Normalize();

        var win = 0.1f;
        if (Time.time - self.lastMiss < win || Time.time - self.lastHit < win)
        {
            if (self.lastAggr != null)
            {
                lastTarget = self.lastAggr.transform.position;
                lastSelf = transform.position;
            }
        }

        if (Vector3.Dot(dir, transform.forward) > 0.5f)
        {
            var result = self.Cast(dir);
            if (result.HasValue)
            {
                var body = result.Value.transform;
                var dist = result.Value.distance;
                var tag = body.tag;

                if (tag == "Player")
                {
                    los = true;
                    lastTarget = body.position;
                    lastSelf = transform.position;
                    seen = 0;

                    // Debug.DrawRay(transform.position, dir * dist, Color.green);
                }
                else
                {
                    // Debug.DrawRay(transform.position, dir * dist, Color.red);
                }
            }
        }

        Debug.DrawLine(lastSelf, lastTarget, Color.magenta);

        locked = los ? Vector3.Dot(transform.forward, dir) : 0;
        shot = locked >= 1 - (bot.spread + 0.005f);
        locked = Mathf.Max(0, locked);

        /*
        float exposure = self.down ? 0.5f : 0;
        Vector3[] checks = new Vector3[]
        {
            Vector3.forward,
            (new Vector3( 0.5f, 0, 1)).normalized,
            (new Vector3(-0.5f, 0, 1)).normalized,
        };

        foreach (var check in checks)
        {
            var result = self.Cast(check);
            if (result.HasValue)
            {
                if (result.Value.distance < 2)
                    continue;
            }

            exposure += 0.5f / 3;
        }
        */

        exposure = Mathf.Clamp01(
            1 - Mathf.Abs(cover.NearestCover(transform.position.x)) / (2 * Global.Values.width)
        );

        float disc = Vector3.Distance(lastSelf, transform.position);
        if (lastSelf.y < 0) disc = 2 * Global.Values.width;
        discovered = Mathf.Clamp01(1 - disc / (2 * Global.Values.width));

        seen += Time.deltaTime;
        seenDelay = Mathf.Clamp01(seen / 3);

        if (transform.position.x > Global.Values.width - 1)
        {
            switchSide = -1;
        }
        else if (transform.position.x < -Global.Values.width + 1)
        {
            switchSide = 1;
        }

        var max = self.lastMiss > self.lastHit ?
            self.lastMiss : self.lastHit;
        underFire = 1 - Mathf.Clamp01((Time.time - max) / 2);
    }

    void FixedUpdate()
    {
        brainTimer += Time.deltaTime;
        if (brainTimer < bot.brainRate)
        {
            return;
        }

        brainTimer = 0;

        float coverScore = 0;
        coverScore += (1 - locked) * exposure;
        coverScore *= (1 - bot.aggro);
        // coverScore += bot.fear * underFire;

        float peekScore = shot ? 0 : 1;

        float aimScore = 0;
        aimScore += (1 - locked);
        aimScore += seenDelay;
        aimScore += shot ? 0 : 1;
        aimScore /= 3;

        float fireScore = shot ? 1 : 0;
        fireScore += 2 * bot.trhappy;

        float switchScore = 0;
        switchScore += los ? 0 : 1;
        switchScore += discovered;
        switchScore /= 2;
        switchScore *= 2 * bot.mobile;
        switchScore *= (1 - bot.aggro);

        // Put here instead of cover
        // for better running
        switchScore += bot.fear * underFire;

        float hideScore = 0;
        hideScore += 1 - exposure;
        hideScore *= 2 // (!)
            * bot.sneak;

        float cockScore = self.cocked ? 0 : 1;
        cockScore *= 2 // (!)
            * (1 - bot.forget);

        float recScore = self.down ? 1 : 0;
        recScore *= 2; // (!)

        scores = new float[]
        {
            coverScore,
            peekScore,
            aimScore,
            fireScore,
            switchScore,
            hideScore,
            cockScore,
            recScore,
        };

        scores[(int)action] += bot.hysteresis;

        float total = 0;
        foreach (var score in scores)
        {
            total += score;
        }

        float r = Random.Range(0, total);
        float acc = 0;

        for (int i = 0; i < scores.Length; ++i)
        {
            acc += scores[i];
            if (acc < r) continue;
            action = (Action)i;
            break;
        }
    }

    void Update()
    {
        if (0 == self.hp)
            return;

        Debug.Assert(enemy != null);
        if (Global.invert > 0) return;
        Collect();

        // target = Quaternion.AngleAxis(180, Vector3.up);
        // var urg = 0.5f;

        var urg = bot.baseSpeed;
        urg += bot.stress * underFire * (1 - bot.baseSpeed);

        switch (action)
        {
        case Action.Cover:
            // urg = bot.coverSpeed + stress * (1 - bot.coverSpeed);
            Move(cover.NearestCover(transform.position.x) > 0 ? 1 : -1, urg);
            break;
        case Action.Peek:
            // urg = bot.peekSpeed;
            Peek(urg);
            break;
        case Action.Aim:
            var guess = lastTarget - transform.position;
            guess.Normalize();
            target = Quaternion.LookRotation(guess, Vector3.up);
            break;
        case Action.Fire:
            self.Shoot();
            break;
        case Action.Switch:
            var side = switchSide;
            // urg = bot.switchSpeed;
            Move(switchSide, urg);
            break;
        case Action.Hide:
            break;
        case Action.Cock:
            self.Cock();
            break;
        case Action.Recover:
            Recover();
            break;
        }

        timer += Time.deltaTime;
        var t = bot.aim * Time.deltaTime;
        self.manual = Quaternion.Slerp(self.manual, target, t);
    }
}
