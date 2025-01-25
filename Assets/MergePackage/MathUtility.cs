using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hiker.Merge
{
    public class MathUtility
    {
        public static int GetMaxRange(List<int> data)
        {
            if (data.Count == 0)
            {
                return 0;
            }

            var sorted = data.Select(value => value).OrderBy(value => value).ToList();
            return sorted[^1] - sorted[0];
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

        public static bool CheckIfCoordContains(List<Coord> parent, List<Coord> child)
        {
            foreach (var checkCoord in child)
            {
                if (parent.Find(coord =>
                coord.RowIndex == checkCoord.RowIndex && coord.ColumnIndex == checkCoord.ColumnIndex) != null)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public static bool CheckIfMatricesMatched(List<List<int>> matrixA, List<List<int>> matrixB)
        {
            int aRowCount = matrixA.Count;
            int bRowCount = matrixB.Count;
            if (aRowCount != bRowCount) { return false; }

            for (int r = 0; r < aRowCount; r++)
            {
                int aColumnCount = matrixA[r].Count;
                int bColumnCount = matrixB[r].Count;
                if (aColumnCount != bColumnCount) { return false; }

                for (int c = 0; c < aColumnCount; c++)
                {
                    if (matrixA[r][c] != matrixB[r][c])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static Vector2 ConvertWorldToCanvasPosition(RectTransform canvasRect, Camera camera, Vector3 worldPos)
        {
            var canvasPos = camera.WorldToViewportPoint(worldPos);
            canvasPos.x *= canvasRect.sizeDelta.x;
            canvasPos.y *= canvasRect.sizeDelta.y;
            canvasPos.x -= canvasRect.sizeDelta.x * canvasRect.pivot.x;
            canvasPos.y -= canvasRect.sizeDelta.y * canvasRect.pivot.y;
            return canvasPos;
        }

        public static bool GetCoordDiff(List<Coord> fromCoords, List<Coord> toCoords, out Vector2Int diff)
        {
            diff = new Vector2Int(0, 0);
            if (fromCoords.Count == 0 || fromCoords.Count != toCoords.Count)
            {
                return false;
            }

            var sortedFromCoords = fromCoords.OrderBy(coord => coord.RowIndex).OrderBy(coord => coord.ColumnIndex).ToList();
            var sortedToCoords = toCoords.OrderBy(coord => coord.RowIndex).OrderBy(coord => coord.ColumnIndex).ToList();
            diff = new Vector2Int(sortedToCoords[0].ColumnIndex - sortedFromCoords[0].ColumnIndex, sortedToCoords[0].RowIndex - sortedFromCoords[0].RowIndex);
            for (int i = 1; i < sortedFromCoords.Count; i++)
            {
                var rowDiff = sortedToCoords[i].RowIndex - sortedFromCoords[i].RowIndex;
                var columnDiff = sortedToCoords[i].ColumnIndex - sortedFromCoords[i].ColumnIndex;
                if (rowDiff != diff.y || columnDiff != diff.x)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckProbability(float chanceValue)
        {
            var randomValue = Random.Range(0f, 1f);
            if (randomValue < chanceValue)
            {
                return true;
            }

            return false;
        }
    }
}
