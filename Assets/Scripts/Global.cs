using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Global : MonoBehaviour
{
    [Range(0, 1)] public float thresh = 0.35f;
    [Range(0, 1)] public float flat = 0.9f;
    [Range(0, 1)] public float spin = 0.6f;
    public float sleep = 0.5f;
    public float width = 8;
    public float loadTime;
    public float deathKick = 8;

    public static float invert;
    [HideInInspector] public int level;
    Color clear;

    public Texture2D cursor;

    void Start()
    {
        clear = Camera.main.backgroundColor;
        DontDestroyOnLoad(gameObject);
    }

    void Level(int l)
    {
        level = l;
        SceneManager.LoadScene("Main");
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
            Level(0);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.ForceSoftware);
        // Cursor.lockState = CursorLockMode.Locked; // Bugs out mouse on Linux
    }

    IEnumerator DelayLoad(int l)
    {
        yield return new WaitForSeconds(loadTime);
        Level(l);
    }

    public static void Iter(bool loss)
    {
        Values.level = loss ? 0 : Values.level + 1;
        Values.StartCoroutine(Values.DelayLoad(Values.level));
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
