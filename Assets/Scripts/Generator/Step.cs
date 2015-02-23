public struct Step<T> where T : class {
    public float Wait { get; private set; }

    public T Item { get; private set; }

    public Step(T item)
        : this() {
        Item = item;
        }

    public Step(float wait):this() {
        Wait = wait;
    }

    public bool HasItem { get { return Item != null; } }
}