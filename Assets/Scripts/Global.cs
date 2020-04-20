using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Global : MonoBehaviour
{
    [Range(0, 1)] public float thresh = 0.35f;
    [Range(0, 1)] public float flat = 0.9f;
    [Range(0, 1)] public float spin = 0.6f;
    public float rof = 0.15f;
    public float roc = 0.08f;
    public float fall = 0.01f;
    public float damp = 8;
    public float sleep = 0.5f;
    public float width = 8;
    public float loadTime;
    public float deathKick = 8;
    public float shoutTime = 0.2f;
    public Color[] colors;
    public float flash = 0.2f;
    public Bot[] levels;
    public int forceLevel = -1;

    public GameObject pdataPrefab;
    public static PData pdata;
    public static float invert;
    public static float fx_hp;
    Color clear;
    GameObject cover;
    Char[] chars;

    [System.Serializable]
    public class Invert
    {
        public MeshRenderer target;
        public Color alt;

        Color def;
        Material mat;

        public void Init()
        {
            Debug.Assert(target != null);
            mat = target.material;
            def = mat.color;
        }

        public void SetAlt()
        {
            mat.color = alt;
        }

        public void SetDef()
        {
            mat.color = def;
        }
    }

    public Invert[] inverts;
    public Texture2D cursor;

    void Start()
    {
        Debug.Assert(colors.Length == 2);
        clear = Camera.main.backgroundColor;

        if (pdata == null)
        {
            var obj = GameObject.Find("_pdata");
            if (obj == null)
            {
                obj = GameObject.Instantiate(pdataPrefab);
            }

            pdata = obj.GetComponent<PData>();
        }

        if (forceLevel >= 0)
            pdata.level = forceLevel;

        cover = GameObject.Find("_cover");
        foreach (var inv in inverts) inv.Init();
        chars = Object.FindObjectsOfType<Char>();

        foreach (var ch in chars)
        {
            var opp = ch.GetComponent<Opp>();
            if (opp == null) continue;
            opp.bot = pdata.level >= levels.Length ?
                levels[levels.Length - 1] : levels[pdata.level];
        }
    }

    void Level(int l)
    {
        pdata.level = l;
        SceneManager.LoadScene("Main");
    }

    void Update()
    {
        if (invert > 0)
        {
            invert -= Time.unscaledDeltaTime;
            Time.timeScale = 0;
            cover.SetActive(false);
            foreach (var inv in inverts) inv.SetAlt();
            Color col = Time.unscaledTime % flash < flash * .5f ?
                colors[0] : colors[1];
            Camera.main.backgroundColor = col;
            return;
        }

        cover.SetActive(true);
        Time.timeScale = 1;
        foreach (var inv in inverts) inv.SetDef();
        Camera.main.backgroundColor = clear;

        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            Level(0);
        }
        */

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
        // Cursor.lockState = CursorLockMode.Locked; // Bugs out mouse on Linux

        fx_hp = Mathf.Max(fx_hp - Time.deltaTime, 0);
    }

    IEnumerator DelayLoad(int l)
    {
        yield return new WaitForSeconds(loadTime);
        Level(l);
    }

    public static void Iter(bool loss)
    {
        pdata.level = loss ? 0 : pdata.level + 1;
        Values.StartCoroutine(Values.DelayLoad(pdata.level));
    }

    public static void Dispatch(Transform body)
    {
        foreach (var ch in Values.chars)
        {
            if (ch.transform == body) continue;

            var dir = ch.transform.position - body.position;
            dir.Normalize();

            var src = body.GetComponent<Char>();
            var result = src.Cast(dir);
            if (result.HasValue && result.Value.transform.GetComponent<Char>() != null)
            {
                var amt = Vector3.Dot(dir, body.forward);
                ch.Miss(src, amt);
            }
        }
    }

    public static int MobCount(bool player)
    {
        int result = 0;

        foreach (var ch in Values.chars)
        {
            if (ch.hp == 0) continue;
            var mob = ch.GetComponent<Mob>();
            if (mob == null) continue;
            result += mob.pside == player ? 1 : 0;
        }

        return result;
    }

    public static Global inst;
    public static Global Values
    {
        get
        {
            if (inst == null)
            {
                inst = GameObject.Find("_global")
                    .GetComponent<Global>();
            }

            return inst;
        }
    }
}
