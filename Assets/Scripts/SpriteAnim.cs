using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class SpriteAnim : MonoBehaviour
{
    public int frame;

    public bool play = false;
    public bool loop = false;
    public bool dup = false;
    public float rate = 1;
    public int count = 1;
    public int width = 32;

    Material mat;
    float timer;

    void Awake()
    {
        var render = GetComponent<MeshRenderer>();
        mat = Application.isPlaying ?
            render.material : render.sharedMaterial;
        if (dup)
            timer = -rate;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (Application.isPlaying && play)
        {
            if (timer > rate)
            {
                timer = 0;
                ++frame;
            }

            if (!loop && frame >= count)
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
}
