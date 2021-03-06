using UnityEngine;

[RequireComponent(typeof(Char))]
public class Player : MonoBehaviour
{
    public float sway;
    public Vector2 raise;
    public float swayDamp;
    public float kick;
    public GameObject cross;

    Char self;
    TextMesh hp;
    TextMesh sensobj;
    TextMesh volobj;
    TextMesh lvl;
    Transform gun;
    Vector3 gunCenter;
    Cam cam;
    float sensTimer;
    float volTimer;

    void Start()
    {
        self = GetComponent<Char>();
        hp = Camera.main.transform.Find("_hp").GetComponent<TextMesh>();
        sensobj = Camera.main.transform.Find("_sens").GetComponent<TextMesh>();
        volobj = Camera.main.transform.Find("_vol").GetComponent<TextMesh>();
        lvl = Camera.main.transform.Find("_lvl").GetComponent<TextMesh>();
        gun = Camera.main.transform.Find("_revolver");
        gunCenter = gun.localPosition;
        cam = Camera.main.GetComponent<Cam>();
    }

    void Render()
    {
        sensTimer -= Time.deltaTime;
        volTimer -= Time.deltaTime;

        sensobj.text = Global.Sens.ToString();
        if (sensTimer > 0)
        {
            sensobj.gameObject.SetActive(true);
        }
        else
        {
            sensobj.gameObject.SetActive(false);
        }

        volobj.text = Global.Volume.ToString();
        if (volTimer > 0)
        {
            volobj.gameObject.SetActive(true);
        }
        else
        {
            volobj.gameObject.SetActive(false);
        }

        hp.text = self.hp.ToString();

        if (self.fx_hp > 0)
        {
            hp.gameObject.SetActive(Time.time % 0.1f > 0.05f);
        }
        else
        {
            hp.gameObject.SetActive(false);
        }

        lvl.text = Global.pdata.level.ToString();
        if (Time.timeSinceLevelLoad < 1)
        {
            lvl.gameObject.SetActive(Time.time % 0.1f > 0.05f);
        }
        else
        {
            lvl.gameObject.SetActive(false);
        }

        var sign = self.angle > 0 ? 1 : -1;
        var mag = sign * self.angv;
        mag *= mag > 0 ? raise.x : raise.y;
        var offset = new Vector3(
            -self.vel * sway,
            mag - cam.h * 0.001f,
            0
        );

        var swayPt = gunCenter + offset
            - Vector3.forward * (self.cocked ? 0.005f: 0);
        gun.localPosition = Vector3.Lerp(gun.localPosition, swayPt, Time.deltaTime * swayDamp);

        var t = Mathf.Clamp01(10 * (Time.time - self.lastShot));

        /*
        var kickup = Vector3.zero;
        if (t < 1)
        {
            t = 1 - t * t * t;
            kickup = Vector3.up * t * kick;
        }

        gun.localPosition = gunAnim + kickup;
        */

        Camera.main.transform.localRotation = Quaternion.AngleAxis(
            (1 - t) * kick,
            -Vector3.right
        );

        cross.SetActive(Global.XActive);
    }

    void Control()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Global.Sens = Mathf.Clamp(Global.Sens + 1, 1, 50);
            sensTimer = 1;
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Global.Sens = Mathf.Clamp(Global.Sens - 1, 1, 50);
            sensTimer = 1;
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Global.Volume = Mathf.Clamp(Global.Volume - 1, 0, 10);
            volTimer = 1;
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Global.Volume = Mathf.Clamp(Global.Volume + 1, 0, 10);
            volTimer = 1;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Global.XActive = !Global.XActive;
        }

        if (0 == self.hp)
            return;

        if (Global.invert > 0) return;

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
            cam.heart += 0.1f;
            self.Tap(dir);
        }

        /* Screen size dependent, but the alternative is broken on linux
        self.look = Input.mousePosition.x / Screen.width;
        self.look = 2 * self.look - 1;
        */

        var sens = Global.Sens / 100.0f;
        self.look += Input.GetAxis("Mouse X") * sens;
        self.look = Mathf.Max(-1, Mathf.Min(1, self.look));

        var fire = Input.GetKeyDown(KeyCode.Space)
            || Input.GetMouseButtonDown(0);
        if (fire)
        {
            self.Shoot();
        }

        var cock = Input.GetKeyDown(KeyCode.LeftAlt)
            || Input.GetMouseButtonDown(1);
        if (cock)
        {
            self.Cock();
        }
    }

    void Update()
    {
        Render();
        Control();
    }
}
