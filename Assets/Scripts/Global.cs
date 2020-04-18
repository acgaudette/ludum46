using UnityEngine;
using UnityEngine.SceneManagement;

public class Global : MonoBehaviour
{
    [Range(0, 1)] public float thresh = 0.35f;
    [Range(0, 1)] public float flat = 0.9f;
    [Range(0, 1)] public float spin = 0.6f;
    public float sleep = 0.5f;
    public float width = 8;

    public static float invert;
    Color clear;

    void Start()
    {
        clear = Camera.main.backgroundColor;
    }

    void Update()
    {
        if (invert > 0)
        {
            invert -= Time.unscaledDeltaTime;
            Time.timeScale = 0;
            Camera.main.backgroundColor = Color.white;
            return;
        }

        Camera.main.backgroundColor = clear;
        Time.timeScale = 1;

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Main");
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
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
