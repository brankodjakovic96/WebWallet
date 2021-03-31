using Core.ApplicationServices;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using Core.Domain.Services.Internal.BankRoutingService;
using Core.Domain.Services.Internal.FeeService;
using Core.Infrastructure.DataAccess.EfCoreDataAccess;
using Core.Infrastructure.Services.BrankoBankServiceMock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.ApplicationServicesTests
{
    [TestClass]
    public class ChangePasswordTests
    {
        private ICoreUnitOfWork CoreUnitOfWork;
        private CoreEfCoreDbContext DbContext;
        private IBankRoutingService BankRoutingService;
        private IConfiguration Configuration;
        private IFeeService FeeService;


        [TestInitialize]
        public async Task Setup()
        {
            var dbContextFactory = new DbContextFactory();
            DbContext = dbContextFactory.CreateDbContext(new string[] { });
            CoreUnitOfWork = new CoreEfCoreUnitOfWork(DbContext);
            var brankoBankService = new BrankoBankService();
            BankRoutingService = new BankRoutingService(brankoBankService);
            FeeService = new FeeService();

            var inMemorySettings = new Dictionary<string, string> {
                {"MaximalDeposit", "750000"},
                {"MaximalWithdraw", "500000"},
                {"NumberOfDaysAfterCreationWithNoFee", "0"},
                {"FirstTransactionFreeEachMonth", "True"},
                {"FixedFeeLimit", "10000" },
                {"FixedFee", "100"},
                {"PercentageFee", "1"}
            };

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [TestCleanup()]
        public async Task Cleanup()
        {
            CoreUnitOfWork.ClearTracking();
            Wallet wallet1 = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                    wallet => wallet.Jmbg == "0605996781029",
                    wallet => wallet.Transactions
                );
            if (wallet1 != null)
            {
                await CoreUnitOfWork.WalletRepository.Delete(wallet1);
                await CoreUnitOfWork.SaveChangesAsync();
            }
            await DbContext.DisposeAsync();
            DbContext = null;
        }


        [TestMethod]
        public async Task ChangePasswordSuccessTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");

                //Act
                await walletService.ChangePassword("0605996781029", password, "123456", "123456");

                //Assert
                wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                Assert.AreEqual(true, wallet.CheckPassword("123456"), "Password must be '123456'.");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }


        [TestMethod]
        public async Task ChangePasswordWrongLengthFailTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert        
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.ChangePassword("0605996781029", password, "1234567", "1234567"), "New password must be 6 digits long.");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }


        [TestMethod]
        public async Task ChangePasswordContainsNonDigitCharacterFailTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert        
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.ChangePassword("0605996781029", password, "12345a", "12345a"), "New password can only contain digits.");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task ChangePasswordWrongOldPasswordFailTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert        
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.ChangePassword("0605996781029", "654321", "123456", "123456"), "Old password doesn't match.");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task ChangePasswordPasswordAndConfirmationDontMatchFailTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert        
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.ChangePassword("0605996781029", "654321", "123456", "12345"), "New password and password confirmation do not match.");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

    }
}
