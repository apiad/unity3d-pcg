using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Rnd = System.Random;

public abstract class Generator : MonoBehaviour {
    public int Seed;
    public int UpdateStep = 10;
    public float OpenSize;

    public static int OpenNodes = 0;
    public static float StepsPerSecond = 0;

    private bool updating;
    private bool opened;
    private bool root = true;
    private Rnd rnd;
    private Rnd childRnd;

    public OpenMode OpenMode = OpenMode.BFS;

    private Coroutine updateCoroutine;

    private List<GameObject> repr = new List<GameObject>();
    private List<Generator> children = new List<Generator>();

    public int ChildrenCount { get { return children.Count; } }

    public T GetChildren<T>(int i) where T : Generator {
        return (T)this.children[i];
    }

    public IEnumerable<T> GetChildren<T>() where T : Generator {
        foreach (T child in children) {
            yield return child;
        }
    }

    private bool started;
    public int ExpectedFramerate = 50;
    private bool forceClose;

    public void Start() {
        PreStart();

        if (root) {
            StartCoroutine(DoUpdate());
        }
    }

    protected virtual void PreStart() {

    }

    private IEnumerable<Step<GameObject>> UpdateRepr() {
        this.rnd = new Rnd(this.Seed);

        foreach (var step in Open()) {
            if (step.HasItem) {
                var go = step.Item;

                var lp = go.transform.localPosition;
                var lr = go.transform.localRotation;
                var ls = go.transform.localScale;

                go.transform.parent = this.transform;

                go.transform.localPosition = lp;
                go.transform.localRotation = lr;
                go.transform.localScale = ls;

                this.repr.Add(go);
            }

            yield return step;
        }

        started = true;

        OpenNodes++;
    }

    public void Restart() {
        forceClose = true;
    }

    public virtual float ScreenSize {
        get {
            Vector3 max = Vector3.one * float.MinValue;
            Vector3 min = Vector3.one * float.MaxValue;
            Vector3 center = Vector3.zero;

            foreach (var item in this.repr) {
                foreach (var renderer in item.GetComponents<Renderer>()) {
                    max = Vector3.Max(max, renderer.bounds.max);
                    min = Vector3.Min(min, renderer.bounds.min);
                    center += renderer.bounds.center;
                }
            }

            center /= this.repr.Count;

            float l = Vector3.Distance(min, max);
            float d = Vector3.Distance(Camera.main.transform.position, center);
            float D = new Vector2(Screen.width, Screen.height).magnitude;

            return l * D / d;
        }
    }

    protected T CreateChildren<T>(string objectName) where T : Generator {
        var child = new GameObject(objectName).AddComponent<T>();
        child.Seed = this.childRnd.Next();
        child.transform.parent = this.transform;
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        return child;
    }

    private IEnumerable<Step<Generator>> OpenChildren() {
        this.childRnd = new Rnd(this.Seed);

        foreach (Step<Generator> step in GenerateChildren()) {
            if (step.HasItem) {
                var child = step.Item;
                child.root = false;
                this.children.Add(child);

                foreach (var s in child.UpdateRepr()) {
                    yield return new Step<Generator>(s.Wait);
                }
            }

            yield return step;
        }
    }

    private IEnumerable<Step<Generator>> OpenNeeded() {
        if (!started) {
            foreach (var step in UpdateRepr()) {
                yield return new Step<Generator>(step.Wait);
            }
        }

        switch (OpenMode) {
            case OpenMode.BFS:
                foreach (var step in OpenInBFS()) {
                    yield return step;
                }
                break;
            case OpenMode.DFS:
                foreach (var step in OpenInDFS()) {
                    yield return step;
                }
                break;
            case OpenMode.IDS:
                foreach (var step in OpenInIDS()) {
                    yield return step;
                }
                break;
        }
    }

    private IEnumerable<Step<Generator>> OpenInDFS() {
        yield return new Step<Generator>(this);

        if (!opened && NeedOpen()) {
            this.rnd = new Rnd(this.Seed);

            foreach (var step in OpenChildren()) {
                yield return step;
            }

            this.opened = true;
        }

        foreach (var c in this.children) {
            foreach (var step in c.OpenInDFS()) {
                yield return step;
            }
        }
    }

    private IEnumerable<Step<Generator>> OpenInIDS() {
        yield return new Step<Generator>(this);

        if (!opened && NeedOpen()) {
            this.rnd = new Rnd(this.Seed);

            foreach (var step in OpenChildren()) {
                yield return step;
            }

            this.opened = true;
            yield break;
        }

        this.children.Sort((x, y) => x.ScreenSize.CompareTo(y.ScreenSize));

        foreach (var c in this.children) {
            foreach (var step in c.OpenInIDS()) {
                yield return step;
            }
        }
    }

