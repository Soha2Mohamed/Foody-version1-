using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Foody.Models
{
    public class AddRecipeViewModel
    {
        public AddRecipeViewModel()
        {
            // Initialize the Ingredients list to prevent null reference errors
            Ingredients = new List<RecipeIngredientViewModel>
            {
                   new RecipeIngredientViewModel { IngredientId = 1, Name = "Salt", Quantity = 1.0 },
                   new RecipeIngredientViewModel { IngredientId = 2, Name = "Pepper", Quantity = 0.5 }

            };
        }
        [Required(ErrorMessage = "Name of recipe is required.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Instructions are required.")]
        public string Instructions { get; set; }

        [Required(ErrorMessage = "Give your estimated cooking time.")]
        public int CookingTime { get; set; }

        public string Notes { get; set; }
        public string Cuisine { get; set; }

        // Collection of ingredients with their quantities
        public List<RecipeIngredientViewModel> Ingredients { get; set; } = new List<RecipeIngredientViewModel>();
        
    }
}
