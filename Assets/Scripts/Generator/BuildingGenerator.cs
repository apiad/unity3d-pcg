using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BuildingGenerator : Generator {
	public BlockType Type;
	public float Width;
	public float Height;
	public float Depth;

	public BuildingGenerator() {
		OpenSize = 100f;
	}

 	protected override IEnumerable<Step<GameObject>> Open() {
 		GameObject building = null;

 		switch (this.Type) {
 			case BlockType.Office:
 				building = CreateOffice();
 				break;
 			case BlockType.Residential:
 				building = CreateResidential();
 				break;
 			case BlockType.Comercial:
 				building = CreateComerial();
 				break;
 			case BlockType.Industrial:
 				building = CreateIndustrial();
 				break;
 			case BlockType.Facility:
 				building = CreateFacility();
 				break;
 			case BlockType.Park:
 				building = CreatePark();
 				break;
 			default:
 				break;
 		}

		building.transform.localScale = new Vector3(Width, Height, Depth);
        building.AddComponent<Fader>();

		yield return new Step<GameObject>(building);
	}

	private GameObject CreateOffice() {
		var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.renderer.material.color = Color.white;
		return go;
	}

	private GameObject CreateResidential() {
		var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.renderer.material.color = Color.red;
		return go;
	}

	private GameObject CreateComerial() {
		var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.renderer.material.color = Color.blue;
		return go;
	}

	private GameObject CreateIndustrial() {
		var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.renderer.material.color = Color.magenta;
		return go;
	}

	private GameObject CreateFacility() {
		var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.renderer.material.color = Color.yellow;
		return go;
	}

	private GameObject CreatePark() {
		var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		
		// int numberOfTrees = (int)RandomUniform(5, 20);

		// for(int i=0; i < numberOfTrees; i++) {
		// 	float x = (float)(this.rnd.NextDouble() - 0.5); // * this.scale.x;
		// 	float z = (float)(this.rnd.NextDouble() - 0.5); // * this.scale.z;

		// 	var tree = CreateTree();
		// 	tree.transform.position = new Vector3(x, 0, z);
		// 	tree.transform.parent = go.transform;
		// }

		go.renderer.material.color = Color.green;
		return go;
	}

	private GameObject CreateTree() {

		// float height = (float) this.rnd.NextDouble() * 5f + 5f;
		// float width = (float) this.rnd.NextDouble() * 0.05f + 0.01f;

		var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		// go.transform.localScale = new Vector3(width, height, width);
		// go.renderer.material.color = new Color(0.7f, 0.5f, 0, 1);

		// int copes = this.rnd.Next(1, 10);

		// for(int i=0; i<copes; i++) {
		// 	float size = (float) this.rnd.NextDouble() * 1f + 1f;
		// 	float cheight = (float) this.rnd.NextDouble() * 0.5f + 0.5f;

		// 	var cope = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		// 	cope.transform.position = new Vector3(0, cheight, 0);
		// 	cope.transform.localScale = new Vector3(size, 0, size);
		// 	cope.transform.parent = go.transform;
		// 	cope.renderer.material.color = Color.green;
		// } 

		return go;
	}
}
