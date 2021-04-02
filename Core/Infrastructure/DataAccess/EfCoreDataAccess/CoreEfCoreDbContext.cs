using Common.EfCoreDataAccess;
using Core.Domain.Entities;
using Core.Infrastructure.DataAccess.EfCoreDataAccess.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure.DataAccess.EfCoreDataAccess
{
    public class CoreEfCoreDbContext : EfCoreDbContext
    {
        public CoreEfCoreDbContext(DbContextOptions<CoreEfCoreDbContext> options) : base(options)
        {

        }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new WalletConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
