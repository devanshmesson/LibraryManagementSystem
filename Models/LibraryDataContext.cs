using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;


#nullable disable

namespace LoginPage1.Models
{
    public partial class LibraryDataContext : DbContext
    {
        
        public LibraryDataContext(DbContextOptions<LibraryDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<LendRequest> LendRequests { get; set; }
        public virtual DbSet<Publisher> Publishers { get; set; }

        
    }
}
