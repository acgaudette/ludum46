using UnityEngine;

[RequireComponent(typeof(Char))]
public class Player : MonoBehaviour
{
    // public float sens;
    public float sway;
    public float raise;
    public float swayDamp;
    public float kick;

    Char self;
    TextMesh hp;
    TextMesh lvl;
    Transform gun;
    Vector3 gunCenter;
    Cam cam;

    void Start()
    {
        self = GetComponent<Char>();
        hp = Camera.main.transform.Find("_hp").GetComponent<TextMesh>();
        lvl = Camera.main.transform.Find("_lvl").GetComponent<TextMesh>();
        gun = Camera.main.transform.Find("_revolver");
        gunCenter = gun.localPosition;
        cam = Camera.main.GetComponent<Cam>();
    }

    void Render()
    {
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
        var offset = new Vector3(
            -self.vel * sway,
            sign * self.angv * raise - cam.h * 0.001f,
            0
        );

        var swayPt = gunCenter + offset
            - Vector3.forward * (self.cocked ? 0.01f: 0);
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
    }

    void Control()
    {
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

        // Screen size dependent, but the alternative is broken on linux
        self.look = Input.mousePosition.x / Screen.width;
        self.look = 2 * self.look - 1;

        /*
        self.look += Input.GetAxis("Mouse X") * sens;
        self.look = Mathf.Max(-1, Mathf.Min(1, self.look));
        */

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
