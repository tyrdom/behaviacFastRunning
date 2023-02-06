
public class FrameRandom
{
    private static Random ARandom { get; } = new();
    public static ushort Random(uint node45RandomMaxNum)
    {
        var next = ARandom.Next((int) node45RandomMaxNum);
        return (ushort) next;
    }
}