using UnityEngine;
using System.Collections.Generic;

public class TreeGenerator : Generator {
    private float height;
    private float width;
    private float bush;
    private GameObject head;

    public int Level = 2;

    public Color BaseColor = new Color32(42, 107, 5, 255);
    public float ColorVariation = 0.1f;

    public int BranchSteps = 5;
    public int MinBranchingGuide = 3;
    public float AngleVariation = 45f;
    public float MinHeight = 2f;
    public float MaxHeight = 5f;
    public int MinBranches = 5;
    public int MaxBranches = 10;

    public float LeavesDensity = 1f;

    public Material BarkMaterial;
    public Material LeavesMaterial;

    private List<GameObject> guides = new List<GameObject>();

    protected override IEnumerable<Step<GameObject>> Open() {
        height = RandomUniform(MinHeight, MaxHeight);
        width = RandomUniform(0.25f, 0.5f);
        bush = RandomUniform(5f, 10f);

        guides.Clear();

        var basing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        basing.transform.localScale = Vector3.one * width * 1.5f;
        basing.renderer.sharedMaterial = BarkMaterial;

        yield return new Step<GameObject>(basing);

        var guideRoot = new GameObject("Guide");

        guides.Add(guideRoot);

        yield return new Step<GameObject>(guideRoot);

        GameObject lastGuide = guideRoot;

        for (int i = 0; i < BranchSteps; i++) {
            float angle = RandomUniform(-AngleVariation, AngleVariation);
            float baseAngle = RandomUniform(0, 360);

            lastGuide.transform.Rotate(0, baseAngle, angle);
            lastGuide.transform.localScale = Vector3.one;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.renderer.sharedMaterial = BarkMaterial;
            body.transform.parent = lastGuide.transform;
            body.transform.localScale = new Vector3(width, height / 2 / BranchSteps, width);
            body.transform.localPosition = Vector3.up * height / 2 / BranchSteps;
            body.transform.localRotation = Quaternion.identity;

            yield return new Step<GameObject>();

            var top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            top.renderer.sharedMaterial = BarkMaterial;
            top.transform.parent = lastGuide.transform;
            top.transform.localScale = Vector3.one * width;
            top.transform.localPosition = Vector3.up * height / BranchSteps;
            top.transform.localRotation = Quaternion.identity;

            yield return new Step<GameObject>();

            var newGuide = new GameObject("Guide");
            newGuide.transform.parent = lastGuide.transform;
            newGuide.transform.localPosition = top.transform.localPosition;
            newGuide.transform.localRotation = top.transform.localRotation;

            yield return new Step<GameObject>();

            guides.Add(newGuide);

            lastGuide = newGuide;
        }

        if (RandomBernoulli(LeavesDensity)) {
            head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.renderer.sharedMaterial = LeavesMaterial;
            //head.renderer.material.color = (BaseColor + new Color(RandomUniform(-1, 1) * ColorVariation, RandomUniform(-1, 1) * ColorVariation, RandomUniform(-1, 1) * ColorVariation));
            head.transform.localScale = Vector3.one * width * bush;
            head.transform.localPosition = this.transform.InverseTransformPoint(lastGuide.transform.position);

            yield return new Step<GameObject>(head);

            head.transform.rotation = Quaternion.identity;
        }
    }

    protected override void PostChildrenClose() {
        if (head != null) head.SetActive(true);
    }

    protected override IEnumerable<Step<Generator>> GenerateChildren() {
        if (Level == 0) {
            yield break;
        }

        int branches = RandomInteger(MinBranches, MaxBranches);

        Vector3 right = this.transform.TransformDirection(Vector3.right);
        Vector3 up = this.transform.TransformDirection(Vector3.up);

        for (int i = 0; i < branches; i++) {
            var child = CreateChildren<TreeGenerator>("Branch");
            float childScale = RandomUniform(0.25f, 0.5f);
            //child.transform.localPosition = Vector3.up * RandomUniform(0.5f, 1f) * height;
            int guideIndex = RandomInteger(MinBranchingGuide, BranchSteps + 1);
            child.transform.localPosition = transform.InverseTransformPoint(guides[guideIndex].transform.position);
            child.transform.Rotate(right, RandomUniform(30f, 90f), Space.World);
            child.transform.Rotate(up, RandomUniform(0f, 360f), Space.World);
            child.transform.localScale = Vector3.one * childScale;
            child.transform.Translate(Vector3.up * width / 2f * this.transform.localScale.x * childScale);
            child.OpenSize = OpenSize;
            child.Level = Level - 1;
            child.BaseColor = BaseColor;
            child.ColorVariation = ColorVariation;
            child.BranchSteps = BranchSteps;
            child.AngleVariation = AngleVariation;
            child.LeavesDensity = LeavesDensity;
            child.BarkMaterial = BarkMaterial;
            child.LeavesMaterial = LeavesMaterial;

            yield return new Step<Generator>(child);
        }

        if (head != null) head.SetActive(false);
    }
}
