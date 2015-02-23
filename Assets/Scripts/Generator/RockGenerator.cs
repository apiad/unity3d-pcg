using System.Collections.Generic;
using UnityEngine;

class RockGenerator : Generator {
    private Vector3 scale;

    public int Level = 2;
    private GameObject sphere;

    protected override IEnumerable<Step<GameObject>> Open() {
        scale = new Vector3(RandomUniform(1f, 2f), RandomUniform(0.5f, 1f), RandomUniform(1f, 2f));

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.renderer.material = Resources.Load<Material>("Materials/Rock");
        sphere.transform.localScale = scale;

        yield return new Step<GameObject>(sphere);
    }

    protected override IEnumerable<Step<Generator>> GenerateChildren() {
        if (Level == 0) {
            yield break;
        }

        int boulders = RandomInteger(3, 10);

        for (int i = 0; i < boulders; i++) {
            var child = CreateChildren<RockGenerator>("Rock Fragment");

            child.transform.localScale = Vector3.one * RandomUniform(0.25f, 0.5f);
            
            var spheric = RandomSphere().normalized;
            
            child.transform.localRotation = Quaternion.FromToRotation(Vector3.up, spheric);

            spheric.x *= sphere.transform.localScale.x;
            spheric.y *= sphere.transform.localScale.y;
            spheric.z *= sphere.transform.localScale.z;

            child.transform.localPosition = spheric * 0.4f;
            child.OpenSize = OpenSize;
            child.Level = Level - 1;

            yield return new Step<Generator>(child);
        }

        //sphere.transform.localScale *= 0.5f;
    }

    protected override void PostChildrenClose() {
        //sphere.transform.localScale = scale;
    }
}
