namespace CMG.Browser.Scripting.Recording;

public sealed class VirtualPointer
{
    public ElementPoint Position { get; private set; } = new(32, 32);

    public IEnumerable<ElementPoint> MoveTo(
        ElementPoint target,
        int frameCount,
        ScriptPointerEasing easing = ScriptPointerEasing.EaseInOut,
        ScriptPointerPath path = ScriptPointerPath.Auto,
        ElementBox? targetBounds = null)
    {
        var start = Position;
        var frames = Math.Max(1, frameCount);

        for (var index = 1; index <= frames; index++)
        {
            var progress = (double)index / frames;
            var eased = Ease(progress, easing);
            var point = index == frames
                ? target
                : PathPoint(start, target, eased, ResolvePath(path, targetBounds), targetBounds);

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

    private static ScriptPointerPath ResolvePath(ScriptPointerPath path, ElementBox? bounds) =>
        path is not ScriptPointerPath.Auto ? path :
        bounds is { Width: >= 48, Height: >= 20 } ? ScriptPointerPath.AvoidTarget : ScriptPointerPath.Arc;

    private static ElementPoint PathPoint(ElementPoint start, ElementPoint target, double progress, ScriptPointerPath path, ElementBox? bounds) =>
        path switch
        {
            ScriptPointerPath.Arc or ScriptPointerPath.AvoidCenter => ArcPoint(start, target, progress),
            ScriptPointerPath.AvoidTarget when bounds is not null => AvoidTargetPoint(start, target, progress, bounds),
            ScriptPointerPath.Manhattan => ManhattanPoint(start, target, progress),
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

    private static ElementPoint AvoidTargetPoint(ElementPoint start, ElementPoint target, double progress, ElementBox bounds)
    {
        const double margin = 16;
        var candidates = new[]
        {
            new ElementPoint(bounds.X - margin, target.Y),
            new ElementPoint(bounds.X + bounds.Width + margin, target.Y),
            new ElementPoint(target.X, bounds.Y - margin),
            new ElementPoint(target.X, bounds.Y + bounds.Height + margin)
        };
        var approach = candidates.MinBy(point => DistanceSquared(start, point))!;
        const double approachAt = 0.9;
        return progress < approachAt
            ? ArcPoint(start, approach, progress / approachAt)
            : Lerp(approach, target, (progress - approachAt) / (1 - approachAt));
    }

    private static double DistanceSquared(ElementPoint left, ElementPoint right) =>
        Math.Pow(left.X - right.X, 2) + Math.Pow(left.Y - right.Y, 2);
}
