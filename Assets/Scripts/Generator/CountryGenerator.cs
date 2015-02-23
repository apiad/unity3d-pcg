using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CountryGenerator : Generator {
	public float Size = 50000;
	public float Density = 0.2f;
	public float Sparsness = 0.1f;
	public int Regions = 10;

	protected override IEnumerable<Step<GameObject>> Open() {
		var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.transform.localScale = new Vector3(100000, 0, 100000);

        yield return new Step<GameObject>(plane);
	}

	protected override IEnumerable<Step<Generator>> GenerateChildren() {
		for(int i=0; i<Regions; i++) {
			for(int j=0; j<Regions; j++) {
				if (!RandomBernoulli(Density)) {
                    yield return new Step<Generator>();
					continue;
				}

				float x = i * Size + RandomUniform(0, Size - 20000f) - Size * Regions / 2;
				float z = j * Size + RandomUniform(0, Size - 20000f) - Size * Regions / 2;

				var city = CreateChildren<CityGenerator>("City");
				city.transform.localPosition = new Vector3(x, 0, z);

				yield return new Step<Generator>(city);
			}
		}
	}
}
