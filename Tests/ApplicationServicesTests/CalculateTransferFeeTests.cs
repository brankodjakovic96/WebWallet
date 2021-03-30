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
using System.Text;
using System.Threading.Tasks;

namespace Tests.ApplicationServicesTests
{
    [TestClass]
    public class CalculateTransferFeeTests
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
        public async Task CalculateTransferFeeFixedFeeSuccessTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                decimal fee1 = await walletService.CalculateTransferFee("0605996781029", password, 1);
                decimal fee2 = await walletService.CalculateTransferFee("0605996781029", password, 5000);
                decimal fee3 = await walletService.CalculateTransferFee("0605996781029", password, 9999);

                //Assert
                Assert.AreEqual(100, fee1, "Fee1 must be 100.00 RSD.");
                Assert.AreEqual(100, fee2, "Fee2 must be 100.00 RSD.");
                Assert.AreEqual(100, fee3, "Fee3 must be 100.00 RSD.");

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
        public async Task CalculateTransferFeePercentageFeeSuccessTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                decimal fee1 = await walletService.CalculateTransferFee("0605996781029", password, 10000);
                decimal fee2 = await walletService.CalculateTransferFee("0605996781029", password, 100000);
                decimal fee3 = await walletService.CalculateTransferFee("0605996781029", password, 1000000);

                //Assert
                Assert.AreEqual(100, fee1, "Fee1 must be 100.00 RSD.");
                Assert.AreEqual(1000, fee2, "Fee2 must be 1000.00 RSD.");
                Assert.AreEqual(10000, fee3, "Fee3 must be 10000.00 RSD.");

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
        public async Task CalculateTransferFeeFirstTransferInMonthNoFeeSuccessTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");

                //Act
                decimal fee1 = await walletService.CalculateTransferFee("0605996781029", password, 10000);

                var date = new SqlParameter("@LastTransferDateTime", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                var query = DbContext.Database
                    .ExecuteSqlRaw("UPDATE Wallets SET LastTransferDateTime = @LastTransferDateTime", date);
                DbContext.Entry(wallet).Reload();

                decimal fee2 = await walletService.CalculateTransferFee("0605996781029", password, 100000);

                //Assert
                Assert.AreEqual(0, fee1, "Fee1 must be 0.00 RSD.");
                Assert.AreEqual(1000, fee2, "Fee2 must be 1000.00 RSD.");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task CalculateTransferFeeNoFeeDaysXAfterWalletCreatedSuccessTest()
        {
            try
            {
                //Arrange
                Configuration["FirstTransactionFreeEachMonth"] = "False";
                Configuration["NumberOfDaysAfterCreationWithNoFee"] = "7";

                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");
                Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById("0605996781029");

                //Act
                decimal fee1 = await walletService.CalculateTransferFee("0605996781029", password, 10000);
                decimal fee2 = await walletService.CalculateTransferFee("0605996781029", password, 100000);

                var date = new SqlParameter("@WalletCreatedDateTime", $"{DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss")}");
                var query = DbContext.Database
                    .ExecuteSqlRaw("UPDATE Wallets SET WalletCreatedDateTime = @WalletCreatedDateTime", date);
                DbContext.Entry(wallet).Reload();

                decimal fee3 = await walletService.CalculateTransferFee("0605996781029", password, 1000000);

                //Assert
                Assert.AreEqual(0, fee1, "Fee1 must be 0.00 RSD.");
                Assert.AreEqual(0, fee2, "Fee2 must be 0.00 RSD.");
                Assert.AreEqual(10000, fee3, "Fee3 must be 10000.00 RSD.");

            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
            finally
            {
                Configuration["FirstTransactionFreeEachMonth"] = "True";
                Configuration["NumberOfDaysAfterCreationWithNoFee"] = "0";
            }
        }





        [TestMethod]
        public async Task CalulateTransferFeeNoWalletFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CalculateTransferFee("0605996781029", "1234", 1100000M), $"No wallet for entered jmbg '{"0605996781028"}' and password pair."); ;
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task CalulateTransferFeeWrongPasswordFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CalculateTransferFee("0605996781029", "12345", 1100000M), $"No wallet for entered jmbg '{"0605996781029"}' and password pair."); ;
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }

        [TestMethod]
        public async Task CalulateTransferFeeLessThanOrEqualToZeroAmountFailTest()
        {
            try
            {
                //Arrange
                var walletService = new WalletService(CoreUnitOfWork, BankRoutingService, Configuration, FeeService);
                string password = await walletService.CreateWallet("ime", "prezime", "0605996781029", (short)BankType.BrankoBank, "1234", "123456789876543210");

                //Act
                //Assert
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await walletService.CalculateTransferFee("0605996781029", password, 0M), "Amount must be higher than 0 RSD."); ;
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected error: " + ex.Message);
            }
        }
    }
}
