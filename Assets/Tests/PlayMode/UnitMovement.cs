using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UnitMovement
{
    [UnityTest]
    public IEnumerator UnitMovementWithEnumeratorPasses()
    {
        Bacteria unit = new Bacteria();
        yield return null;
    }
}
