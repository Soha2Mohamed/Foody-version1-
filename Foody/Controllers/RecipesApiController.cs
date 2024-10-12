using Foody.Data;
using Foody.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foody.Controllers
{
    [Route("api/[controller]")]
    [ApiController]  // Makes this controller support API responses
    public class RecipesApiController : ControllerBase
    {
        private readonly ApplicationDbcontext _applicationDbcontext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RecipesApiController(ApplicationDbcontext applicationDbcontext, IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbcontext = applicationDbcontext;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Recipes
        [HttpGet]
        public async Task<IActionResult> GetRecipes()
        {
            var recipes = await _applicationDbcontext.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .ToListAsync();

            if (recipes == null || !recipes.Any())
            {
                return NotFound("No recipes found.");
            }

            return Ok(recipes);
        }

        // GET: api/Recipes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(int id)
        {
            var recipe = await _applicationDbcontext.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound("Recipe not found.");
            }

            return Ok(recipe);
        }

        // POST: api/Recipes
        [HttpPost]
        public async Task<IActionResult> AddRecipe([FromBody] AddRecipeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var recipe = new Recipe
            {
                Name = model.Name,
                Description = model.Description,
                Instructions = model.Instructions,
                CookingTIme = model.CookingTime,
                Cuisine = model.Cuisine,
                UserId = userId.Value,
                Notes = model.Notes,
                Created_at = DateTime.Now
            };

            _applicationDbcontext.Recipes.Add(recipe);
            await _applicationDbcontext.SaveChangesAsync();

            foreach (var ingredientModel in model.Ingredients)
            {
                var ingredient = await _applicationDbcontext.Ingredients
                    .FirstOrDefaultAsync(i => i.Name == ingredientModel.Name);

                if (ingredient == null)
                {
                    ingredient = new Ingredient { Name = ingredientModel.Name };
                    _applicationDbcontext.Ingredients.Add(ingredient);
                    await _applicationDbcontext.SaveChangesAsync();
                }

                var recipeIngredient = new RecipeIngredient
                {
                    RecipeId = recipe.Id,
                    IngredientId = ingredient.Id,
                    Quantity = ingredientModel.Quantity,
                    Name = ingredientModel.Name,
                };

                _applicationDbcontext.RecipeIngredients.Add(recipeIngredient);
            }

            await _applicationDbcontext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRecipeById), new { id = recipe.Id }, recipe);
        }

        // PUT: api/Recipes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditRecipe(int id, [FromBody] EditRecipeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var recipe = await _applicationDbcontext.Recipes
                .Include(r => r.RecipeIngredients)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);

            if (recipe == null)
            {
                return NotFound("Recipe not found.");
            }

            recipe.Name = model.Name;
            recipe.Description = model.Description;
            recipe.Instructions = model.Instructions;
            recipe.CookingTIme = model.CookingTIme;
            recipe.Cuisine = model.Cuisine;
            recipe.Notes = model.Notes;

            foreach (var ingredientModel in model.Ingredients)
            {
                var recipeIngredient = recipe.RecipeIngredients
                    .FirstOrDefault(ri => ri.IngredientId == ingredientModel.IngredientId);

                if (recipeIngredient != null)
                {
                    recipeIngredient.Quantity = ingredientModel.Quantity;
                }
            }

            _applicationDbcontext.Recipes.Update(recipe);
            await _applicationDbcontext.SaveChangesAsync();

            return NoContent();  // Indicates successful update without returning content
        }

        // DELETE: api/Recipes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var recipe = await _applicationDbcontext.Recipes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);

            if (recipe == null)
            {
                return NotFound("Recipe not found.");
            }

            if (!string.IsNullOrEmpty(recipe.RecipePicture))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "RecipesUploads", recipe.RecipePicture);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _applicationDbcontext.Recipes.Remove(recipe);
            await _applicationDbcontext.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Recipes/RateRecipe/{id}
        [HttpPost("RateRecipe/{id}")]
        public async Task<IActionResult> RateRecipe(int id, [FromBody] int ratingValue)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("Please log in first.");
            }

            var recipe = await _applicationDbcontext.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound("Recipe not found.");
            }

            var existingRating = _applicationDbcontext.Ratings
                .FirstOrDefault(r => r.RecipeId == id && r.UserId == userId.Value);

            if (existingRating != null)
            {
                existingRating.Score = ratingValue;
                _applicationDbcontext.Ratings.Update(existingRating);
            }
            else
            {
                var newRating = new Rating
                {
                    RecipeId = id,
                    UserId = userId.Value,
                    Score = ratingValue,
                    Created_at = DateTime.Now
                };
                _applicationDbcontext.Ratings.Add(newRating);
            }

            var averageRating = await _applicationDbcontext.Ratings
                .Where(r => r.RecipeId == id)
                .AverageAsync(r => (float?)r.Score) ?? 0;

            recipe.RatingAverage = averageRating;
            await _applicationDbcontext.SaveChangesAsync();

            return Ok(recipe.RatingAverage);
        }

       

    }
}
