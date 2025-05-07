
namespace Munglo.Commons
{
    //[CreateAssetMenu(fileName = "Recipe", menuName = "Recipes", order = 1)]
    public class RecipeObject : IRecipe
    {
        private string name = "UnNamed";
        
        //[SerializeField, Tooltip("The things needed to fulfill the recipe")]
        private ComponentEntry[] pattern;

        public bool isFullFilled => throw new System.NotImplementedException();

        public ComponentEntry[] Pattern => pattern;

        public int PartsCount => CountParts();

        public string Name => name;

        public float CraftingTime => craftingTime;

        public float craftingTime = 10.0f;

        private int CountParts()
        {
            int count = 0;
            for (int i = 0; i < pattern.Length; i++)
            {
                count += pattern[i].amount;
            }
            return count;
        }
       
    }// EOF CLASS
}