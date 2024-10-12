using Foody.Data;
using Foody.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Foody.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbcontext _applicationDbcontext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RecipesController(ApplicationDbcontext applicationDbcontext, IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbcontext = applicationDbcontext;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Display form for adding a new recipe
        public async Task<IActionResult> AddRecipe()
        {
            var model = new AddRecipeViewModel
            {
                Ingredients = new List<RecipeIngredientViewModel>
                {
                    //just to initialize some ingredients for the user to start with
                   new RecipeIngredientViewModel { IngredientId = 1, Name = "Salt", Quantity = 1.0 },
                   new RecipeIngredientViewModel { IngredientId = 2, Name = "Pepper", Quantity = 0.5 }
                }
            };
            // Load available ingredients to display in the dropdown
            ViewBag.Ingredients = await _applicationDbcontext.Ingredients.ToListAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddRecipe(AddRecipeViewModel model, IFormFile RecipePicture)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Users");
                }

                if (_applicationDbcontext.Users.Any(u => u.Id == userId.Value))
                {
                    string RecipePictureFileName = null;

                    // Handle recipe picture upload
                    if (RecipePicture != null)
                    {
                        RecipePictureFileName = Guid.NewGuid().ToString() + "_" + RecipePicture.FileName;

                        string FilePath = Path.Combine(_webHostEnvironment.WebRootPath, "RecipesUploads", RecipePictureFileName);

                        using (var fileStream = new FileStream(FilePath, FileMode.Create))
                        {
                            await RecipePicture.CopyToAsync(fileStream);
                        }
                    }

                    var recipe = new Recipe
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Instructions = model.Instructions,
                        CookingTIme = model.CookingTime,
                        Cuisine = model.Cuisine,
                        UserId = userId.Value,
                        RecipePicture = RecipePictureFileName,
                        Notes = model.Notes,
                        Created_at = DateTime.Now
                    };

                    _applicationDbcontext.Recipes.Add(recipe);
                    await _applicationDbcontext.SaveChangesAsync();

                  /*  Console.WriteLine("Ingredients Count: " + model.Ingredients.Count);
                    foreach (var ingredient in model.Ingredients)
                    {
                        Console.WriteLine("Ingredient ID: " + ingredient.IngredientId + ", Quantity: " + ingredient.Quantity);
                    }*/

                    // Handle adding ingredients and their quantities
                    foreach (var ingredientModel in model.Ingredients)
                    {
                        // Add the ingredient if it doesn't exist in the database (This assumes you want to allow new ingredients)
                        var ingredient = await _applicationDbcontext.Ingredients
                            .FirstOrDefaultAsync(i => i.Name == ingredientModel.Name);

                        if (ingredient == null)
                        {
                            // If the ingredient doesn't exist, create a new one
                            ingredient = new Ingredient { Name = ingredientModel.Name };  // Remove this if you're selecting by ID only
                            _applicationDbcontext.Ingredients.Add(ingredient);
                            await _applicationDbcontext.SaveChangesAsync();
                        }

                        // Link the recipe to the selected ingredient with the quantity
                        var recipeIngredient = new RecipeIngredient
                        {
                            RecipeId = recipe.Id,
                            IngredientId = ingredient.Id,  
                            Quantity = ingredient.Quantity,
                            Name = ingredientModel.Name,
                        };

                        _applicationDbcontext.RecipeIngredients.Add(recipeIngredient);

                    }

                    await _applicationDbcontext.SaveChangesAsync();

                    return RedirectToAction("ViewRecipe", new { id = recipe.Id });
                }
                return RedirectToAction("Login", "Users");
            }

            // Reload available ingredients in case of validation failure
            ViewBag.AvailableIngredients = await _applicationDbcontext.Ingredients.ToListAsync();
            return View(model);
        }

        
        public async Task<IActionResult> ViewRecipe(int id)
        {
            var recipe = await _applicationDbcontext.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Check if the user exists
            var user = await _applicationDbcontext.Users.FindAsync(userId.Value);

            if (user == null)
            {
                return RedirectToAction("Login", "Users"); 
            }

            // Find the recipe by ID and check if it belongs to the current user
            var recipe = await _applicationDbcontext.Recipes
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);

            if (recipe == null)
            {
                return NotFound(); // Recipe not found or doesn't belong to the user
            }

            // Check and delete the recipe picture if it exists
            if (!string.IsNullOrEmpty(recipe.RecipePicture))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "RecipesUploads", recipe.RecipePicture);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // Delete the image file
                }
            }

            // Delete the recipe from the database
            _applicationDbcontext.Recipes.Remove(recipe);
            await _applicationDbcontext.SaveChangesAsync();

            return RedirectToAction("Profile", "Users"); // Redirect back to profile after successful deletion
        }

        public async Task<IActionResult> ViewRecipes()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }
            var yourRecipes = await _applicationDbcontext.Recipes
                        .Include(r => r.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient)
                        .Where(r => r.UserId == userId.Value)
                        .ToListAsync();
            return View(yourRecipes);
        }

        [HttpGet]
        public async Task<IActionResult> EditRecipe(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var recipe = await _applicationDbcontext.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);

            if (recipe == null)
            {
                return NotFound();
            }

            var model = new EditRecipeViewModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                Instructions = recipe.Instructions,
                CookingTIme = recipe.CookingTIme,
                Cuisine = recipe.Cuisine,
                Notes = recipe.Notes,

                Ingredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientViewModel
                {
                    IngredientId = ri.IngredientId,
                    Name = ri.Ingredient.Name,
                    Quantity = ri.Quantity
                }).ToList()
            };

            ViewBag.AvailableIngredients = await _applicationDbcontext.Ingredients.ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRecipe(int id, EditRecipeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AvailableIngredients = await _applicationDbcontext.Ingredients.ToListAsync();
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var recipe = await _applicationDbcontext.Recipes
                .Include(r => r.RecipeIngredients)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);

            if (recipe == null)
            {
                return NotFound();
            }

            // Update recipe fields
            recipe.Name = model.Name;
            recipe.Description = model.Description;
            recipe.Instructions = model.Instructions;
            recipe.CookingTIme = model.CookingTIme;
            recipe.Cuisine = model.Cuisine;
            recipe.Notes = model.Notes;

            // Handle recipe picture
            if (model.RecipePicture != null)
            {
                var RecipePictureFileName = Guid.NewGuid().ToString() + "_" + model.RecipePicture.FileName;
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "RecipesUploads", RecipePictureFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.RecipePicture.CopyToAsync(fileStream);
                }

                recipe.RecipePicture = RecipePictureFileName;
            }

            // Handle ingredient updates
            foreach (var ingredientModel in model.Ingredients)
            {
                var recipeIngredient = recipe.RecipeIngredients
                    .FirstOrDefault(ri => ri.IngredientId == ingredientModel.IngredientId);

                if (recipeIngredient != null)
                {
                    recipeIngredient.Quantity = ingredientModel.Quantity;
                }
            }

            // Save changes
            _applicationDbcontext.Recipes.Update(recipe);
            await _applicationDbcontext.SaveChangesAsync();

            return RedirectToAction("ViewRecipe", new { id = recipe.Id });
        }
        public IActionResult RateRecipe(int id)
        {
            var recipe = _applicationDbcontext.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound(); // or return a different view/message if recipe is not found
            }
            return View(recipe); // Return the RateRecipe view with the recipe details
        }
        [HttpPost]
        public IActionResult RateRecipe(int recipeId, int ratingValue)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Check if the recipe exists in the database
            var recipe = _applicationDbcontext.Recipes
                .FirstOrDefault(r => r.Id == recipeId);

            if (recipe == null) // Check if recipe exists
            {
                return NotFound("Recipe not found.");
            }

            // Check if the user has already rated the recipe
            var existingRating = _applicationDbcontext.Ratings
                .FirstOrDefault(r => r.RecipeId == recipeId && r.UserId == userId.Value);

            if (existingRating != null)
            {
                // Update the existing rating
                existingRating.Score = ratingValue;
                _applicationDbcontext.Ratings.Update(existingRating);
            }
            else
            {
                // Add a new rating
                var newRating = new Rating
                {
                    RecipeId = recipeId,
                    UserId = userId.Value,
                    Score = ratingValue,
                    Created_at = DateTime.Now
                };
                _applicationDbcontext.Ratings.Add(newRating);
            }

            // Calculate the average rating
            var averageRating = _applicationDbcontext.Ratings
                .Where(r => r.RecipeId == recipeId)
                .Average(r => (float?)r.Score) ?? 0; // Handle case if no ratings

            // Update the recipe's average rating
            recipe.RatingAverage = averageRating;

            // Save all changes in one go
            _applicationDbcontext.SaveChanges();

            return RedirectToAction("ViewRecipe", new { id = recipe.Id });
        }

        public async Task<IActionResult> Timeline()
        {
            var topRecipes = await _applicationDbcontext.Recipes
        .OrderByDescending(r => r.RatingAverage)
        .Take(10)  // Fetch top 10 recipes
        .ToListAsync();
            return View(topRecipes);
        }
        /////
        ///<form asp-action="RateRecipe" >
                //<input type = "hidden" name="recipeId" value="@recipe.Id" />
               // <button type = "submit" class="btn btn-primary">Rate Recipe</button>
         //   </form>

        public IActionResult AddToCookLater(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }
            var recipe = _applicationDbcontext.Recipes
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null) // Check if recipe exists
            {
                return NotFound("Recipe not found.");
            }
            var existingRecipe = _applicationDbcontext.CookLaters
                .FirstOrDefault(r => r.RecipeId == id && r.UserId == userId.Value);
            if (existingRecipe != null)
            {

                return View("RecipeAlreadyInList");
            }
            else
            {
                // Add a new rating
                var newCoolLaterRecipe = new cookLater
                {
                    RecipeId = id,
                    UserId = userId.Value,
                    Added_at = DateTime.Now
                };
                _applicationDbcontext.CookLaters.Add(newCoolLaterRecipe);
            }
            _applicationDbcontext.SaveChanges();

            return RedirectToAction("ViewCookLater");
        }
        public IActionResult ViewCookLater()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }
            var yourRecipes =  _applicationDbcontext.CookLaters
            .Where(r => r.UserId == userId.Value)
            .Include(r => r.Recipe)  // Include related recipe details
            .ToList();
            return View(yourRecipes);
        }
    }

}
