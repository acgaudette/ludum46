using UnityEngine;
using UnityEngine.SceneManagement;

public class Tut : MonoBehaviour
{
    public string[] messages;

    int i;
    Char self;
    float timer = 1;

    void Start()
    {
        self = GetComponent<Char>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 3)
        {
            self.TextPop(new[] { messages[i] }, Color.yellow, true);
            i = (i + 1) % messages.Length;
            timer = 0;
        }

        if (self.hp == 2 && Global.invert < 0)
        {
            SceneManager.LoadScene("Main");
        }
    }
}
