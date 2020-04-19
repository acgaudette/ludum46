using UnityEngine;

public class PData : MonoBehaviour
{
    public int level;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.name = "_pdata";
    }
}
