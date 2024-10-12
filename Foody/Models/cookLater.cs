namespace Foody.Models
{
    public class cookLater
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public User User { get; set; }
        public DateTime Added_at {  get; set; }

        public Recipe Recipe { get; set; }
    }
}
