namespace Foody.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }

       // public int RecipeId { get; set; }
        public double Quantity { get; set; }

       // public Recipe Recipe { get; set; }

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    }
}
