using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

[CreateAssetMenu(fileName = "Prefab Brush", menuName = "Brushes/Prefab Brush")]
[CustomGridBrush(false, true, false, "Prefab Brush")]
public class PrefabBrush : GameObjectBrush
{
}
