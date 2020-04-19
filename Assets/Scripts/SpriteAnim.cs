using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class SpriteAnim : MonoBehaviour
{
    public int frame;

    public bool play = false;
    public bool loop = false;
    public bool dup = false;
    public float delay = 1;
    public int count = 1;
    public int width = 32;
    public int clip = 0;

    Material mat;
    float timer;

    int Lim
    {
        get
        {
            return clip > 0 ? clip : count;
        }
    }

    void Awake()
    {
        var render = GetComponent<MeshRenderer>();
        mat = Application.isPlaying ?
            render.material : render.sharedMaterial;
        if (dup)
            timer = -delay;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (Application.isPlaying && play)
        {
            if (timer > delay)
            {
                timer = 0;
                ++frame;
            }

            if (!loop && frame >= Lim)
            {
                frame = count - 1;
                play = false;
            }
        }

        frame %= count;
        mat.mainTextureScale = new Vector2(1 / (float)count, 1);
        mat.mainTextureOffset = new Vector2(frame / (float)count, 0);
    }

    public void Play()
    {
        frame = 0;
        play = true;
    }

    public void Reset(int i)
    {
        frame = 1;
        play = false;
    }

    public float Len()
    {
        return Lim * delay;
    }
}
