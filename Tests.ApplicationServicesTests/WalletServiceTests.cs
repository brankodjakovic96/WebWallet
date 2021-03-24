using Core.ApplicationServices;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using Core.Infrastructure.DataAccess.EfCoreDataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Tests.ApplicationServicesTests
{
    [TestClass]
    public class WalletServiceTests
    {

        private ICoreUnitOfWork CoreUnitOfWork;
        private CoreEfCoreDbContext DbContext;

        [TestInitialize]
        public void Setup()
        {
            var dbContextFactory = new DbContextFactory();
            DbContext = dbContextFactory.CreateDbContext(new string[] { });
            CoreUnitOfWork = new CoreEfCoreUnitOfWork(DbContext);
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
                var walletService = new WalletService(CoreUnitOfWork);

                //Act
                string walletId = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1111", "123456789876543210");

                //Assert
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById(walletId);

                Assert.IsNotNull(wallet, "Wallet must not be null");
                Assert.AreEqual("ime", wallet.FirstName, "FirstName must be 'ime'");
                Assert.AreEqual("prezime", wallet.LastName, "LastName must be 'prezime'");
                Assert.AreEqual("0605996781029", wallet.Jmbg, "Jmbg must be '0605996781029'");
                Assert.AreEqual(BankType.BrankoBank, wallet.BankType, $"BankType must be '{BankType.BrankoBank}'");
                Assert.AreEqual("1111", wallet.PIN, "PIN must be '1111'");
                Assert.AreEqual("123456789876543210", wallet.BankAccount, "BankAccount must be '123456789876543210'");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
            finally
            {
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                await CoreUnitOfWork.WalletRepository.Delete(wallet);
                await CoreUnitOfWork.SaveChangesAsync();
            }
        }

        [TestMethod]
        public async Task CreateWalletAlreadyExistsFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork);
                string walletId = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1111", "123456789876543210");

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1111", "123456789876543210"), $"{ nameof(Wallet) } with jmbg '{walletId}' already exists!");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
            finally
            {
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                await CoreUnitOfWork.WalletRepository.Delete(wallet);
                await CoreUnitOfWork.SaveChangesAsync();
            }
        }

        [TestMethod]
        public async Task CreateWalletNotOldEnoughFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605010781029", (short)BankType.BrankoBank, "1111", "123456789876543210"), $"Need to be older than 18 years old to open a wallet!");

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
                var walletService = new WalletService(CoreUnitOfWork);
                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605996481029", (short)BankType.BrankoBank, "1111", "123456789876543210"), $"Need to be from Serbia to open a wallet!");

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
                var walletService = new WalletService(CoreUnitOfWork);
                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "11111", "123456789876543210"), $"PIN needs to be 4 digits long!");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

    }
}
