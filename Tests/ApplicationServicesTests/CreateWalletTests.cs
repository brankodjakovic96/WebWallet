using Core.ApplicationServices;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using Core.Domain.Services.Internal.BankRoutingService;
using Core.Infrastructure.DataAccess.EfCoreDataAccess;
using Core.Infrastructure.Services.BrankoBankServiceMock;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.ApplicationServicesTests
{
    [TestClass]
    public class CreateWalletTests
    {

        private ICoreUnitOfWork CoreUnitOfWork;
        private CoreEfCoreDbContext DbContext;
        private IBankRoutingService BankRoutingService;
        private IConfiguration Configuration;

        [TestInitialize]
        public void Setup()
        {
            var dbContextFactory = new DbContextFactory();
            DbContext = dbContextFactory.CreateDbContext(new string[] { });
            CoreUnitOfWork = new CoreEfCoreUnitOfWork(DbContext);
            var brankoBankService = new BrankoBankService();
            BankRoutingService = new BankRoutingService(brankoBankService);

            var inMemorySettings = new Dictionary<string, string> {
                {"MaximalDeposit", "1000000"},
                {"MaximalWithdraw", "1000000"},
            };

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

        }

        [TestCleanup()]
        public async Task Cleanup()
        {
            await DbContext.DisposeAsync();
            DbContext = null;
        }

        [TestMethod]
        public async Task CreateWalletSuccessTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration);

                //Act
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Assert
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");

                Assert.IsNotNull(wallet, "Wallet must not be null");
                Assert.AreEqual(6, password.Length, "Password must be 6 characters long");
                Assert.AreEqual("ime", wallet.FirstName, "FirstName must be 'ime'");
                Assert.AreEqual("prezime", wallet.LastName, "LastName must be 'prezime'");
                Assert.AreEqual("0605996781029", wallet.Jmbg, "Jmbg must be '0605996781029'");
                Assert.AreEqual(BankType.BrankoBank, wallet.BankType, $"BankType must be '{BankType.BrankoBank}'");
                Assert.AreEqual("1234", wallet.PIN, "PIN must be '1234'");
                Assert.AreEqual("123456789876543210", wallet.BankAccount, "BankAccount must be '123456789876543210'");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
            finally
            {
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                if (wallet != null)
                {
                    await CoreUnitOfWork.WalletRepository.Delete(wallet);
                    await CoreUnitOfWork.SaveChangesAsync();
                }
            }
        }

        [TestMethod]
        public async Task CreateWalletAlreadyExistsFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210"), $"{ nameof(Wallet) } with jmbg '{0605996781029}' already exists!");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
            finally
            {
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                if (wallet != null)
                {
                    await CoreUnitOfWork.WalletRepository.Delete(wallet);
                    await CoreUnitOfWork.SaveChangesAsync();
                }
            }
        }

        [TestMethod]
        public async Task CreateWalletNotOldEnoughFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605010781029", (short)BankType.BrankoBank, "1234", "123456789876543210"), $"Need to be older than 18 years old to open a wallet!");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }


        [TestMethod]
        public async Task CreateWalletWrongCountryFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration);
                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605996481029", (short)BankType.BrankoBank, "1234", "123456789876543210"), $"Need to be from Serbia to open a wallet!");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task CreateWalletWrongPINLengthFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration);
                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "12345", "123456789876543210"), $"PIN needs to be 4 digits long!");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task CreateWalletBankStatusCheckFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration);
                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605996781027", (short)BankType.BrankoBank, "1234", "123456789876543210"), $"Account not found for given jmbg and pin!");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

    }
}
