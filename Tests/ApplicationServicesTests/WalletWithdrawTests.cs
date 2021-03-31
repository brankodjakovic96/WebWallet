using Core.ApplicationServices;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using Core.Domain.Services.Internal.BankRoutingService;
using Core.Domain.Services.Internal.FeeService;
using Core.Infrastructure.DataAccess.EfCoreDataAccess;
using Core.Infrastructure.Services.BrankoBankServiceMock;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.ApplicationServicesTests
{
    [TestClass]
    public class WalletWithdrawTests
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
                {"MaximalDeposit", "2000000"},
                {"MaximalWithdraw", "2000000"},
            };

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [TestCleanup()]
        public async Task Cleanup()
        {
            CoreUnitOfWork.ClearTracking();
            Wallet wallet = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                    wallet => wallet.Jmbg == "0605996781029",
                    wallet => wallet.Transactions
                );

            if (wallet != null)
            {
                await CoreUnitOfWork.WalletRepository.Delete(wallet);
                await CoreUnitOfWork.SaveChangesAsync();
            }
            await DbContext.DisposeAsync();
            DbContext = null;
        }


        [TestMethod]
        public async Task WalletWithdrawSuccessTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                await walletService.Deposit("0605996781029", password, 110000M);

                //Act
                await walletService.Withdraw("0605996781029", password, 100000M);

                //Assert
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");

                Assert.AreEqual(10000M, wallet.Balance, "Wallet balance must be 10000");
                Assert.AreEqual(TransactionType.Withdraw, wallet.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.Withdraw).Type, $"A transaction of type {TransactionType.Withdraw} must exist on the wallet.");
                Assert.AreEqual(100000M, wallet.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.Withdraw).Amount, $"Withdraw transaction amount must be 100000.");
                Assert.AreEqual(BankType.BrankoBank.ToString(), wallet.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.Withdraw).Destination, $"Source of the transaction should be {BankType.BrankoBank}.");
                Assert.AreEqual("0605996781029", wallet.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.Withdraw).Source, $"Destination of the transaction should be 0605996781029.");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }


        [TestMethod]
        public async Task WalletWithdrawMoreThanMaximumFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await walletService.Withdraw("0605996781029", password, 2600000M), $"Transaction would exceed wallet (0605996781029) monthly withdrawal limit ({Configuration["MaximalWithdraw"]} RSD).");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }

        }


        [TestMethod]
        public async Task WalletWithdrawNotEnoughFundsOnWalletFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await walletService.Withdraw("0605996781029", password, 1100000M), "Bank account doesn't have enoguh funds!");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task WalletWithdrawWalletDoesntExistFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.Withdraw("0605996781029", "1234", 1100000M), $"No wallet for entered jmbg '{"0605996781029"}' and password pair.");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task WalletWithdrawWalletBlockedFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                await walletService.Deposit("0605996781029", password, 750000M);
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                wallet.Block();
                await CoreUnitOfWork.WalletRepository.Update(wallet);
                await CoreUnitOfWork.SaveChangesAsync();
                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await walletService.Withdraw("0605996781029", password, 500000M), $"Wallet '{0605996781029}' is blocked");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }
    }
}
