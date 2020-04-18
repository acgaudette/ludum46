using UnityEngine;

[RequireComponent(typeof(Char))]
public class Player : MonoBehaviour
{
    Char self;
    TextMesh hp;
    public float sens;

    void Start()
    {
        self = GetComponent<Char>();
        hp = Camera.main.transform.Find("hp").GetComponent<TextMesh>();
    }

    void Update()
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
            Camera.main.GetComponent<Cam>().heart += 0.1f;
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
}
