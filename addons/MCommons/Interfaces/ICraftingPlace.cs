using System.Collections.Generic;
using Godot;

namespace Munglo.Commons
{
    /// <summary>
    /// Try to make this a generic interface later
    /// </summary>
    public interface ICraftingPlace
    {
        public bool isOccupied { get; }
        public bool isUsable{ get; }
        public bool isCrafting{ get; }
        public bool DoneCrafting { get; }
        public bool craftingCompleted{ get; }
        public Vector3 Position { get; }
        public Vector3 Direction { get; }
        public void Clear();
        /// <summary>
        /// This should return true if recipe is valid and accepted for this crafting place
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public bool StartCrafting<T>(IRecipe recipe, List<T> comps);

        public object EndCrafting();

    }
}
