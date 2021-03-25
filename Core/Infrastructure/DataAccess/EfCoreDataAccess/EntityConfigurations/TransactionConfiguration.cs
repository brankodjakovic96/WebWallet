using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure.DataAccess.EfCoreDataAccess.EntityConfigurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> modelBuilder)
        {
            modelBuilder.Property(a => a.Id).ValueGeneratedOnAdd();
            modelBuilder.Property(a => a.Source).HasMaxLength(100);
            modelBuilder.Property(a => a.Destination).HasMaxLength(100);
            modelBuilder.Property(a => a.Amount).HasPrecision(12, 2);
            modelBuilder.Property(a => a.TransactionDateTime).HasColumnType("datetime2");
            modelBuilder.HasOne(a => a.Wallet).WithMany(w => w.Transactions).HasForeignKey(a => a.WalletJmbg).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
