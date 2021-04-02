﻿// <auto-generated />
using System;
using Core.Infrastructure.DataAccess.EfCoreDataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Core.Infrastructure.DataAccess.EfCoreDataAccess.Migrations
{
    [DbContext(typeof(CoreEfCoreDbContext))]
    [Migration("20210330150745_Added_WalletCreatedDateTime_And_LastTransferDateTime_To_Wallet")]
    partial class Added_WalletCreatedDateTime_And_LastTransferDateTime_To_Wallet
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.4")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Core.Domain.Entities.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("Amount")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<string>("Destination")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Source")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("TransactionDateTime")
                        .HasColumnType("datetime2");

                    b.Property<byte>("Type")
                        .HasColumnType("tinyint");

                    b.Property<string>("WalletJmbg")
                        .HasColumnType("nvarchar(13)");

                    b.HasKey("Id");

                    b.HasIndex("WalletJmbg");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("Core.Domain.Entities.Wallet", b =>
                {
                    b.Property<string>("Jmbg")
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.Property<decimal>("Balance")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<string>("BankAccount")
                        .HasMaxLength(18)
                        .HasColumnType("nvarchar(18)");

                    b.Property<short>("BankType")
                        .HasColumnType("smallint");

                    b.Property<string>("FirstName")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("LastName")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("LastTransactionDateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastTransferDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("PIN")
                        .HasMaxLength(4)
                        .HasColumnType("nvarchar(4)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<decimal>("UsedDepositThisMonth")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<decimal>("UsedWithdrawThisMonth")
                        .HasPrecision(12, 2)
                        .HasColumnType("decimal(12,2)");

                    b.Property<DateTime>("WalletCreatedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("_password")
                        .HasMaxLength(6)
                        .HasColumnType("nvarchar(6)");

                    b.HasKey("Jmbg");

                    b.ToTable("Wallets");
                });

            modelBuilder.Entity("Core.Domain.Entities.Transaction", b =>
                {
                    b.HasOne("Core.Domain.Entities.Wallet", "Wallet")
                        .WithMany("Transactions")
                        .HasForeignKey("WalletJmbg")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Wallet");
                });

            modelBuilder.Entity("Core.Domain.Entities.Wallet", b =>
                {
                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
