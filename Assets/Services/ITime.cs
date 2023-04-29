public interface ITime
{
    public float SetScale(float timeScale);
}

public class GameTime : ITime
{
    public float SetScale(float timeScale)
    {
        return timeScale;
    }
}