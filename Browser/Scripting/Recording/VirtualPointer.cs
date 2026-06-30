namespace CMG.Browser.Scripting.Recording;

public sealed class VirtualPointer
{
    public ElementPoint Position { get; private set; } = new(32, 32);

    public IEnumerable<ElementPoint> MoveTo(
        ElementPoint target,
        int frameCount,
        ScriptPointerEasing easing = ScriptPointerEasing.EaseInOut,
        ScriptPointerPath path = ScriptPointerPath.Direct)
    {
        var start = Position;
        var frames = Math.Max(1, frameCount);

        for (var index = 1; index <= frames; index++)
        {
            var progress = (double)index / frames;
            var eased = Ease(progress, easing);
            var point = PathPoint(start, target, eased, path);

            Position = point;
            yield return point;
        }
    }

    public void Set(ElementPoint point)
    {
        Position = point;
    }

    private static double Ease(double value, ScriptPointerEasing easing) =>
        easing switch
        {
            ScriptPointerEasing.Linear => value,
            ScriptPointerEasing.EaseIn => value * value,
            ScriptPointerEasing.EaseOut => 1 - Math.Pow(1 - value, 2),
            ScriptPointerEasing.Spring => Spring(value),
            _ => EaseInOut(value)
        };

    private static double EaseInOut(double value)
    {
        return value < 0.5
            ? 2 * value * value
            : 1 - Math.Pow(-2 * value + 2, 2) / 2;
    }

    private static double Spring(double value)
    {
        var eased = 1 - Math.Cos(value * Math.PI * 4) * Math.Exp(-value * 6);
        return Math.Clamp(eased, 0, 1);
    }

    private static ElementPoint PathPoint(ElementPoint start, ElementPoint target, double progress, ScriptPointerPath path) =>
        path switch
        {
            ScriptPointerPath.Arc or ScriptPointerPath.AvoidCenter => ArcPoint(start, target, progress),
            ScriptPointerPath.Manhattan or ScriptPointerPath.AvoidTarget => ManhattanPoint(start, target, progress),
            _ => Lerp(start, target, progress)
        };

    private static ElementPoint Lerp(ElementPoint start, ElementPoint target, double progress) =>
        new(start.X + (target.X - start.X) * progress, start.Y + (target.Y - start.Y) * progress);

    private static ElementPoint ArcPoint(ElementPoint start, ElementPoint target, double progress)
    {
        var direct = Lerp(start, target, progress);
        var dx = target.X - start.X;
        var dy = target.Y - start.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance <= 0.001)
        {
            return target;
        }

        var lift = Math.Min(96, Math.Max(24, distance * 0.18)) * Math.Sin(progress * Math.PI);
        return new(direct.X + (-dy / distance) * lift, direct.Y + (dx / distance) * lift);
    }

    private static ElementPoint ManhattanPoint(ElementPoint start, ElementPoint target, double progress)
    {
        if (progress < 0.5)
        {
            return new(start.X + (target.X - start.X) * (progress * 2), start.Y);
        }

        return new(target.X, start.Y + (target.Y - start.Y) * ((progress - 0.5) * 2));
    }
}
