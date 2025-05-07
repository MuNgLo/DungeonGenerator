using System.Collections.Generic;

namespace Munglo.Commons
{
    /// <summary>
    /// Use this interface on recipes for crafting, cooking and such
    /// </summary>
    public interface IRecipe
    {
        public string Name { get; }
        public ComponentEntry[] Pattern { get; }
        public bool isFullFilled { get; }
        public int PartsCount { get; }
        public float CraftingTime { get; }
    }
}
