namespace InlineComposition;

/// <summary>
/// Represents the content of an inlined method (with head declarations and without closing bracket).
/// </summary>
internal struct MethodEntry {
    public List<string> headList = new();
    public List<string> blockList = new();
    public string lastBlock = string.Empty;

    public MethodEntry() { }
}
