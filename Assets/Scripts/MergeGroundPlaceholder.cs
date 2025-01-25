using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MergeGroundPlaceholder : MonoBehaviour
{
    [SerializeField]
    protected string mergeBoardCodeName;
    public string MergeBoardCodeName => mergeBoardCodeName;
    [SerializeField]
    protected int maxRowCount;
    public int MaxRowCount => maxRowCount;
    [SerializeField]
    protected int maxColumnCount;
    public int MaxColumnCount => maxColumnCount;
    [SerializeField]
    protected LandDirection direction;
    public LandDirection Direction => direction;
    public float RotateAngle => (int)direction * 90;
    [SerializeField]
    protected List<Hole> buildingSpace;
    public List<Hole> BuildingSpace => buildingSpace;
    [SerializeField]
    protected Transform buildingPlaceholder;
    public Transform BuildingPlaceholder => buildingPlaceholder;
    [SerializeField]
    protected Transform inPoint;
    public Transform InPoint => inPoint;
    [SerializeField]
    protected Transform outPoint;
    public Transform OutPoint => outPoint;

    private void OnDrawGizmos()
    {
        // Visualize merge ground
        var groundOffset = new Vector3((maxColumnCount - 1) / 2f, 0, (maxRowCount - 1) / 2f);
        for (int r = 0; r < maxRowCount; r++)
        {
            for (int c = 0; c < maxColumnCount; c++)
            {
                if (buildingSpace.FindIndex(hole => hole.RowIndex == r && hole.ColumnIndex == c) >= 0)
                {
                    continue;
                }

                float xPos = (c - groundOffset.x) * MergeGround.BLOCK_WIDTH_GAP + transform.position.x;
                float zPos = -(r - groundOffset.z) * MergeGround.BLOCK_HEIGHT_GAP + transform.position.z;

                var position = new Vector3(xPos, 0, zPos);
                position = RotatePoint(position, transform.position);
                Gizmos.DrawWireCube(position, new Vector3(1, 0.05f, 1));
            }
        }

        if (buildingSpace.Count > 0)
        {
            var buildingPosition = MergeGround.CalculatePositionFromCoordGroup(
                buildingSpace.Select(item => new Hiker.Merge.Coord(item.RowIndex, item.ColumnIndex)).ToList(),
                groundOffset,
                transform.position);
            buildingPosition.y = 0.5f;
            buildingPlaceholder.position = RotatePoint(buildingPosition, transform.position);
        }
    }

    protected Vector3 RotatePoint(Vector3 point, Vector3 pivot)
    {
        var dir = point - pivot;
        dir = Quaternion.Euler(new Vector3(0, RotateAngle, 0)) * dir;
        point = dir + pivot;
        return point;
    }

    [System.Serializable]
    public class Hole : System.IEquatable<Hole>
    {
        public int RowIndex;
        public int ColumnIndex;

        public bool Equals(Hole other)
        {
            return other.RowIndex == RowIndex
                && other.ColumnIndex == ColumnIndex;
        }
    }
}
public enum LandDirection
{
    Forward,
    Right,
    Backward,
    Left,
}
