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

    [Theory]
    [InlineData(ScriptPointerPath.Auto)]
    [InlineData(ScriptPointerPath.AvoidTarget)]
    public void MoveTo_TargetAwarePathAvoidsLabelUntilFinalApproach(ScriptPointerPath path)
    {
        var pointer = new VirtualPointer();
        var bounds = new ElementBox(200, 100, 160, 60);
        var target = new ElementPoint(280, 130);

        var points = pointer.MoveTo(target, 10, path: path, targetBounds: bounds).ToArray();

        Assert.Equal(target, points[^1]);
        Assert.All(points[..7], point => Assert.False(Inside(point, bounds)));
    }

    private static bool Inside(ElementPoint point, ElementBox bounds) =>
        point.X > bounds.X && point.X < bounds.X + bounds.Width &&
        point.Y > bounds.Y && point.Y < bounds.Y + bounds.Height;
}
