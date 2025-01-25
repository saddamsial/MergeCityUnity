using System.Collections.Generic;
using UnityEngine;

public class PositionPlaceholder : MonoBehaviour
{
    [SerializeField]
    protected int positionMaxCount;
    public int PositionMaxCount => positionMaxCount;
    [SerializeField]
    protected LineRenderer lineVisual;

    public List<Vector3> GetPositionPoints(int count)
    {
        var queueLineJoins = new List<Transform>();
        foreach (Transform join in transform)
        {
            queueLineJoins.Add(join);
        }
        int joinCount = queueLineJoins.Count;

        var queuePoints = new List<Vector3>();
        if (count > 0)
        {
            float queueDist = 0;
            for (int i = 0; i < joinCount - 1; i++)
            {
                var startJoin = queueLineJoins[i];
                var endJoin = queueLineJoins[i + 1];
                float dist = (endJoin.position - startJoin.position).magnitude;
                queueDist += dist;
            }
            float segmentDist = queueDist / (count - 1);

            float offset = 0;
            int queueCount = 0;
            for (int i = 0; i < joinCount - 1; i++)
            {
                var startJoin = queueLineJoins[i];
                var endJoin = queueLineJoins[i + 1];
                var dir = (endJoin.position - startJoin.position).normalized;
                float dist = (endJoin.position - startJoin.position).magnitude - offset;

                int segmentPointCount = (int)(dist / segmentDist) + 1;
                int j = 0;
                while (j < segmentPointCount
                    || (queueCount < count && i == joinCount - 2))
                {
                    var queuePoint = startJoin.position + (j * segmentDist + offset) * dir;
                    queueCount++;
                    j++;

                    queuePoints.Add(queuePoint);
                }
                offset = segmentPointCount * segmentDist - dist;
            }
        }

        return queuePoints;
    }
}
