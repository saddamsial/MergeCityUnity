using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Merge
{

    public class MergeUtility
    {
        public static string GetUniqueID()
        {
            return System.Guid.NewGuid().ToString();
        }

        public static List<Coord> GetCoordsFromMatrix(Coord coord, List<List<int>> matrix)
        {
            var coords = new List<Coord>();
            int rowCount = matrix.Count;
            for (int i = 0; i < rowCount; i++)
            {
                int columnCount = matrix[i].Count;
                for (int j = 0; j < columnCount; j++)
                {
                    var flag = matrix[i][j];
                    if (flag != 0)
                    {
                        coords.Add(new Coord()
                        {
                            RowIndex = coord.RowIndex + i,
                            ColumnIndex = coord.ColumnIndex + j,
                        });
                    }
                }
            }
            return coords;
        }

    }

    [System.Serializable]
    public class Slot
    {
        public int RowIndex { get; protected set; }
        public int ColumnIndex { get; protected set; }
        public string OccupierID { get; protected set; } = "";
        public string LockOccupierID { get; protected set; } = "";

        public void Fill(string id)
        {
            OccupierID = id;
        }

        public string Empty()
        {
            var lastOccupierID = OccupierID;
            OccupierID = "";
            return lastOccupierID;
        }

        public void Lock(string id)
        {
            LockOccupierID = id;
        }

        public string Unlock()
        {
            var lastLockerID = LockOccupierID;
            LockOccupierID = "";
            return lastLockerID;
        }

        public void ParseFromCoord(Coord coord)
        {
            RowIndex = coord.RowIndex;
            ColumnIndex = coord.ColumnIndex;
        }

        public bool IsOccupied
        {
            get { return !string.IsNullOrEmpty(OccupierID); }
        }

        public bool IsLock
        {
            get { return !string.IsNullOrEmpty(LockOccupierID); }
        }

        public bool IsLockBy(string lockerID)
        {
            return string.Equals(LockOccupierID, lockerID);
        }
    }

    [System.Serializable]
    public class Coord : System.IEquatable<Coord>
    {
        public int RowIndex;
        public int ColumnIndex;

        public Coord() { }

        public Coord(int rowIndex, int columnIndex)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
        }

        public override string ToString()
        {
            return RowIndex + "_" + ColumnIndex;
        }

        public static Coord GetCoordFromKey(string key)
        {
            var parts = key.Split("_");
            return new Coord()
            {
                RowIndex = int.Parse(parts[0]),
                ColumnIndex = int.Parse(parts[1]),
            };
        }

        public static List<Coord> AddCoords(List<Coord> coords, Vector2Int modifier)
        {
            var result = new List<Coord>();
            foreach (var coord in coords)
            {
                result.Add(new Coord(coord.RowIndex + modifier.y, coord.ColumnIndex + modifier.x));
            }
            return result;
        }

        public bool Equals(Coord other)
        {
            return RowIndex == other.RowIndex
                && ColumnIndex == other.ColumnIndex;
        }
    }
}
