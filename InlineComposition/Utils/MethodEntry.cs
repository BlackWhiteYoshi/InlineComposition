namespace InlineComposition;

/// <summary>
/// Represents the content of an inlined method (with head declarations and without closing bracket).
/// </summary>
public struct MethodEntry() {
    public readonly List<string> headList = [];
    public readonly List<string> blockList = [];
    public string? lastBlock = null;
}
