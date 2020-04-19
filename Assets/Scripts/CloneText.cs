using UnityEngine;

public class CloneText : MonoBehaviour
{
    public TextMesh src;
    TextMesh dst;

    void Start()
    {
        dst = GetComponent<TextMesh>();
        Debug.Assert(dst != null);
    }

    void LateUpdate()
    {
        dst.text = src.text;
    }
}
