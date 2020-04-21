using UnityEngine;

public class PData : MonoBehaviour
{
    public int level;
    public int sens = 10;
    public bool xactive = true;
    public int volume = 5;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.name = "_pdata";
    }
}
