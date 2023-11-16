namespace InlineComposition;

/// <summary>
/// Represents the content of an inlined method (with head declarations and without closing bracket).
/// </summary>
internal struct MethodEntry() {
    public List<string> headList = [];
    public List<string> blockList = [];
    public string? lastBlock = null;
}
