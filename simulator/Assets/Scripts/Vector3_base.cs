using UnityEngine;
public class Vector3_base {
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    public Vector3_base(Vector3 v) {
        x = v.x;
        y = v.y;
        z = v.z;
    }
}
