using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
namespace Munglo.Commons
{
    public interface IInventory<T>
    {
        public List<T> Items { get; } // All items in inventory
        public bool HaveItemWithID(ulong itemID);
        public bool HaveItems<T1>(int count);
        public bool AddItem(T item, bool parentToInventory = true);
        public int CountItems(string typeName);
        public int CountItems<T3>();
        public bool RemoveItem(T item);
        public IFood GetMostEfficientFood();
        public bool RemoveItem<T2>();
        public void RemoveLastItem();
        /// <summary>
        /// returns true if the recipe can be fulfilled with the items in the inventory
        /// </summary>
        /// <typeparam name="T4"></typeparam>
        /// <param name="recipe"></param>
        /// <returns></returns>
        //public bool CheckRecipe<T4>(IRecipe<T4> recipe);
        /// <summary>
        /// Move needed items from inventory to recipe and return true if it is all fulfilled
        /// </summary>
        /// <typeparam name="T4"></typeparam>
        /// <param name="recipe"></param>
        /// <returns></returns>
        //public bool FillRecipe(IRecipe<T> recipe, out List<T> comps);

        public int Count { get; }

        public bool HasAmmo();
        public int PayAmmo(int chargesMax);

    }// EOF CLASS
}