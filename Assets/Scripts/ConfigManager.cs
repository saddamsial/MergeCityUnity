using Hiker;
using Hiker.Merge;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConfigManager
{
    public static ConfigManager Instance { get; protected set; }

    public OtherConfig OtherConfig { get; protected set; }
    public UnitSpawnerConfig UnitSpawnerConfig { get; protected set; }

    public SpawnConfig SpawnConfig { get; protected set; }

    public EnergyConfig EnergyConfig { get; protected set; }

    private Dictionary<string, ItemConfig> itemsConfig = new Dictionary<string, ItemConfig>();
    protected Dictionary<string, UnitConfig> unitsConfig = new Dictionary<string, UnitConfig>();
    private Dictionary<string, BuildingConfig> buildingsConfig = new Dictionary<string, BuildingConfig>();
    public Dictionary<string, ItemConfig> ItemsConfig { get => itemsConfig; set => itemsConfig = value; }
    public Dictionary<string, BuildingConfig> BuildingsConfig { get => buildingsConfig; set => buildingsConfig = value; }

    public static void Init()
    {
        Instance = new ConfigManager();
        Instance.Load();
    }

    protected void Load()
    {
        var otherConfigResource = Resources.Load("Configs/OtherConfig") as TextAsset;
        string otherConfigContent = otherConfigResource.text;
        OtherConfig = JsonMapper.ToObject<OtherConfig>(otherConfigContent);

        var unitSpawnerConfigResource = Resources.Load("Configs/UnitSpawnerConfig") as TextAsset;
        string unitSpawnerConfigContent = unitSpawnerConfigResource.text;
        UnitSpawnerConfig = JsonMapper.ToObject<UnitSpawnerConfig>(unitSpawnerConfigContent);
        UnitSpawnerConfig.Init();

        var itemsConfigResource = Resources.Load("Configs/ItemsConfig") as TextAsset;
        string itemsConfigContent = itemsConfigResource.text;
        itemsConfig = GenericConfigTypeHelpers<ItemConfig>.ReadDictionaryConfig(itemsConfigContent);
        foreach (var itemConfig in itemsConfig.Values)
        {
            itemConfig.Init();
        }

        var unitsConfigResource = Resources.Load("Configs/UnitsConfig") as TextAsset;
        string unitsConfigContent = unitsConfigResource.text;
        unitsConfig = GenericConfigTypeHelpers<UnitConfig>.ReadDictionaryConfig(unitsConfigContent);

        var buildingsConfigResource = Resources.Load("Configs/BuildingsConfig") as TextAsset;
        string buildingsConfigContent = buildingsConfigResource.text;
        buildingsConfig = GenericConfigTypeHelpers<BuildingConfig>.ReadDictionaryConfig(buildingsConfigContent);
        foreach (var buildingConfig in buildingsConfig.Values)
        {
            buildingConfig.Init();
        }
        var spawnConfigResource = Resources.Load("Configs/SpawnerConfig") as TextAsset;
        string spawnConfigContent = spawnConfigResource.text;
        SpawnConfig = JsonMapper.ToObject<SpawnConfig>(spawnConfigContent);

        var energyConfigResource = Resources.Load("Configs/EnergyConfig") as TextAsset;
        string energyConfigContent = energyConfigResource.text;
        EnergyConfig = JsonMapper.ToObject<EnergyConfig>(energyConfigContent);
        Debug.Log("init energy :" + EnergyConfig.BaseEnergy);
    }

    public ItemConfig GetItemConfig(string codeName)
    {
        if (itemsConfig.TryGetValue(codeName,out var itemConfig))
        {
            return itemConfig;
        }

        return null;
    }
    public List<ItemConfig> GetItemConfigByBuildingName(string buildingName)
    {
        List<ItemConfig> itemConfigs = new List<ItemConfig>();
        foreach (var item in itemsConfig)
        {
            if (item.Value.BuildingCodeName == buildingName)
            {
                itemConfigs.Add(item.Value);
                return itemConfigs;
            }
        }

        return null; // Return null if no item matches the condition
    }
    public BuildingConfig GetBuildingConfig(string codeName)
    {
        if (buildingsConfig.TryGetValue(codeName, out var buildingConfig))
        {
            return buildingConfig;
        }

        return null;
    }

    public UnitConfig GetUnitConfig(string codeName)
    {
        if (unitsConfig.TryGetValue(codeName, out var unitConfig))
        {
            return unitConfig;
        }

        return null;
    }

  

    public static class GenericConfigTypeHelpers<T> where T : BaseConfig
    {
        /// <summary>
        /// Convenient function to read dictionaries of [codename - config] pairs.
        /// </summary>
        public static Dictionary<string, T> ReadDictionaryConfig(string content)
        {
            Dictionary<string, T> result = JsonMapper.ToObject<Dictionary<string, T>>(content);
            foreach (KeyValuePair<string, T> p in result)
            {
                p.Value.SetCodeName(p.Key);
            }
            return result;
        }
    }
}

[System.Serializable]
public class MergeBoardConfig
{
    public int RowCount { get; protected set; }
    public int ColumnCount { get; protected set; }
    public List<Coord> ObstacleCoods { get; protected set; } = new List<Coord>();
    public List<MergeItemConfig> Items { get; protected set; } = new List<MergeItemConfig>();

    public MergeBoardConfig() { }

