using System.Collections.Generic;
using UnityEngine;

public class QueuePlaceholder : PositionPlaceholder
{
    private void OnDrawGizmos()
    {
        var queueLineJoins = new List<Transform>();
        foreach (Transform join in transform)
        {
            queueLineJoins.Add(join);
        }
        int joinCount = queueLineJoins.Count;
        lineVisual.positionCount = joinCount;
        Gizmos.color = Color.red;
        for (int i = 0; i < joinCount; i++)
        {
            var join = queueLineJoins[i];
            lineVisual.SetPosition(i, join.position);

            Gizmos.DrawSphere(join.position, 0.1f);
        }

        var queuePoints = GetPositionPoints(positionMaxCount);
        Gizmos.color = Color.green;
        foreach (var point in queuePoints)
        {
            Gizmos.DrawSphere(point, 0.05f);
        }
    }
}
