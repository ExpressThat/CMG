namespace CMG.Browser.Scripting.Recording;

public sealed class VirtualPointer
{
    public ElementPoint Position { get; private set; } = new(32, 32);

    public IEnumerable<ElementPoint> MoveTo(ElementPoint target, int frameCount, ScriptPointerEasing easing = ScriptPointerEasing.EaseInOut)
    {
        var start = Position;
        var frames = Math.Max(1, frameCount);

        for (var index = 1; index <= frames; index++)
        {
            var progress = (double)index / frames;
            var eased = Ease(progress, easing);
            var point = new ElementPoint(
                start.X + (target.X - start.X) * eased,
                start.Y + (target.Y - start.Y) * eased);

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
}