    private IEnumerable<Step<Generator>> OpenInBFS() {
        var queue = new Queue<Generator>();
        queue.Enqueue(this);

        yield return new Step<Generator>(this);

        while (queue.Count > 0) {
            var node = queue.Dequeue();

            if (!node.opened && node.NeedOpen()) {
                foreach (var step in node.OpenChildren()) {
                    yield return step;
                }

                node.opened = true;
            }

            foreach (var child in node.children) {
                queue.Enqueue(child);
                yield return new Step<Generator>(child);
            }
        }
    }

    protected void Close() {
        foreach (var go in repr) {
            //var fader = go.GetComponent<Fader>();

            //if (fader != null) {
            //    fader.FadeOutAndDestroy();
            //}
            //else {
                Destroy(go);
            //}
        }

        OpenNodes--;
        this.repr.Clear();
    }

    protected virtual IEnumerable<Step<GameObject>> Open() {
        yield break;
    }

    private bool CloseUnNeeded(bool force=false) {
        bool canClose = true;

        foreach (var c in this.children) {
            canClose &= c.CloseUnNeeded(force);
        }

        if (canClose && (NeedClose() || force)) {
            foreach (var c in children) {
                c.Close();
                Destroy(c.gameObject);
            }

            this.children.Clear();
            this.opened = false;

            this.PostChildrenClose();

            return true;
        }

        return false;
    }

    protected virtual void PostChildrenClose() {

    }

    protected IEnumerator DoUpdate() {
        this.updating = true;
        this.CloseUnNeeded(forceClose);

        forceClose = false;

        float lastUpdate = 0f;
        float lastSecond = 0f;
        int steps = 0;

        foreach (var step in OpenNeeded()) {
            if (step.HasItem) { 
                steps++; 
            }

            if (step.Wait > 0f) {
                yield return new WaitForSeconds(step.Wait);
            }

            if (Time.realtimeSinceStartup - lastUpdate > 1f / ExpectedFramerate) {
                yield return new WaitForEndOfFrame();

                UpdateStep += steps;

                if (Time.realtimeSinceStartup - lastSecond >= 0.25f) {
                    StepsPerSecond = StepsPerSecond * 0.5f + UpdateStep / (Time.realtimeSinceStartup - lastSecond) * 0.5f;
                    UpdateStep = 0;
                    lastSecond = Time.realtimeSinceStartup;
                }
                
                lastUpdate = Time.realtimeSinceStartup;
                steps = 0;
            }
        }

        this.updating = false;
        yield return new WaitForEndOfFrame();

        StartCoroutine(DoUpdate());
    }

    protected virtual IEnumerable<Step<Generator>> GenerateChildren() {
        yield break;
    }

    protected virtual bool NeedOpen() {
        if (OpenSize > 0) {
            return ScreenSize > OpenSize;
        }

        return false;
    }

    protected virtual bool NeedClose() {
        if (OpenSize > 0) {
            return ScreenSize < OpenSize * 0.95f;
        }

        return true;
    }

    protected bool RandomBernoulli(float p) {
        return this.rnd.NextDouble() <= p;
    }

    protected float RandomUniform(float a, float b) {
        return (float)this.rnd.NextDouble() * (b - a) + a;
    }

    protected int RandomInteger(int min, int max) {
        return this.rnd.Next(min, max);
    }

    public Vector3 RandomSphere() {
        while (true) {
            Vector3 r = new Vector3(RandomUniform(-1, 1), RandomUniform(-1, 1), RandomUniform(-1, 1));

            if (r.magnitude < 1) {
                return r;
            }
        }
    }

    protected IEnumerable<Step<Island[]>> GenerateIslands(int count, float size, float density, float separation) {
        List<Island> islands = new List<Island>();

        for (int i = 0; i < count; i++) {
            for (int j = 0; j < count; j++) {
                if (!RandomBernoulli(density)) {
                    yield return new Step<Island[]>();
                    continue;
                }

                float x = i * size + RandomUniform(0, size - size * separation) - size * count / 2;
                float z = j * size + RandomUniform(0, size - size * separation) - size * count / 2;

                var island = new Island(new Vector3(x, 0, z), size /10f);
                islands.Add(island);
                yield return new Step<Island[]>();
            }
        }

        bool[] cannotGrow = new bool[islands.Count];
        int canGrow = islands.Count;

        while (canGrow > 0) {
            if (islands.Count < 2) {
                break;
            }

            for (int i = 0; i < islands.Count; i++) {
                var child = islands[i];

                if (cannotGrow[i]) {
                    yield return new Step<Island[]>();
                    continue;
                }

                for (int j = 0; j < islands.Count; j++) {
                    if (i == j) {
                        yield return new Step<Island[]>();
                        continue;
                    }

                    var other = islands[j];

                    if (Vector3.Distance(child.Center, other.Center) - size * separation < (child.Radius + other.Radius)) {
                        cannotGrow[i] = true;
                        canGrow--;
                        yield return new Step<Island[]>();
                        break;
                    }

                    yield return new Step<Island[]>();
                }

                if (!cannotGrow[i]) {
                    child.Radius += size * 0.01f;
                }
            }
        }

        yield return new Step<Island[]>(islands.ToArray());
    }
}
