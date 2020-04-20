using UnityEngine;

public class PData : MonoBehaviour
{
    public int level;
    [Range(0, 1)] public float sens = 0.1f;
    public bool xactive = true;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.name = "_pdata";
    }
}
