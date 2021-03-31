using Core.ApplicationServices;
using Core.ApplicationServices.DTOs;
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
using System.Text;
using System.Threading.Tasks;

namespace Tests.ApplicationServicesTests
{
    [TestClass]
    public class GetWalletInfoTests
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
        public async Task GetWalletInfoNewWalletSuccessTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                WalletInfoDTO wallet = await walletService.GetWalletInfo("0605996781029", password);


                //Assert

                Assert.IsNotNull(wallet, "Wallet must not be null");
                Assert.AreEqual("ime", wallet.FirstName, "FirstName must be 'ime'");
                Assert.AreEqual("prezime", wallet.LastName, "LastName must be 'prezime'");
                Assert.AreEqual("0605996781029", wallet.Jmbg, "Jmbg must be '0605996781029'");
                Assert.AreEqual((short)BankType.BrankoBank, wallet.BankType, $"BankType must be '{BankType.BrankoBank}'");
                Assert.AreEqual("123456789876543210", wallet.BankAccount, "BankAccount must be '123456789876543210'");
                Assert.AreEqual(0, wallet.Balance, "Balance must be 0 RSD");
                Assert.AreEqual(0, wallet.UsedDepositThisMonth, "UsedDepositThisMonth must be 0 RSD");
                Assert.AreEqual(0, wallet.UsedWithdrawThisMonth, "UsedWithdrawThisMonth must be 0 RSD");
                Assert.AreEqual(decimal.Parse(Configuration["MaximalDeposit"]), wallet.MaximalDeposit, $"MaximalDeposit must be {Configuration["MaximalDeposit"]} RSD");
                Assert.AreEqual(decimal.Parse(Configuration["MaximalWithdraw"]), wallet.MaximalWithdraw, $"MaximalWithdraw must be {Configuration["MaximalDeposit"]} RSD");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task GetWalletInfoBlockedWalletSuccessTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                Wallet walletTmp = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");
                walletTmp.Block();
                await CoreUnitOfWork.WalletRepository.Update(walletTmp);
                await CoreUnitOfWork.SaveChangesAsync();

                //Act
                WalletInfoDTO wallet = await walletService.GetWalletInfo("0605996781029", password);


                //Assert

                Assert.IsNotNull(wallet, "Wallet must not be null");
                Assert.AreEqual("ime", wallet.FirstName, "FirstName must be 'ime'");
                Assert.AreEqual("prezime", wallet.LastName, "LastName must be 'prezime'");
                Assert.AreEqual("0605996781029", wallet.Jmbg, "Jmbg must be '0605996781029'");
                Assert.AreEqual((short)BankType.BrankoBank, wallet.BankType, $"BankType must be '{BankType.BrankoBank}'");
                Assert.AreEqual("123456789876543210", wallet.BankAccount, "BankAccount must be '123456789876543210'");
                Assert.AreEqual(0, wallet.Balance, "Balance must be 0 RSD");
                Assert.AreEqual(0, wallet.UsedDepositThisMonth, "UsedDepositThisMonth must be 0 RSD");
                Assert.AreEqual(0, wallet.UsedWithdrawThisMonth, "UsedWithdrawThisMonth must be 0 RSD");
                Assert.AreEqual(decimal.Parse(Configuration["MaximalDeposit"]), wallet.MaximalDeposit, $"MaximalDeposit must be {Configuration["MaximalDeposit"]} RSD");
                Assert.AreEqual(decimal.Parse(Configuration["MaximalWithdraw"]), wallet.MaximalWithdraw, $"MaximalWithdraw must be {Configuration["MaximalDeposit"]} RSD");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task GetWalletInfoWithDepositSuccessTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                await walletService.Deposit("0605996781029", password, 100000M);

                //Act
                WalletInfoDTO wallet = await walletService.GetWalletInfo("0605996781029", password);


                //Assert

                Assert.IsNotNull(wallet, "Wallet must not be null");
                Assert.AreEqual("ime", wallet.FirstName, "FirstName must be 'ime'");
                Assert.AreEqual("prezime", wallet.LastName, "LastName must be 'prezime'");
                Assert.AreEqual("0605996781029", wallet.Jmbg, "Jmbg must be '0605996781029'");
                Assert.AreEqual((short)BankType.BrankoBank, wallet.BankType, $"BankType must be '{BankType.BrankoBank}'");
                Assert.AreEqual("123456789876543210", wallet.BankAccount, "BankAccount must be '123456789876543210'");
                Assert.AreEqual(100000M, wallet.Balance, "Balance must be 0 RSD");
                Assert.AreEqual(100000M, wallet.UsedDepositThisMonth, "UsedDepositThisMonth must be 0 RSD");
                Assert.AreEqual(0, wallet.UsedWithdrawThisMonth, "UsedWithdrawThisMonth must be 0 RSD");
                Assert.AreEqual(decimal.Parse(Configuration["MaximalDeposit"]), wallet.MaximalDeposit, $"MaximalDeposit must be {Configuration["MaximalDeposit"]} RSD");
                Assert.AreEqual(decimal.Parse(Configuration["MaximalWithdraw"]), wallet.MaximalWithdraw, $"MaximalWithdraw must be {Configuration["MaximalDeposit"]} RSD");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task GetWalletInfoWithDepositAndWithdrawSuccessTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                await walletService.Deposit("0605996781029", password, 100000M);
                await walletService.Withdraw("0605996781029", password, 50000M);

                //Act
                WalletInfoDTO wallet = await walletService.GetWalletInfo("0605996781029", password);


                //Assert

                Assert.IsNotNull(wallet, "Wallet must not be null");
                Assert.AreEqual("ime", wallet.FirstName, "FirstName must be 'ime'");
                Assert.AreEqual("prezime", wallet.LastName, "LastName must be 'prezime'");
                Assert.AreEqual("0605996781029", wallet.Jmbg, "Jmbg must be '0605996781029'");
                Assert.AreEqual((short)BankType.BrankoBank, wallet.BankType, $"BankType must be '{BankType.BrankoBank}'");
                Assert.AreEqual("123456789876543210", wallet.BankAccount, "BankAccount must be '123456789876543210'");
                Assert.AreEqual(50000M, wallet.Balance, "Balance must be 0 RSD");
                Assert.AreEqual(100000M, wallet.UsedDepositThisMonth, "UsedDepositThisMonth must be 0 RSD");
                Assert.AreEqual(50000M, wallet.UsedWithdrawThisMonth, "UsedWithdrawThisMonth must be 0 RSD");
                Assert.AreEqual(decimal.Parse(Configuration["MaximalDeposit"]), wallet.MaximalDeposit, $"MaximalDeposit must be {Configuration["MaximalDeposit"]} RSD");
                Assert.AreEqual(decimal.Parse(Configuration["MaximalWithdraw"]), wallet.MaximalWithdraw, $"MaximalWithdraw must be {Configuration["MaximalDeposit"]} RSD");
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }


        [TestMethod]
        public async Task GetWalletInfoWalletDoesntExistFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.GetWalletInfo("0605996781029", "1234"), $"No wallet for entered jmbg '{"0605996781029"}' and password pair.");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }
    }
}
