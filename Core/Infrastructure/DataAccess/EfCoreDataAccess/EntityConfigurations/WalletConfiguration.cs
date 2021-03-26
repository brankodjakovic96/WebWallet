using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure.DataAccess.EfCoreDataAccess.EntityConfigurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> modelBuilder)
        {
            modelBuilder.Property(a => a.Jmbg).HasMaxLength(13);
            modelBuilder.HasKey(a => a.Jmbg);
            modelBuilder.Property(a => a.FirstName).HasMaxLength(200);
            modelBuilder.Property(a => a.LastName).HasMaxLength(200);
            modelBuilder.Property(a => a.BankAccount).HasMaxLength(18);
            modelBuilder.Property(a => a.PIN).HasMaxLength(4);
            modelBuilder.Property(a => a.Balance).HasPrecision(12, 2);
            modelBuilder.Property(a => a.UsedDepositThisMonth).HasPrecision(12, 2);
            modelBuilder.Property(a => a.UsedWithdrawThisMonth).HasPrecision(12, 2);
            modelBuilder.Property("_password");
            modelBuilder.Property("_password").HasMaxLength(6);
            modelBuilder.Property(a => a.RowVersion).IsRowVersion();

        }
    }
}
