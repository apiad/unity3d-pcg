using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SectorGenerator : Generator {
    public float Radius;

    public SectorGenerator() {
        OpenSize = 100f;
    }

    protected override IEnumerable<Step<GameObject>> Open() {
        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.localScale = new Vector3(1, 0.25f, 1) * 2 * Radius;
        ball.renderer.material.color = new Color(0.8f, 0.2f, 0.4f);
        //ball.AddComponent<Fader>().MinVisibleDistance = 20000f;

        yield return new Step<GameObject>(ball);
    }

    protected override IEnumerable<Step<Generator>> GenerateChildren() {
        int count = 0;
        float gridSize = 200;
        float maxHeight = 200f;
        float minHeight = 2f;
        float maxRadius = 1000f;

        float maxProb = 1f;
        float minProb = 0f;

        if (Terrain != null) {
            while (!Terrain.Ready) {
                yield return new Step<Generator>(1f);
            }
        }

        for (float x = -Radius; x <= Radius; x += gridSize) {
            for (float z = -Radius; z <= Radius; z += gridSize) {
                x = x - x % gridSize;
                z = z - z % gridSize;

                Vector3 childCenter = new Vector3(x, 0, z);
                float d = childCenter.magnitude;
                float ds = (Radius - d) / Radius;
                float p = (maxProb - minProb) * ds + minProb;

                if (d > Radius - gridSize / 2 || !RandomBernoulli(p)) {
                    yield return new Step<Generator>();
                    continue;
                }

                if (Terrain != null) {
                    Vector3 transformedCenter = this.transform.TransformPoint(childCenter);
                    transformedCenter = Terrain.SamplePoint(transformedCenter);
                    childCenter = this.transform.InverseTransformPoint(transformedCenter);
                
                    Vector3 normal = Terrain.SampleNormal(transformedCenter, gridSize);

                    Debug.DrawRay(transformedCenter, normal * 100f, Color.red, 100f);

                    if (transformedCenter.y < 10 || Vector3.Angle(normal, Vector3.up) > 30) {
                        yield return new Step<Generator>();
                        continue;
                    }
                }

                float mh = Mathf.Max(2 * minHeight, maxHeight * ds * this.Radius / maxRadius);
                float h = RandomUniform(minHeight, mh);

                count++;

                var block = CreateChildren<BlockGenerator>("Block");
                block.transform.localPosition = childCenter;
                block.Width = 160;
                block.Depth = 160;
                block.Height = h;
                block.Terrain = Terrain;

                yield return new Step<Generator>(block);
            }
        }
    }

    public TerrainGenerator Terrain;
}
