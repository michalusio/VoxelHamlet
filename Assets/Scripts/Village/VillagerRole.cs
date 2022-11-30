using System;

namespace Assets.Scripts.Village
{
    [Flags]
    public enum VillagerRole
    {
        Hauler = 1,
        Miner = 2,
        Builder = 4
    }
}