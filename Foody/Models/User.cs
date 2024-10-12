using System.ComponentModel.DataAnnotations;

namespace Foody.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        public string Description { get; set; }
        public string ProfilePicture { get; set; }

        public DateTime Created_at { get; set; } = DateTime.Now;

        // Navigation Properties
        public ICollection<Recipe> Recipes { get; set; } // One user can have many meals
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Playlist> Playlists { get; set; }

        public ICollection<cookLater> CookLaters { get; set; }

    }
}
