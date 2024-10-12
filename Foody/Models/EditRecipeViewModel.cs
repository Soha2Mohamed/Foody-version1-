namespace Foody.Models
{
    public class EditRecipeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string Instructions { get; set; }

        public int CookingTIme { get; set; }
        public float RatingAverage { get; set; }
        public string Cuisine { get; set; }
        public DateTime Updated_at { get; set; }
        public IFormFile RecipePicture { get; set; }
        public string Notes { get; set; }
        public string recipePhotoss { get; set; } = "o";

        public List<RecipeIngredientViewModel> Ingredients { get; set; }
        
    }
}
