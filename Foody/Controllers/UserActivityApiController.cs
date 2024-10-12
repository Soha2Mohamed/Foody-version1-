using Foody.Data;
using Foody.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class UserActivityApiController : ControllerBase
{
    private readonly ApplicationDbcontext _applicationDbcontext;

    public UserActivityApiController(ApplicationDbcontext applicationDbcontext)
    {
        _applicationDbcontext = applicationDbcontext;
    }

    // GET: api/Recipes/TopRated
    [HttpGet("Timeline")]
    public async Task<IActionResult> Timeline()
    {
        var topRecipes = await _applicationDbcontext.Recipes
            .OrderByDescending(r => r.RatingAverage)
            .Take(10)
            .ToListAsync();

        return Ok(topRecipes);
    }

    // POST: api/Recipes/CookLater/{id}
    [HttpPost("CookLater/{id}")]
    public async Task<IActionResult> AddToCookLater(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Unauthorized("Please log in first.");
        }

        var recipe = await _applicationDbcontext.Recipes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null) // Check if recipe exists
        {
            return NotFound("Recipe not found.");
        }

        var existingRecipe = await _applicationDbcontext.CookLaters
            .FirstOrDefaultAsync(cl => cl.RecipeId == id && cl.UserId == userId.Value);

        if (existingRecipe != null)
        {
            return Conflict("This recipe is already in your Cook Later list.");
        }
        else
        {
            // Add to Cook Later list
            var newCookLaterRecipe = new cookLater
            {
                RecipeId = id,
                UserId = userId.Value,
                Added_at = DateTime.Now
            };

            _applicationDbcontext.CookLaters.Add(newCookLaterRecipe);
            await _applicationDbcontext.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetCookLaterRecipes), new { id = id }, "Recipe added to Cook Later.");
    }

    // GET: api/Recipes/CookLater
    [HttpGet("CookLater")]
    public async Task<IActionResult> GetCookLaterRecipes()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Unauthorized("Please log in first.");
        }

        var yourRecipes = await _applicationDbcontext.CookLaters
            .Where(cl => cl.UserId == userId.Value)
            .Include(cl => cl.Recipe)  // Include related recipe details
            .ToListAsync();

        return Ok(yourRecipes);
    }
}
