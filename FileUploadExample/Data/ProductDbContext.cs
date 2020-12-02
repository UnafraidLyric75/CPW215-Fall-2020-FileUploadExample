using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FileUploadExample.Models;

namespace FileUploadExample.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext (DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileUploadExample.Models.Product> Product { get; set; }
    }
}
