namespace Foody.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //public string Ingredients { get; set; }

        public string Instructions { get; set; }

        public int CookingTIme { get; set; }
        public float RatingAverage { get; set; }
        public string Cuisine {  get; set; }
        public int UserId { get; set; } 
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get;set; }
        public string RecipePicture { get; set; } 
        public string Notes { get;set; }// chef's secrets
        public string recipePhotoss { get;set; } = "o";

        // Navigation Properties
        public User User { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Playlist> Playlists { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

        public ICollection<cookLater> CookLaters { get; set; }
    }
}