    public MergeBoardConfig(int rowCount, int columnCount, List<Coord> obstaclesCoord)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
        ObstacleCoods = obstaclesCoord;
    }

    public static MergeBoard PrepareMergeBoard(MergeBoardConfig config)
    {
        var mergeBoard = new MergeBoard();
        int rowCount = config.RowCount;
        int columnCount = config.ColumnCount;
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                var coord = new Coord(i, j);
                var obstacleCoord = config.ObstacleCoods.Find(coordTemp => coordTemp.Equals(coord));
                if (obstacleCoord != null)
                {
                    continue;
                }

                mergeBoard.SlotMap.Add(coord.ToString(), new Slot());
            }
        }
        mergeBoard.Init();

        return mergeBoard;
    }

    [System.Serializable]
    public class MergeItemConfig
    {
        public string CodeName { get; protected set; }
        public int Tier { get; protected set; }
        public int RowIndex { get; protected set; }
        public int ColumnIndex { get; protected set; }
        public List<Coord> Coords { get; protected set; } = new List<Coord>();

        public MergeItemConfig() { }

        public MergeItemConfig(string codeName, int tier)
        {
            CodeName = codeName;
            Tier = tier;
        }
    }
}

[System.Serializable]
public class ItemConfig : BaseConfig
{
    public string BuildingCodeName;
    public int MaxUseCount { get; protected set; } = 1;
    public List<MatrixRow> SpaceMatrixRaw { get; protected set; } = new List<MatrixRow>();
    public List<List<int>> SpaceMatrix { get; protected set; } = new List<List<int>>();

    public void Init()
    {
        foreach (var matrixRow in SpaceMatrixRaw)
        {
            SpaceMatrix.Add(matrixRow.Columns);
        }
    }

    public static MergeItem CreateMergeItem(ItemConfig config)
    {
        var mergeItem = new MergeItem(config.CodeName);
        mergeItem.Init(
            maxUseCount: config.MaxUseCount
            , spaceMatrix: config.SpaceMatrix
            );
        return mergeItem;
    }
}

[System.Serializable]
public class MatrixRow
{
    public List<int> Columns { get; protected set; }
}
[System.Serializable]
public class EnergyConfig
{
    public int BaseEnergy { get; protected set; }
    public int MaxEnergy { get; protected set; }
    public float RecoverSpeed { get; protected set; }
}
[System.Serializable]
public class UnitConfig : BaseConfig
{
    public float BaseMoveSpeed { get; protected set; }
    public List<RoutineConfig> Routine { get; protected set; } = new List<RoutineConfig>();
    public int RoutineLoopCount { get; protected set; } = 0;
}
[System.Serializable]
public class SpawnConfig 
{
    public float BaseSpawnInterval { get; protected set; }
    public List<ItemPoolConfig> ItemPool { get; protected set; } = new List<ItemPoolConfig>();
}
[System.Serializable]
public class ItemPoolConfig
{
    public string CodeName { get; protected set; }
    public int Weight { get; protected set; }
}
[System.Serializable]
public class RoutineConfig : BaseConfig
{
}

[System.Serializable]
public class BuildingConfig : BaseConfig
{   

    public Dictionary<string, ActivityConfig> Activities { get; protected set; } = new Dictionary<string, ActivityConfig>();
    public int QueueIncreaseAmount { get; protected set; }
    public int CapacityIncreaseAmount { get; protected set; }

    public ActivityConfig GetActivityConfig(string codeName)
    {
        if (Activities.TryGetValue(codeName, out var activityConfig))
        {
            return activityConfig;
        }

        return null;
    }

    [System.Serializable]
    public class ActivityConfig : BaseConfig
    {
        public float Duration { get; protected set; }
    }

    public void Init()
    {
        foreach (var entry in Activities)
        {
            entry.Value.SetCodeName(entry.Key);
        }
    }
}

[System.Serializable]
public class UnitSpawnerConfig
{
    public List<WeightItemConfig> UnitPool { get; protected set; } = new List<WeightItemConfig>();
    protected List<WeightItemConfig> calculatedUnitPool = new List<WeightItemConfig>();
    public float IntervalDuration { get; protected set; }
    public int IntervalMaxUnitCount { get; protected set; }

    public void Init()
    {
        var sortedUnitPool = UnitPool.OrderBy(item => item.Weight).ToList();
        int count = sortedUnitPool.Count;
        float weightOffset = 0;
        for (int i = 0; i < count; i++)
        {
            var poolItem = sortedUnitPool[i];

            poolItem.SetWeight(poolItem.Weight + weightOffset);
            weightOffset = poolItem.Weight;
        }

        for (int i = 0; i < count; i++)
        {
            var poolItem = sortedUnitPool[i];
            poolItem.SetWeight(poolItem.Weight / sortedUnitPool[count - 1].Weight);
        }

        calculatedUnitPool = sortedUnitPool;
    }

    public WeightItemConfig GetRandomPoolItem()
    {
        float rndValue = Random.Range(0f, 1f);
        foreach (var poolItem in calculatedUnitPool)
        {
            if (rndValue <= poolItem.Weight)
            {
                return poolItem;
            }
        }

        return null;
    }
}

[System.Serializable]
public class WeightItemConfig
{
    public string CodeName { get; protected set; }
    public float Weight { get; protected set; }

    public void SetWeight(float value)
    {
        Weight = value;
    }
}

[System.Serializable]
public class BaseConfig
{
    public string CodeName { get; protected set; }

    public void SetCodeName(string value)
    {
        CodeName = value;
    }
}
