using Foody.Models;
using Microsoft.EntityFrameworkCore;

namespace Foody.Data
{
    public class ApplicationDbcontext : DbContext
    {
        public ApplicationDbcontext(DbContextOptions<ApplicationDbcontext> options): base (options)
        {
                
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<cookLater> CookLaters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Comment>()
    .HasOne(c => c.User)
    .WithMany(u => u.Comments)
    .HasForeignKey(c => c.UserId)
    .OnDelete(DeleteBehavior.NoAction);  // Disable cascading delete


            modelBuilder.Entity<Playlist>()
    .HasOne(c => c.User)
    .WithMany(u => u.Playlists)
    .HasForeignKey(c => c.UserId)
    .OnDelete(DeleteBehavior.NoAction);  // Disable cascading delete

            modelBuilder.Entity<Rating>()
    .HasOne(c => c.User)
    .WithMany(u => u.Ratings)
    .HasForeignKey(c => c.UserId)
    .OnDelete(DeleteBehavior.NoAction);  // Disable cascading delete

            modelBuilder.Entity<RecipeIngredient>()
           .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<cookLater>() // Use 'CookLater' with uppercase 'C'
               .HasKey(cl => new { cl.RecipeId, cl.UserId }); // Correct naming of entity

            modelBuilder.Entity<cookLater>()
                .HasOne(cl => cl.Recipe)
                .WithMany(r => r.CookLaters) // Make sure Recipe model has a collection of CookLater
                .HasForeignKey(cl => cl.RecipeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<cookLater>()
                .HasOne(cl => cl.User)
                .WithMany(u => u.CookLaters) // Make sure User model has a collection of CookLater
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
