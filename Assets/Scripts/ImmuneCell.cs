
public abstract class ImmuneCell : Unit
{
    protected override void Init(short maxHitPoints, short maxMovementPoints)
    {
        base.Init(maxHitPoints, maxMovementPoints);
        NewMethod();
    }

    private void NewMethod()
    {
        Start();
    }
}

