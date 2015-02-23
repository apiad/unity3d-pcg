using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BlockGenerator : Generator {
	public float Height;
	public float Width;
	public float Depth;

    public TerrainGenerator Terrain;
	
	private BlockType type;

	public BlockGenerator() {
		OpenSize = 100f;
	}

	protected override IEnumerable<Step<GameObject>> Open() {
		if (Height > 200f) {
			this.type = BlockType.Office;
		}
		else if (Height > 150f) {
			float residentialProb = 0.5f;

			if (RandomBernoulli(residentialProb)) {
				this.type = BlockType.Residential;
			}
			else {
				this.type = BlockType.Office;
			}
		}
		else if (Height > 100f) {
			float residentialProb = 0.33f;
			float comercialProb = 0.33f;

			float p = RandomUniform(0, 1);

			if (p <= residentialProb) {
				this.type = BlockType.Residential;
			}
			else if (p <= residentialProb + comercialProb) {
				this.type = BlockType.Comercial;
			}
			else {
				this.type = BlockType.Office;
			}
		}
		else if (Height > 50f) {
			float residentialProb = 0.5f;
			float comercialProb = 0.25f;

			float p = RandomUniform(0, 1);

			if (p <= residentialProb) {
				this.type = BlockType.Residential;
			}
			else if (p <= residentialProb + comercialProb) {
				this.type = BlockType.Comercial;
			}
			else {
				this.type = BlockType.Industrial;
			}	
		}
		else if (Height > 25f) {
			float residentialProb = 0.25f;
			float comercialProb = 0.25f;

			float p = RandomUniform(0, 1);

			if (p <= residentialProb) {
				this.type = BlockType.Residential;
			}
			else if (p <= residentialProb + comercialProb) {
				this.type = BlockType.Comercial;
			}
			else {
				this.type = BlockType.Industrial;
			}	
		}
		else if (Height > 10f) {
			float facilityProb = 0.5f;
			float p = RandomUniform(0, 1);

			if (p <= facilityProb) {
				this.type = BlockType.Facility;
			}
			else {
				this.type = BlockType.Industrial;
			}	
		}
		else {
			this.type = BlockType.Park;
		}

        //var basing = new GameObject("Basing", typeof(MeshRenderer), typeof(MeshFilter)); 
        var basing = GameObject.CreatePrimitive(PrimitiveType.Cube);
		basing.transform.localScale = new Vector3(Width, 1, Depth);
        basing.transform.localPosition = Vector3.up * Height / 2f;
        basing.renderer.material.shader = Shader.Find("Diffuse");
        //basing.AddComponent<Fader>().MinVisibleDistance = 3000f;

		switch(this.type) {
			case BlockType.Office:
                basing.renderer.material.color = Color.white;
				break;
			case BlockType.Residential:
				basing.renderer.material.color = Color.red;
				break;
			case BlockType.Comercial:
				basing.renderer.material.color = Color.blue;
				break;
			case BlockType.Industrial:
				basing.renderer.material.color = Color.magenta;
				break;
			case BlockType.Facility:
				basing.renderer.material.color = Color.yellow;
				break;
			case BlockType.Park:
				basing.renderer.material.color = Color.green;
				break;
			default:
				break;
		}

        //var mesh = basing.GetComponent<MeshFilter>().mesh;

        //var vertices = mesh.vertices;

        //for (int i = 0; i < vertices.Length; i++) {
        //    var worldVertices = this.transform.TransformPoint(vertices[i]);
        //    vertices[i] += Terrain.SamplePoint(worldVertices) - worldVertices;
        //}

        //mesh.vertices = vertices;

        yield return new Step<GameObject>(basing);

        //var street = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //street.transform.localScale = new Vector3(Width + 20, 2, Depth + 20);
        //street.AddComponent<Fader>();

        //yield return street;
	}

	protected override IEnumerable<Step<Generator>> GenerateChildren() {
        if (Terrain != null) {
            while (!Terrain.Ready) {
                yield return new Step<Generator>(1f);
            }
        }

		for(int i=0; i<4; i++) {
			for(int j=0; j<4; j++) {
				float x = i * 40 - 80 + 20;
				float z = j * 40 - 80 + 20;
				float h = RandomUniform(Height/2, Height * 0.9f);
				
				var building = CreateChildren<BuildingGenerator>("Building");

                Vector3 localPosition = new Vector3(x, 0, z);

                if (Terrain != null) {
                    Vector3 transformedPosition = this.transform.TransformPoint(localPosition);
                    transformedPosition = Terrain.SamplePoint(transformedPosition);
                    localPosition = this.transform.InverseTransformPoint(transformedPosition) + Vector3.up * h/2;

                    Vector3 normal = Terrain.SampleNormal(transformedPosition, 30f);

                    if (transformedPosition.y < 10 || Vector3.Angle(normal, Vector3.up) > 30) {
                        yield return new Step<Generator>();
                        continue;
                    }
                }

                building.transform.localPosition = localPosition;
				
				building.Height = h;
				building.Width = 30;
				building.Depth = 30;
				building.Type = type;

				yield return new Step<Generator>(building);
			}
		}
	}
}
