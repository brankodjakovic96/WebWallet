using Core.ApplicationServices;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using Core.Domain.Services.Internal.BankRoutingService;
using Core.Domain.Services.Internal.FeeService;
using Core.Infrastructure.DataAccess.EfCoreDataAccess;
using Core.Infrastructure.Services.BrankoBankServiceMock;
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
    public class WalletTransferTests
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
            CoreUnitOfWork.ClearTracking();

            Wallet wallet2 = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                wallet => wallet.Jmbg == "0605996781028",
                wallet => wallet.Transactions
            );
            if (wallet2 != null)
            {
                await CoreUnitOfWork.WalletRepository.Delete(wallet2);
                await CoreUnitOfWork.SaveChangesAsync();
            }
            CoreUnitOfWork.ClearTracking();

            Wallet wallet3 = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                wallet => wallet.Jmbg == "0605996781027",
                wallet => wallet.Transactions
            );

            if (wallet3 != null)
            {
                await CoreUnitOfWork.WalletRepository.Delete(wallet3);
                await CoreUnitOfWork.SaveChangesAsync();
            }
            await DbContext.DisposeAsync();
            DbContext = null;
        }

        [TestMethod]
        public async Task WalletTransferWithoutFeeSuccessTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password1 = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                string password2 = await walletService.CreateWallet("ime", "prezime", "0605996781028", (short)BankType.BrankoBank, "1234", "123456789876543210");

                await walletService.Deposit("0605996781029", password1, 100000M);

                //Act
                await walletService.Transfer("0605996781029", password1, "0605996781028", 100000M);

                //Assert
                Wallet walletSource = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                Wallet walletDesitnation = await CoreUnitOfWork.WalletRepository.GetById("0605996781028");

                Assert.AreEqual(0, walletSource.Balance, "Wallet balance must be 0");
                Assert.AreEqual(TransactionType.TransferPayOut, walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayOut).Type, $"A transaction of type {TransactionType.TransferPayOut} must exist on the wallet.");
                Assert.AreEqual(100000M, walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayOut).Amount, $"{TransactionType.TransferPayOut} transaction amount must be 100000.");
                Assert.AreEqual("0605996781029", walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayOut).Source, $"Source of the transaction should be '0605996781029'.");
                Assert.AreEqual("0605996781028", walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayOut).Destination, $"Destination of the transaction should be '0605996781028'.");

                Assert.AreEqual(null, walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.FeePayOut), $"A transaction of type {TransactionType.FeePayOut} must not exist on the wallet.");

                Assert.AreEqual(100000M, walletDesitnation.Balance, "Wallet balance must be 100000");
                Assert.AreEqual(TransactionType.TransferPayIn, walletDesitnation.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayIn).Type, $"A transaction of type {TransactionType.TransferPayIn} must exist on the wallet.");
                Assert.AreEqual(100000M, walletDesitnation.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayIn).Amount, $"{TransactionType.TransferPayIn} transaction amount must be 100000.");
                Assert.AreEqual("0605996781029", walletDesitnation.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayIn).Source, $"Source of the transaction should be '0605996781029'.");
                Assert.AreEqual("0605996781028", walletDesitnation.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayIn).Destination, $"Destination of the transaction should be '0605996781028'.");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }


        [TestMethod]
        public async Task WalletTransferWithFeeSuccessTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password1 = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                string password2 = await walletService.CreateWallet("ime", "prezime", "0605996781028", (short)BankType.BrankoBank, "1234", "123456789876543210");

                await walletService.Deposit("0605996781029", password1, 101000M);

                //Act
                await walletService.Transfer("0605996781029", password1, "0605996781028", 100000M);

                //Assert
                Wallet walletSource = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                Wallet walletDesitnation = await CoreUnitOfWork.WalletRepository.GetById("0605996781028");

                Assert.AreEqual(0, walletSource.Balance, "Wallet balance must be 0");
                Assert.AreEqual(TransactionType.TransferPayOut, walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayOut).Type, $"A transaction of type {TransactionType.TransferPayOut} must exist on the wallet.");
                Assert.AreEqual(100000M, walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayOut).Amount, $"{TransactionType.TransferPayOut} transaction amount must be 100000.");
                Assert.AreEqual("0605996781029", walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayOut).Source, $"Source of the transaction should be '0605996781029'.");
                Assert.AreEqual("0605996781028", walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayOut).Destination, $"Destination of the transaction should be '0605996781028'.");

                Assert.AreEqual(TransactionType.FeePayOut, walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.FeePayOut).Type, $"A transaction of type {TransactionType.FeePayOut} must exist on the wallet.");
                Assert.AreEqual(1000M, walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.FeePayOut).Amount, $"{TransactionType.FeePayOut} transaction amount must be 1000.");
                Assert.AreEqual("0605996781029", walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.FeePayOut).Source, $"Source of the transaction should be '0605996781029'.");
                Assert.AreEqual("System", walletSource.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.FeePayOut).Destination, $"Destination of the transaction should be 'System'.");

                Assert.AreEqual(100000M, walletDesitnation.Balance, "Wallet balance must be 100000");
                Assert.AreEqual(TransactionType.TransferPayIn, walletDesitnation.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayIn).Type, $"A transaction of type {TransactionType.TransferPayIn} must exist on the wallet.");
                Assert.AreEqual(100000M, walletDesitnation.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayIn).Amount, $"{TransactionType.TransferPayIn} transaction amount must be 100000.");
                Assert.AreEqual("0605996781029", walletDesitnation.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayIn).Source, $"Source of the transaction should be '0605996781029'.");
                Assert.AreEqual("0605996781028", walletDesitnation.Transactions.FirstOrDefault(transaction => transaction.Type == TransactionType.TransferPayIn).Destination, $"Destination of the transaction should be '0605996781028'.");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
            finally
            {
                Configuration["FirstTransactionFreeEachMonth"] = "True";
            }
        }


        [TestMethod]
        public async Task WalletTransferMoreThanMaximumWithdrawFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password1 = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                string password2 = await walletService.CreateWallet("ime", "prezime", "0605996781028", (short)BankType.BrankoBank, "1234", "123456789876543210");
                string password3 = await walletService.CreateWallet("ime", "prezime", "0605996781027", (short)BankType.BrankoBank, "1234", "123456789876543210");

                await walletService.Deposit("0605996781029", password1, 500000M);
                await walletService.Deposit("0605996781027", password3, 750000M);

                await walletService.Transfer("0605996781027", password3, "0605996781028", 400000);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await walletService.Transfer("0605996781029", password1, "0605996781028", 400000), $"Transaction would exceed wallet (0605996781029) monthly withdraw limit ({Configuration["MaximalWithdraw"]} RSD).");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }

        }


        [TestMethod]
        public async Task WalletTransferMoreThanMaximumDepositFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password1 = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                string password2 = await walletService.CreateWallet("ime", "prezime", "0605996781028", (short)BankType.BrankoBank, "1234", "123456789876543210");
                string password3 = await walletService.CreateWallet("ime", "prezime", "0605996781027", (short)BankType.BrankoBank, "1234", "123456789876543210");

                await walletService.Deposit("0605996781029", password1, 750000M);
                await walletService.Deposit("0605996781027", password3, 750000M);
                await walletService.Transfer("0605996781029", password1, "0605996781028", 300000);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await walletService.Transfer("0605996781029", password1, "0605996781028", 300000), $"Transaction would exceed wallet (0605996781028) monthly deposit limit ({Configuration["MaximalDeposit"]} RSD).");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }

        }

        [TestMethod]
        public async Task WalletTransferSourceAccountDoesntExistFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781028", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.Transfer("0605996781029", "passwr", "0605996781028", 750000), $"No wallet for entered jmbg '{"0605996781029"}' and password pair.");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task WalletTransferDestinationAccountDoesntExistFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                await walletService.Deposit("0605996781029", password, 750000M);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.Transfer("0605996781029", password, "0605996781028", 500000), $"No wallet for entered jmbg '{"0605996781028"}' and password pair.");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task WalletTransferSourceAndDestinationJmbgSameFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                await walletService.Deposit("0605996781029", password, 500000M);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.Transfer("0605996781029", password, "0605996781029", 500000), $"Source and destination Jmbg must be different.");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }


        [TestMethod]
        public async Task WalletTransferWalletBlockedFailTest()
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
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await walletService.Transfer("0605996781029", password, "0605996781028", 500000), $"Wallet '{0605996781029}' is blocked");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }
    }
}
