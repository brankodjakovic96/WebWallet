using Common.Utils;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using Core.Domain.Services.Internal.BankRoutingService;
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
        public WalletService(
            ICoreUnitOfWork coreUnitOfWork, 
            IBankRoutingService bankRoutingService,
            IConfiguration configuration
        )
        {
            CoreUnitOfWork = coreUnitOfWork;
            BankRoutingService = bankRoutingService;
            Configuration = configuration;
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
                throw new ArgumentException("No wallet for that jmbg and password pair.");
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
                throw new ArgumentException("No wallet for that jmbg and password pair.");
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

            var password = PasswordGenerator.Generate(6, 2);
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
