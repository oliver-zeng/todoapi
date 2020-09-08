using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class TodoApiDbContext : DbContext
    {
        public TodoApiDbContext(DbContextOptions options) : base(options)
        {
        }

        protected TodoApiDbContext()
        {
        }
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}