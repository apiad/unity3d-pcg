using UnityEngine;
using System.Collections;

public class RoadGenerator : Generator {
	/* private Vector3 origin;
	private Vector3 destination;
	private GameObject go;

	public RoadGenerator(int seed, Vector3 origin, Vector3 destination) : base(seed) {
		this.origin = origin;
		this.destination = destination;
	}

	protected override void OpenSelf() {
		Vector3 v = destination - origin;
		float l = v.magnitude;

		this.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		this.go.transform.localScale = new Vector3(Mathf.Max(2, l/100), 1, l);
		this.go.transform.position = origin;
		this.go.transform.rotation = Quaternion.LookRotation(v);
		this.go.transform.position += v/2;
		// this.go.renderer.material = RoadMaterial;
	}

	protected override void Close() {
		GameObject.Destroy(this.go);
	}
	*/
}
