using UnityEngine;

public class Mob : MonoBehaviour
{
    public Bot bot;

    float exposure;
    float stress;

    Char self;
    [HideInInspector] public bool pside;

    Cover cover;
    float timer;
    float brainTimer;
    float[] scores;
    int switchSide = 1;

    enum Action
    {
          Hide
        , Cover
        , Switch
    };

    Action action;

    void Start()
    {
        self = GetComponent<Char>();
        pside = Vector3.Dot(transform.forward, Vector3.forward) > 0;
        cover = new Cover();
        cover.Init(transform);

        float x = Random.Range(-Global.Values.width, Global.Values.width);
        transform.position += Vector3.right * x;
    }

    void Move(int side)
    {
        if (timer > bot.baseSpeed)
        {
            self.Tap(side);
            timer = 0;
        }
    }

    void Collect()
    {
        var x = transform.position.x;
        exposure = Mathf.Clamp01(
            1 - Mathf.Abs(cover.NearestCover(x)) / (2 * Global.Values.width));

        stress = 1 - Mathf.Clamp01((Time.time - self.lastMiss) / 2);

        if (transform.position.x > Global.Values.width - 1)
        {
            switchSide = -1;
        }
        else if (transform.position.x < -Global.Values.width + 1)
        {
            switchSide = 1;
        }
    }

    void FixedUpdate()
    {
        brainTimer += Time.deltaTime;
        if (brainTimer < bot.brainRate)
        {
            return;
        }

        brainTimer = 0;

        float hideScore = 0;
        hideScore += exposure;

        float coverScore = 0;
        coverScore += stress;

        float switchScore = 0;
        switchScore += .5f;

        scores = new float[]
        {
            hideScore,
            coverScore,
            switchScore,
        };

        scores[(int)action] += bot.hysteresis;

        float total = 0;
        foreach (var score in scores) total += score;

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
        timer += Time.deltaTime;
        self.manual = pside ?
              Quaternion.identity
            : Quaternion.AngleAxis(180, Vector3.up);
        if (0 == self.hp) return;

        Collect();
        switch (action)
        {
        case Action.Hide:
            break;
        case Action.Cover:
            var x = transform.position.x;
            Move(cover.NearestCover(x) > 0 ? 1 : -1);
            break;
        case Action.Switch:
            var side = switchSide;
            Move(switchSide);
            break;
        }
    }
}
