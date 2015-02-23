using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : Generator
{
    public int Samples = 101;
    public float Width = 2000f;
    public float Density = 0.1f;
    public float Separation = -0.2f;
    public float Height = 1f;

    public int Sublevels = 10;
    private int sublevel = 0;

    public int Islands = 10;
    public int Level = 0;
    public int PerlinLevels = 4;
    public float PerlinInfluence = 0.5f;

    public int MinRocks = 20;
    public int MaxRocks = 100;
    public float RocksScale = 1f;

    public int MinTrees = 10;
    public int MaxTrees = 20;

    public TerrainGenerator Parent;
    public Vector2 ParentOffset;

    public float SeaLevel;

    public float MaxRocksWidth = 512f;
    public float TreesWidth = 512f;

    private GameObject plane;

    protected override void PreStart() {
        if (Parent == null) {
            float maxPerlin = 0f;
            float width = Width / 10f;

            for (int i = 0; i < PerlinLevels; i++) {
                maxPerlin += width / 2;
                width /= 10;
            }

            sublevel = Sublevels;

            SeaLevel = maxPerlin * PerlinInfluence;
        }
        else {
            SeaLevel = Parent.SeaLevel;
        }
    }

    private Island[] islands;
    private float[] heights;

    private float SampleHeight(float x, float y, Island island, float height) {
        float d = Vector3.Distance(new Vector3(x, 0, y), island.Center);
        float mountain = Mathf.Exp(-d * d / (island.Radius * island.Radius)) * height;

        return mountain;
    }

    private float SampleGaussian(float x, float mu, float sigma) {
        float d = (x - mu);
        return Mathf.Exp(-d * d / (sigma * sigma));
    }

    private float SamplePerlin(float x, float y) {
        float width = Width / 10f;
        float result = 0;

        x += Width;
        y += Width;

        for (int i = 0; i < PerlinLevels; i++) {
            result += (Mathf.PerlinNoise(x / width, y / width)) * width / 2;
            width /= 10;
        }

        return result;
    }

    public float SampleHeight(float x, float y) {
        if (!Ready) {
            return 0f;
        }

        if (Parent != null) {
            return Parent.SampleHeight(x + ParentOffset.x, y + ParentOffset.y);
        }

        return SampleGaussians(x, y) + PerlinInfluence * SamplePerlin(x, y) - SeaLevel;
    }

    private float SampleGaussians(float x, float y) {
        float h = 0;

        for (int i = 0; i < islands.Length; i++) {
            h += SampleHeight(x, y, islands[i], heights[i]);
        }

        return h;
    }

    protected override IEnumerable<Step<GameObject>> Open() {
        if (Parent == null || sublevel == 0) {
            foreach (var step in GenerateIslands(Islands, Width / (2 * Islands), Density, Separation)) {
                if (step.HasItem) {
                    islands = step.Item;
                }

                yield return new Step<GameObject>();
            }

            heights = new float[islands.Length];

            for (int i = 0; i < heights.Length; i++) {
                heights[i] = RandomUniform(0, 1) * islands[i].Radius * Height;
            }
        }

        Ready = true;

        var displace = new Vector3(1, 0, 1) * (-Width / 2);

        // Instantiate the empty mesh
        var mesh = new Mesh();

        // Create the vertices
        var vertices = new Vector3[Samples * Samples];
        var normals = new Vector3[Samples * Samples];
        var uvs = new Vector2[Samples * Samples];

        // Fill in the vertices
        float edge = (Width * 1.01f) / (Samples - 1);

        for (int i = 0, p = 0; i < Samples; i++) {
            for (int j = 0; j < Samples; j++) {
                var center = new Vector3(i * edge, 0, j * edge) + displace;
                float h = SampleHeight(center.x, center.z);

                if (i == 0 || j == 0 || i == Samples - 1 || j == Samples - 1) {
                    h -= 0.002f * Width;
                }

                center.y = h;
                vertices[p] = center;
                uvs[p] = new Vector2(i * 1f / (Samples - 1), j * 1f / (Samples - 1));
                normals[p] = SampleNormal(center.x, center.z);
                p++;
            }

            yield return new Step<GameObject>();
        }

        // Create the triangles
        int trigsCount = (Samples - 1) * (Samples - 1);
        var triangles = new int[3 * 2 * trigsCount];

        for (int t = 0; t < trigsCount; t++) {
            int i = t / (Samples - 1);
            int j = t % (Samples - 1);

            triangles[6 * t + 0] = CalculateIndex(i + 1, j);
            triangles[6 * t + 1] = CalculateIndex(i, j);
            triangles[6 * t + 2] = CalculateIndex(i, j + 1);
            triangles[6 * t + 3] = CalculateIndex(i + 1, j);
            triangles[6 * t + 4] = CalculateIndex(i, j + 1);
            triangles[6 * t + 5] = CalculateIndex(i + 1, j + 1);

            if (j == 0) {
                yield return new Step<GameObject>();
            }
        }

        // Set the mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        int textureSize = 16;
        float width = Width *1.01f;
        float texel = width / (textureSize - 1);

        var texture = new Texture2D(textureSize, textureSize);
        texture.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color[textureSize * textureSize];

        for (int i = 0; i < textureSize; i++) {
            for (int j = 0; j < textureSize; j++) {
                Vector3 location = new Vector3(j * texel, 0, i * texel) + displace;
                location.y = SampleHeight(location.x, location.z);
                pixels[i * textureSize + j] = SampleColor(location);

                yield return new Step<GameObject>();
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        plane = new GameObject("Terrain Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        plane.GetComponent<MeshFilter>().mesh = mesh;
        plane.AddComponent<MeshCollider>();

        plane.renderer.material.shader = Shader.Find("Diffuse");
        plane.renderer.material.mainTexture = texture;

        plane.renderer.castShadows = false;

        //plane.AddComponent<WaterAnimator>().Terrain = this;

        yield return new Step<GameObject>(plane);
    }

    private float HeightProportion(float min, float max, float h) {
        float mid = (max + min) / 2f;

        if (h < min || h > max) {
            return 0;
        }

        if (h < mid) {
            return (h - min) / (mid - min);
        }
        
        return (max - h) / (max - mid);
    }

    private Color SampleColor(Vector3 location) {
        Vector3 sand = new Vector3(239, 228, 176);
        Vector3 grass = new Vector3(29, 143, 12);
        Vector3 snow = new Vector3(221, 243, 249);

        float height = location.y + Mathf.PerlinNoise(location.x / 100f, location.y / 100f) * 100f - 50f;

        float sandStrength = HeightProportion(-1000, 150, height);
        float grassStrength = HeightProportion(100, 1500, height);
        float snowStrength = HeightProportion(1250, 5000, height);

        Vector3 mix = (sand * sandStrength + grass * grassStrength + snow * snowStrength) /
            (sandStrength + grassStrength + snowStrength);

        return new Color(mix.x / 255f, mix.y / 255f, mix.z / 255f);
    }

    public Vector3 SampleNormal(float x, float y, float resolution) {
        Vector3 p1 = SamplePoint(x, y);
        Vector3 p2 = SamplePoint(x, y - resolution);
        Vector3 p3 = SamplePoint(x + resolution, y);

        Plane p = new Plane(p1, p3, p2);

        return p.normal;
    }

    public Vector3 SampleNormal(float x, float y) {
        return SampleNormal(x, y, Width / Samples);
    }

    public Vector3 SamplePoint(float x, float y) {
        if (Parent != null) {
            return Parent.SamplePoint(x + ParentOffset.x, y + ParentOffset.y);
        }

        return new Vector3(x, SampleHeight(x, y), y);
    }

    public Vector3 SamplePoint(Vector3 location) {
        return SamplePoint(location.x, location.z);
    }

    protected override void PostChildrenClose() {
        plane.SetActive(true);
    }

    protected override IEnumerable<Step<Generator>> GenerateChildren() {
        if (Level == 0) yield break;

        yield return CreateChildrenTerrain(new Vector2(0.5f, 0.5f) * Width / 2);
        yield return CreateChildrenTerrain(new Vector2(-0.5f, 0.5f) * Width / 2);
        yield return CreateChildrenTerrain(new Vector2(0.5f, -0.5f) * Width / 2);
        yield return CreateChildrenTerrain(new Vector2(-0.5f, -0.5f) * Width / 2);

        if (Width <= MaxRocksWidth) {
            foreach (var step in GenerateRocks(RocksScale * Width / MaxRocksWidth)) {
                yield return step;
            }
        }

        if (LevelWidth(TreesWidth)) {
            int trees = RandomInteger(MinTrees, MaxTrees);

            for (int i = 0; i < trees; i++) {
                var child = GenerateChildrenInTerrain<TreeGenerator>("Tree");
                child.OpenMode = OpenMode.IDS;
                child.OpenSize = 100f;
                child.BarkMaterial = Resources.Load<Material>("Materials/TreeBark");
                child.LeavesMaterial = Resources.Load<Material>("Materials/TreeFolliage");

                if (child.transform.position.y > 0) {
                    yield return new Step<Generator>(child);
                }
                else {
                    Destroy(child.gameObject);
                }
            }
        }

        plane.SetActive(false);
    }

    private bool LevelWidth(float width) {
        return Width <= width && Width > width / 2f;
    }

    private IEnumerable<Step<Generator>> GenerateRocks(float scale) {
        int rocks = RandomInteger(MinRocks, MaxRocks);

        for (int i = 0; i < rocks; i++) {
            var child = GenerateChildrenInTerrain<RockGenerator>("Rock");
            child.transform.localScale = Vector3.one * scale;
            child.transform.localRotation = Quaternion.FromToRotation(Vector3.up, SampleNormal(child.transform.localPosition));
            child.OpenSize = OpenSize / 10f;

            yield return new Step<Generator>(child);
        }
    }

    private T GenerateChildrenInTerrain<T>(string objectName) where T: Generator {
        var child = CreateChildren<T>(objectName);
        child.transform.position = SamplePoint(Vector3.left * RandomUniform(-1, 1) * Width / 2 + Vector3.forward * RandomUniform(-1, 1) * Width / 2);

        return child;
    }

    //protected override bool NeedOpen() {
    //    return PlayerDistance > OpenDistance;
    //}

    //protected override bool NeedClose() {
    //    return PlayerDistance < OpenDistance / 2;
    //}

    private Step<Generator> CreateChildrenTerrain(Vector2 offset) {
        var child = CreateChildren<TerrainGenerator>("Terrain Patch");
        child.Parent = this;
        child.Width = Width / 2;
        child.Level = Level - 1;
        child.OpenSize = OpenSize;
        child.ParentOffset = offset;
        child.Samples = Samples;
        child.MaxRocksWidth = MaxRocksWidth;
        child.TreesWidth = TreesWidth;
        child.RocksScale = RocksScale;
        child.MinRocks = MinRocks;
        child.MaxRocks = MaxRocks;
        child.MinTrees = MinTrees;
        child.MaxTrees = MaxTrees;
        child.Sublevels = Sublevels;
        child.sublevel = sublevel == 0 ? Sublevels : sublevel - 1;
        child.transform.localPosition = new Vector3(offset.x, 0, offset.y);

        return new Step<Generator>(child);
    }

    private int CalculateIndex(int i, int j) {
        return i * Samples + j;
    }

    public bool Ready { get; private set; }

    public Vector3 SampleNormal(Vector3 location, float resolution) {
        return SampleNormal(location.x, location.z, resolution);
    }

    public Vector3 SampleNormal(Vector3 location) {
        return SampleNormal(location, Width / Samples);
    }
}
