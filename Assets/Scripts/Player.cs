using UnityEngine;

[RequireComponent(typeof(Char))]
public class Player : MonoBehaviour
{
    Char self;

    void Start()
    {
        self = GetComponent<Char>();
    }

    void Update()
    {
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

        self.look = Input.mousePosition.x / Screen.width;
        self.look = 2 * self.look - 1;

        var fire = Input.GetKeyDown(KeyCode.Space)
            || Input.GetMouseButtonDown(0);
        if (fire)
        {
            Camera.main.GetComponent<Cam>().shake += 0.1f;
            self.Shoot();
        }
    }
}
