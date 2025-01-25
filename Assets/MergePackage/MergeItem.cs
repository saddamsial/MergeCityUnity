using System.Collections.Generic;

namespace Hiker.Merge
{
    public class MergeItem
    {
        public string CodeName { get; protected set; }
        public int Tier { get; protected set; }
        public bool IsActive { get; protected set; } = true;
        public int MaxUseCount { get; protected set; }
        public int CurrentUseCount { get; protected set; } = 1;
        public bool HasUnlimitedUse { get { return MaxUseCount < 0; } }
        public List<List<int>> SpaceMatrix { get; protected set; } = new List<List<int>>();

        public MergeItem(string codeName)
        {
            CodeName = codeName;
        }

        public void Init(
            int maxUseCount
            , List<List<int>> spaceMatrix
            )
        {
            MaxUseCount = maxUseCount;
            SpaceMatrix = spaceMatrix;
        }

        public event System.Action<int> OnUseCountUpdated;
        public void Use()
        {
            if (CurrentUseCount > 0)
            {
                CurrentUseCount--;
                OnUseCountUpdated?.Invoke(CurrentUseCount);
            }
        }

        public void SetTier(int tier)
        {
            Tier = tier;
        }

        public bool HasUseCount()
        {
            return CurrentUseCount < 0 || CurrentUseCount > 0;
        }

        public event System.Action<bool> OnActivated;
        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            OnActivated?.Invoke(isActive);
        }
    }
}
