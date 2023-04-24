using System.Collections;
using UnityEngine.TestTools;

public class PlayerMovement
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator UnitMoves_TargetTileIsReachable_UnitMovesToTile()
    {
        Bacteria bacteria = new Bacteria();

        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
