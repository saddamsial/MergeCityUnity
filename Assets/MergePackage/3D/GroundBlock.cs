using UnityEngine;

public class GroundBlock : MonoBehaviour
{
    protected Data data;
    public int RowIndex => data.RowIndex;
    public int ColumnIndex => data.ColumnIndex;

    public void Init(Data data)
    {
        this.data = data;
    }

    public void UpdatePosition(Vector3 position)
    {
        transform.position = position;
    }

    public class Data
    {
        public int RowIndex;
        public int ColumnIndex;
    }
}
