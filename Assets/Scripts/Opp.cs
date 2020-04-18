using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Char))]
public class Opp : MonoBehaviour
{
    public Char opp;
    public float aim;
    public float spread;

    Vector3 lastTarget;
    Vector3 lastSelf;
    bool los;
    bool shot;
    float locked;
    float exposure;
    float discovered;

    float[] scores;
    Action action;

    Char self;
    List<int> cover;
    Quaternion target;
    float timer;
    int switchSide = 1;

    enum Action
    {
          Cover
        , Peek
        , Aim
        , Fire
        , Switch
        , Hide
        , Scan
    };

    void Start()
    {
        self = GetComponent<Char>();
        lastSelf = -Vector3.up;
        target = Quaternion.AngleAxis(180, Vector3.up);
        cover = new List<int>();

        Vector3 origin = transform.position;
        Vector3 fwd = transform.forward;
        for (float i = -Global.Values.width; i < Global.Values.width; i += 0.5f)
        {
            var start = transform.Find("_cast").position;
            Vector3 curr = new Vector3(
                i,
                start.y, // FIXME
                origin.z
            );

            int val = 0;

            RaycastHit hit;
            var ray = Physics.Raycast(curr, fwd, out hit, 3);
            if (ray)
            {
                val = 1;
            }

            Debug.DrawRay(curr, fwd * 3, ray ? Color.green : Color.red, 8, false);
            cover.Add(val);
        }
    }

    float NearestCover()
    {
        int pos = (int)(
            cover.Count
                * (transform.position.x + Global.Values.width)
                / (2 * Global.Values.width)
            );

        if (pos < 0) pos = 0;
        if (pos >= cover.Count) pos = cover.Count - 1;

        int left = cover.Count;
        for (int i = pos; i > 0; --i)
        {
            if (0 == cover[i]) continue;
            left = pos - i;
        }

        int right = cover.Count;
        for (int i = pos; i < cover.Count; ++i)
        {
            if (0 == cover[i]) continue;
            right = i - pos;
        }

        float result = Mathf.Abs(right) > Mathf.Abs(left) ?
            -left : right;
        return (2 * Global.Values.width) * result / cover.Count;
    }

    void Move(int side, float urg)
    {
        if (timer > 1 - urg)
        {
            self.Tap(side);
            timer = 0;
        }
    }

    void Update()
    {
        if (0 == self.hp)
            return;

        Debug.Assert(opp != null);
        if (Global.invert > 0) return;

        los = false;
        var dir = opp.transform.position - transform.position;
        dir.Normalize();

        if (Vector3.Dot(dir, transform.forward) > 0.5f)
        {
            RaycastHit hit;
            var start = transform.Find("_cast").position;
            var ray = Physics.Raycast(start, dir, out hit, 128);
            if (ray)
            {
                var tag = hit.transform.tag;
                if (tag == "Player")
                {
                    los = true;
                    lastTarget = hit.transform.position;
                    lastSelf = transform.position;
                }
            }
        }

        locked = los ? Vector3.Dot(transform.forward, dir) : 0;
        shot = locked > 1 - spread;
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
            1 - Mathf.Abs(NearestCover()) / (2 * Global.Values.width)
        );

        float disc = Vector3.Distance(lastSelf, transform.position);
        if (lastSelf.y < 0) disc = 2 * Global.Values.width;
        discovered = Mathf.Clamp01(1 - disc / (2 * Global.Values.width));

        /* ... */

        float coverScore = 0;
        coverScore += 1 - locked;
        coverScore += exposure;
        coverScore /= 2;

        float peekScore = shot ? 0 : 1;

        float aimScore = locked;
        float fireScore = shot ? 1 : 0;

        float switchScore = 0;
        switchScore += los ? 0 : 1;
        switchScore += discovered;
        switchScore /= 2;

        float hideScore = 1 - exposure;
        hideScore *= 2;
        // hideScore *= 0.5f;

        scores = new float[]
        {
            coverScore,
            peekScore,
            aimScore,
            fireScore,
            switchScore,
            hideScore,
        };

        scores[(int)action] += 4;

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

        if (transform.position.x > Global.Values.width - 1)
        {
            switchSide = -1;
        }
        else if (transform.position.x < -Global.Values.width + 1)
        {
            switchSide = 1;
        }

        // target = Quaternion.AngleAxis(180, Vector3.up);
        switch (action)
        {
        case Action.Cover:
            // TODO: cover selection, urgency
            Move(NearestCover() > 0 ? 1 : -1, 0.7f);
            break;
        case Action.Peek:
            // Move(NearestCover() > 0 ? -1 : 1, 0.5f);
            var side = transform.position.x > lastTarget.x ? -1 : 1;
            Move(side, 0.2f);
            break;
        case Action.Aim:
            target = Quaternion.LookRotation(dir, Vector3.up);
            break;
        case Action.Fire:
            self.Shoot();
            break;
        case Action.Switch:
            side = switchSide;
            Move(switchSide, 0.7f);
            break;
        case Action.Hide:
            break;
        }

        timer += Time.deltaTime;
        self.manual = Quaternion.Slerp(self.manual, target, Time.deltaTime * aim);
    }
}
