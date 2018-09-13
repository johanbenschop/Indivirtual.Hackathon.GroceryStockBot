using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Indivirtual.Hackathon.GroceryStockBot.DataContexts
{
    public class DataContext : DbContext
    {
        public DbSet<InventoryEntry> InventoryEntries { get; set; }
        public DbSet<ShoppingCartEntry> ShoppingCartEntries { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryEntry>().HasData(
                new InventoryEntry { ProductName = "AH Snoep­groen­te to­maat", Amount = 2 },
                new InventoryEntry { ProductName = "AH Karnemelk", Amount = 5 },
                new InventoryEntry { ProductName = "AH Biologisch Halfvolle melk", Amount = 2 },
                new InventoryEntry { ProductName = "AH AH Mil­de yog­hurt(half­vol)", Amount = 2 },
                new InventoryEntry { ProductName = "AH Snoepfruit", Amount = 4 });

            modelBuilder.Entity<ShoppingCartEntry>().HasData(
                new ShoppingCartEntry { ProductName = "Lay's Pa­pri­ka", Amount = 10 },
                new ShoppingCartEntry { ProductName = "Lay's Wok­kels pa­pri­ka", Amount = 12 },
                new ShoppingCartEntry { ProductName = "AH Bi­o­lo­gisch Ap­pel­chips", Amount = 12 },
                new ShoppingCartEntry { ProductName = "Chi­quita Ba­na­nen", Amount = 120 },
                new ShoppingCartEntry { ProductName = "Unox Cup-a-soup ta­gli­a­tel­le & cham­pig­non", Amount = 21 });
        }
    }

    public class InventoryEntry
    {
        [Key]
        public string ProductName { get; set; }
        public int Amount { get; set; }
    }

    public class ShoppingCartEntry
    {
        [Key]
        public string ProductName { get; set; }
        public int Amount { get; set; }
    }
}
