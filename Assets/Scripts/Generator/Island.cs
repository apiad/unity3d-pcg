using UnityEngine;

public class Island {
    public Island(Vector3 center, float radius) {
        this.Center = center;
        this.Radius = radius;
    }

    public Vector3 Center { get; set; }
    public float Radius { get; set; }
}