using Core.Domain.Entities;
using Core.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices
{
    public class WalletService
    {
        private readonly ICoreUnitOfWork CoreUnitOfWork;

        public WalletService(ICoreUnitOfWork coreUnitOfWork)
        {
            CoreUnitOfWork = coreUnitOfWork;
        }


        public async Task<string> CreateWallet(string firstName, string lastName, string jmbg, short bankType, string pin, string bankAccount)
        {
            await ValidateWalletInput(jmbg, bankType, pin, bankAccount);
            var wallet = new Wallet(firstName, lastName, jmbg, (BankType)bankType, pin, bankAccount);

            await CoreUnitOfWork.WalletRepository.Insert(wallet);
            await CoreUnitOfWork.SaveChangesAsync();

            return jmbg;
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
            if (bankAccount.Length != 18)
            {
                throw new ArgumentException("BankAccount has to be 13 digits long.");
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

            if (!Enum.IsDefined(typeof(BankType), bankType))
            {
                throw new ArgumentException("Given BankType doesn't exist.");
            }
        }
    }
}
