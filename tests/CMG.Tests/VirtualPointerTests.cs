using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class VirtualPointerTests
{
    [Fact]
    public void MoveTo_DirectPathEndsAtTarget()
    {
        var pointer = new VirtualPointer();

        var points = pointer.MoveTo(new ElementPoint(132, 32), 4, path: ScriptPointerPath.Direct).ToArray();

        Assert.Equal(new ElementPoint(132, 32), points[^1]);
        Assert.All(points, point => Assert.Equal(32, point.Y));
    }

    [Fact]
    public void MoveTo_ArcPathBendsAwayFromLine()
    {
        var pointer = new VirtualPointer();

        var points = pointer.MoveTo(new ElementPoint(132, 32), 4, path: ScriptPointerPath.Arc).ToArray();

        Assert.Equal(new ElementPoint(132, 32), points[^1]);
        Assert.Contains(points[..^1], point => point.Y > 32);
    }

    [Fact]
    public void MoveTo_ManhattanPathTurnsAtRightAngle()
    {
        var pointer = new VirtualPointer();

        var points = pointer.MoveTo(new ElementPoint(132, 132), 4, path: ScriptPointerPath.Manhattan).ToArray();

        Assert.Equal(new ElementPoint(132, 132), points[^1]);
        Assert.Contains(points, point => point.Y == 32 && point.X > 32);
        Assert.Contains(points, point => point.X == 132 && point.Y > 32);
    }
}
