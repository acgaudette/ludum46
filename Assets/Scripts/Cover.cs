using UnityEngine;
using System.Collections.Generic;

public class Cover
{
    List<int> field;

    public void Init(Transform host)
    {
        field = new List<int>();
        var origin = host.position;
        var fwd = host.forward;

        for (float i = -Global.Values.width; i < Global.Values.width; i += 0.5f)
        {
            var start = host.Find("_cam").Find("_cast").position;
            Vector3 curr = new Vector3(i, start.y, origin.z);

            int val = 0;

            RaycastHit hit;
            var ray = Physics.Raycast(curr, fwd, out hit, 3);
            if (ray)
            {
                val = 1;
            }

            Debug.DrawRay(curr, fwd * 3, ray ? Color.green : Color.red, 8, false);
            field.Add(val);
        }
    }

    int PosToField(float x)
    {
        return (int)(
            field.Count
                * (x + Global.Values.width)
                / (2 * Global.Values.width));
    }

    public bool Covered(float x)
    {
        int i = PosToField(x);
        return field[i] > 0;
    }

    public float NearestCover(float x)
    {
        int pos = PosToField(x);
        if (pos < 0) pos = 0;
        if (pos >= field.Count) pos = field.Count - 1;

        int left = field.Count;
        for (int i = pos; i > 0; --i)
        {
            if (0 == field[i]) continue;
            left = pos - i;
        }

        int right = field.Count;
        for (int i = pos; i < field.Count; ++i)
        {
            if (0 == field[i]) continue;
            right = i - pos;
        }

        float result = Mathf.Abs(right) > Mathf.Abs(left) ?
            -left : right;
        return (2 * Global.Values.width) * result / field.Count;
    }
}
