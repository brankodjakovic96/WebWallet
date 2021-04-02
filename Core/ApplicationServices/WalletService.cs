using Common.Utils;
using Core.ApplicationServices.DTOs;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using Core.Domain.Services.Internal.BankRoutingService;
using Core.Domain.Services.Internal.FeeService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices
{
    public class WalletService
    {
        private readonly ICoreUnitOfWork CoreUnitOfWork;
        private readonly IBankRoutingService BankRoutingService;
        private readonly IConfiguration Configuration;
        private readonly IFeeService FeeService;
        public WalletService(
            ICoreUnitOfWork coreUnitOfWork,
            IBankRoutingService bankRoutingService,
            IConfiguration configuration,
            IFeeService feeService
        )
        {
            CoreUnitOfWork = coreUnitOfWork;
            BankRoutingService = bankRoutingService;
            Configuration = configuration;
            FeeService = feeService;
        }

        public async Task<WalletInfoDTO> GetWalletInfo(string jmbg, string password)
        {
            Wallet wallet = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                    wallet => wallet.Jmbg == jmbg,
                    wallet => wallet.Transactions
                );
            if (wallet == null || !wallet.CheckPassword(password))
            {
                throw new ArgumentException($"No wallet for entered jmbg '{jmbg}' and password pair.");
            }

            wallet.CheckAndUpdateUsedDepositWithdraw();
            await CoreUnitOfWork.WalletRepository.Update(wallet);
            await CoreUnitOfWork.SaveChangesAsync();

            decimal maximalDeposit;
            decimal maximalWithdraw;
            bool success = decimal.TryParse(Configuration["MaximalDeposit"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out maximalDeposit);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["MaximalDeposit"]} (MaximalDeposit) to decimal");
            }
            success = decimal.TryParse(Configuration["MaximalWithdraw"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out maximalWithdraw);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["MaximalWithdraw"]} (MaximalWithdraw) to decimal");
            }
            var walletInfoDTO = new WalletInfoDTO()
            {
                Jmbg = wallet.Jmbg,
                FirstName = wallet.FirstName,
                LastName = wallet.LastName,
                BankType = (short)wallet.BankType,
                BankAccount = wallet.BankAccount,
                Balance = wallet.Balance,
                UsedDepositThisMonth = wallet.UsedDepositThisMonth,
                MaximalDeposit = maximalDeposit,
                UsedWithdrawThisMonth = wallet.UsedWithdrawThisMonth,
                MaximalWithdraw = maximalWithdraw,
                IsBlocked = wallet.IsBlocked,
                TransactionDTOs = wallet.Transactions.Select(
                    transaction => new TransactionDTO() {
                        Amount = transaction.Amount,
                        Destination = transaction.Destination,
                        Source = transaction.Source,
                        TransactionDateTime = transaction.TransactionDateTime,
                        Type = (short)transaction.Type,
                        WalletBalance = transaction.WalletBalance
                    }
                ).ToList()
            };

            return walletInfoDTO;
        }

        public async Task BlockWallet(string jmbg, string adminPassword)
        {
            if (adminPassword != Configuration["AdminPassword"])
            {
                throw new ArgumentException($"Wrong admin password.");

            }

            Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById(jmbg);
            if (wallet == null)
            {
                throw new ArgumentException($"No wallet for entered jmbg '{jmbg}'.");
            }

            wallet.Block();
            await CoreUnitOfWork.WalletRepository.Update(wallet);
            await CoreUnitOfWork.SaveChangesAsync();
        }

        public async Task UnblockWallet(string jmbg, string adminPassword)
        {
            if (adminPassword != Configuration["AdminPassword"])
            {
                throw new ArgumentException($"Wrong admin password.");

            }

            Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById(jmbg);
            if (wallet == null)
            {
                throw new ArgumentException($"No wallet for entered jmbg '{jmbg}'.");
            }

            wallet.Unblock();
            await CoreUnitOfWork.WalletRepository.Update(wallet);
            await CoreUnitOfWork.SaveChangesAsync();
        }

        public async Task ChangePassword(string jmbg, string oldPassword, string newPassword, string newPasswordConfirmation)
        {
            Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById(jmbg);
            if (wallet == null || !wallet.CheckPassword(oldPassword))
            {
                throw new ArgumentException($"No wallet for entered jmbg '{jmbg}' and password pair.");
            }

            wallet.ChangePassword(oldPassword, newPassword, newPasswordConfirmation);
            await CoreUnitOfWork.WalletRepository.Update(wallet);
            await CoreUnitOfWork.SaveChangesAsync();
        }


        public async Task<decimal> CalculateTransferFee(string jmbg, string password, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be higher than 0 RSD.");
            }
            Wallet wallet = await CoreUnitOfWork.WalletRepository.GetById(jmbg);
            if (wallet == null || !wallet.CheckPassword(password))
            {
                throw new ArgumentException($"No wallet for entered jmbg '{jmbg}' and password pair.");
            }

            decimal fee = CalculateFee(wallet, amount);
            return fee;
        }

        private decimal CalculateFee(Wallet wallet, decimal amount)
        {
            decimal fixedFeeLimit;
            decimal fixedFee;
            decimal percentageFee;
            int numberOfDaysAfterCreationWithNoFee;
            bool firstTransactionFreeEachMonth;

            bool success = decimal.TryParse(Configuration["FixedFeeLimit"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out fixedFeeLimit);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["FixedFeeLimit"]} (FixedFeeLimit) to decimal");
            }

            success = decimal.TryParse(Configuration["FixedFee"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out fixedFee);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["FixedFee"]} (FixedFee) to decimal");
            }

            success = decimal.TryParse(Configuration["PercentageFee"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out percentageFee);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["PercentageFee"]} (PercentageFee) to decimal");
            }

            success = bool.TryParse(Configuration["FirstTransactionFreeEachMonth"], out firstTransactionFreeEachMonth);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["FirstTransactionFreeEachMonth"]} (FirstTransactionFreeEachMonth) to bool");
            }

            success = int.TryParse(Configuration["NumberOfDaysAfterCreationWithNoFee"], out numberOfDaysAfterCreationWithNoFee);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["NumberOfDaysAfterCreationWithNoFee"]} (NumberOfDaysAfterCreationWithNoFee) to int");
            }

            decimal fee = FeeService.CalculateFee(numberOfDaysAfterCreationWithNoFee, firstTransactionFreeEachMonth, fixedFeeLimit, fixedFee, percentageFee, wallet, amount);
            return fee;
        }

        public async Task Transfer(string sourceJmbg, string sourcePassword, string desitnationJmbg, decimal amount)
        {
            if (sourceJmbg == desitnationJmbg)
            {
                throw new ArgumentException("Source and destination Jmbg must be different.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be higher than 0 RSD.");
            }
            Wallet walletSource = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                    wallet => wallet.Jmbg == sourceJmbg,
                    wallet => wallet.Transactions
                );
            if (walletSource == null || !walletSource.CheckPassword(sourcePassword))
            {
                throw new ArgumentException($"No wallet for entered jmbg '{sourceJmbg}' and password pair.");
            }

            if (walletSource.IsBlocked)
            {
                throw new InvalidOperationException($"Wallet '{sourceJmbg}' is blocked");
            }

            Wallet walletDestination = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                    wallet => wallet.Jmbg == desitnationJmbg,
                    wallet => wallet.Transactions
                );
            if (walletDestination == null)
            {
                throw new ArgumentException($"No wallet for jmbg '{desitnationJmbg}'.");
            }

            decimal maximalDeposit;
            decimal maximalWithdraw;
            bool success = decimal.TryParse(Configuration["MaximalDeposit"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out maximalDeposit);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["MaximalDeposit"]} (MaximalDeposit) to decimal");
            }
            success = decimal.TryParse(Configuration["MaximalWithdraw"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out maximalWithdraw);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["MaximalWithdraw"]} (MaximalWithdraw) to decimal");
            }

            decimal fee = CalculateFee(walletSource, amount);

            await CoreUnitOfWork.BeginTransactionAsync();

            try
            {
                walletSource.PayOut(amount, TransactionType.TransferPayOut, desitnationJmbg, maximalWithdraw);
                await CoreUnitOfWork.WalletRepository.Update(walletSource);
                await CoreUnitOfWork.SaveChangesAsync();

                if (fee > 0)
                {
                    walletSource.PayOut(fee, TransactionType.FeePayOut, "System", maximalWithdraw);
                    await CoreUnitOfWork.WalletRepository.Update(walletSource);
                    await CoreUnitOfWork.SaveChangesAsync();
                }

                walletDestination.PayIn(amount, TransactionType.TransferPayIn, sourceJmbg, maximalDeposit);
                await CoreUnitOfWork.WalletRepository.Update(walletDestination);
                await CoreUnitOfWork.SaveChangesAsync();

                await CoreUnitOfWork.CommitTransactionAsync();
            }
            catch (InvalidOperationException ex)
            {
                await CoreUnitOfWork.RollbackTransactionAsync();
                throw ex;
            }
        }

        public async Task Deposit(string jmbg, string password, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be higher than 0 RSD.");
            }
            Wallet wallet = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                    wallet => wallet.Jmbg == jmbg,
                    wallet => wallet.Transactions
                );
            if (wallet == null || !wallet.CheckPassword(password))
            {
                throw new ArgumentException($"No wallet for entered jmbg '{jmbg}' and password pair.");
            }

            if (wallet.IsBlocked)
            {
                throw new InvalidOperationException($"Wallet '{jmbg}' is blocked");
            }

            decimal maximalDeposit;
            bool success = decimal.TryParse(Configuration["MaximalDeposit"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out maximalDeposit);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["MaximalDeposit"]} (MaximalDeposit) to decimal");
            }

            await CoreUnitOfWork.BeginTransactionAsync();

            try
            {
                wallet.PayIn(amount, TransactionType.Deposit, wallet.BankType.ToString(), maximalDeposit);
                await CoreUnitOfWork.WalletRepository.Update(wallet);
                await CoreUnitOfWork.SaveChangesAsync();
                var withdrawResponse = await BankRoutingService.Withdraw(jmbg, wallet.PIN, amount, wallet.BankType);
                if (!withdrawResponse.Status)
                {
                    throw new InvalidOperationException(withdrawResponse.ErrorCodes);
                }

                await CoreUnitOfWork.CommitTransactionAsync();
            }
            catch (InvalidOperationException ex)
            {
                await CoreUnitOfWork.RollbackTransactionAsync();
                throw ex;
            }
        }

        public async Task Withdraw(string jmbg, string password, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be higher than 0 RSD.");
            }
            Wallet wallet = await CoreUnitOfWork.WalletRepository.GetFirstOrDefaultWithIncludes(
                    wallet => wallet.Jmbg == jmbg,
                    wallet => wallet.Transactions
                );
            if (wallet == null || !wallet.CheckPassword(password))
            {
                throw new ArgumentException($"No wallet for entered jmbg '{jmbg}' and password pair.");
            }

            if (wallet.IsBlocked)
            {
                throw new InvalidOperationException($"Wallet '{jmbg}' is blocked");
            }

            decimal maximalWithdraw;
            bool success = decimal.TryParse(Configuration["MaximalWithdraw"], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out maximalWithdraw);
            if (!success)
            {
                throw new ArgumentException($"Couldn't cast {Configuration["MaximalWithdraw"]} (MaximalWithdraw) to decimal");
            }

            await CoreUnitOfWork.BeginTransactionAsync();

            try
            {
                wallet.PayOut(amount, TransactionType.Withdraw, wallet.BankType.ToString(), maximalWithdraw);
                await CoreUnitOfWork.WalletRepository.Update(wallet);
                await CoreUnitOfWork.SaveChangesAsync();
                var withdrawResponse = await BankRoutingService.Deposit(jmbg, wallet.PIN, amount, wallet.BankType);
                if (!withdrawResponse.Status)
                {
                    throw new InvalidOperationException(withdrawResponse.ErrorCodes);
                }

                await CoreUnitOfWork.CommitTransactionAsync();
            }
            catch (InvalidOperationException ex)
            {
                await CoreUnitOfWork.RollbackTransactionAsync();
                throw ex;
            }
        }

        public async Task<string> CreateWallet(string firstName, string lastName, string jmbg, short bankType, string pin, string bankAccount)
        {
            await ValidateWalletInput(jmbg, bankType, pin, bankAccount);

            var password = PasswordGenerator.Generate(6);
            var wallet = new Wallet(firstName, lastName, jmbg, (BankType)bankType, pin, bankAccount, password);

            await CoreUnitOfWork.WalletRepository.Insert(wallet);
            await CoreUnitOfWork.SaveChangesAsync();

            return password;
        }

        private async Task ValidateWalletInput(string jmbg, short bankType, string pin, string bankAccount)
        {
            if (pin.Length != 4)
            {
                throw new ArgumentException("Pin has to be 4 digits long.");
            }
            if (pin.Any(c => !char.IsDigit(c)))
            {
                throw new ArgumentException("Pin has to be only made out of digits.");
            }
            if (jmbg.Length != 13)
            {
                throw new ArgumentException("Jmbg has to be 13 digits long.");
            }
            if (jmbg.Any(c => !char.IsDigit(c)))
            {
                throw new ArgumentException("Jmbg has to be only made out of digits.");
            }
            if (bankAccount.Length > 18)
            {
                throw new ArgumentException("BankAccount has to be 18 or less digits long.");
            }
            if (bankAccount.Any(c => !char.IsDigit(c)))
            {
                throw new ArgumentException("BankAccount has to be only made out of digits.");
            }

            var day = jmbg.Substring(0, 2);
            var month = jmbg.Substring(2, 2);
            var year = int.Parse(jmbg.Substring(4, 3));
            if (year > 900) { year += 1000; }
            else { year += 2000; }
            var dateOfBirth = DateTime.Parse($"{year}-{month}-{day}");
            var isOldEnough = DateTime.Now.Date >= dateOfBirth.AddYears(18).Date;
            if (!isOldEnough)
            {
                throw new ArgumentException("Need to be older than 18 to open a wallet.");
            }

            var region = int.Parse(jmbg.Substring(7, 2));

            if (region < 70)
            {
                throw new ArgumentException("Need to be a citizen of Serbia to open a wallet.");
            }

            var wallet = await CoreUnitOfWork.WalletRepository.GetById(jmbg);
            if (wallet != null)
            {
                throw new ArgumentException("Wallet for that jmbg already exists.");
            }

            if (!Enum.IsDefined(typeof(BankType), bankType) && (BankType)bankType != BankType.Undefined)
            {
                throw new ArgumentException("Given BankType doesn't exist.");
            }

            var accountStatus = await BankRoutingService.CheckStatus(jmbg, pin, (BankType)bankType);
            if (!accountStatus.Status)
            {
                throw new ArgumentException(accountStatus.ErrorCodes);
            }
        }
    }
}
