using System.ComponentModel.DataAnnotations;

namespace Foody.Models
{
    public class RecipeIngredientViewModel
    {
        [Required]
        public int IngredientId { get; set; } // Ingredient ID to link with Ingredients table

        [Required(ErrorMessage = "Quantity is required.")]
        public double Quantity { get; set; }  // Quantity of the ingredient
        public string Name { get; set; }
        //cut potatoes into small squared then put them in boiled water for 10 minuted then make them 2 halves. the first half , put it in cold water and mash the second half. cook the peas and carrot and then add every thing together and keep the second half of the potates the last you add.
    }
}
