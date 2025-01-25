using System.Collections.Generic;
using UnityEngine;

public class ActionPlaceholder : PositionPlaceholder
{
    private void OnDrawGizmos()
    {
        var actionJoins = new List<Transform>();
        foreach (Transform join in transform)
        {
            actionJoins.Add(join);
        }
        int joinCount = actionJoins.Count;
        lineVisual.positionCount = joinCount;
        Gizmos.color = Color.red;
        for (int i = 0; i < joinCount; i++)
        {
            var join = actionJoins[i];
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
