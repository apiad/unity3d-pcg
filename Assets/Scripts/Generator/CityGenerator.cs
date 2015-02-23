using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityGenerator : Generator {
	public float Size = 1000;
	public float Density = 0.2f;
	public float Sparsness = 0.1f;
	public int Regions = 10;

    public TerrainGenerator Terrain;

	public CityGenerator() {
		OpenSize = 100f;
	}

	protected override IEnumerable<Step<GameObject>> Open() {
		if (Terrain != null) {
            while (!Terrain.Ready) {
                yield return new Step<GameObject>(1f);
            }
        }
        
        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.localScale = new Vector3(1, 0.25f, 1) * Size * Regions * 1.5f;
        ball.renderer.material.color = new Color(Density, 1 - Density, Density * Density);
        ball.AddComponent<Fader>().MinVisibleDistance = 50000f;

        if (Terrain != null) {
            ball.transform.localPosition = this.transform.InverseTransformPoint(Terrain.SamplePoint(this.transform.position)); 
        }
        
        yield return new Step<GameObject>(ball);
	}

	protected override IEnumerable<Step<Generator>> GenerateChildren() {
        var sectors = new List<SectorGenerator>();

		for(int i=0; i<Regions; i++) {
			for(int j=0; j<Regions; j++) {
				if (!RandomBernoulli(Density)) {
                    yield return new Step<Generator>();
					continue;
				}

				float x = i * Size + RandomUniform(0, Size - 100f) - Size * Regions / 2;
				float z = j * Size + RandomUniform(0, Size - 100f) - Size * Regions / 2;
				float rot = RandomUniform(0, 45);

                Vector3 localPosition = this.transform.TransformPoint(new Vector3(x, 0, z));

                if (Terrain != null) {
                    localPosition = Terrain.SamplePoint(localPosition);

                    if (localPosition.y < 10f || Vector3.Angle(Terrain.SampleNormal(localPosition, Size), Vector3.up) > 30f) {
                        yield return new Step<Generator>();
                        continue;
                    }
                }
                else if (!RandomBernoulli(Density)) {
                    yield return new Step<Generator>();
                    continue;
                }

                var sector = CreateChildren<SectorGenerator>("Sector");

                sector.transform.localPosition = this.transform.InverseTransformPoint(localPosition);
				sector.transform.Rotate(0, rot, 0);
				sector.Radius = 100f;
                sector.Terrain = Terrain;

                sectors.Add(sector);
                yield return new Step<Generator>();
			}
		}

		bool[] cannotGrow = new bool[sectors.Count];
        int canGrow = sectors.Count;

		while (canGrow > 0) {
			for(int i=0; i<sectors.Count; i++) {
                var child = sectors[i];

				if (cannotGrow[i]) {
					continue;
				}

				for(int j=0; j<sectors.Count; j++) {
					if (i == j) continue;

                    var other = sectors[j];

					if (Vector3.Distance(child.transform.position, other.transform.position) - Size * Sparsness < (child.Radius + other.Radius)) {
						cannotGrow[i] = true;
						canGrow--;
						break;
					}
				}

				if (!cannotGrow[i]) {
					child.Radius += 5;
				}

                yield return new Step<Generator>();
			}
		}

        foreach (var sector in sectors) {
            yield return new Step<Generator>(sector);
        }
	}
}