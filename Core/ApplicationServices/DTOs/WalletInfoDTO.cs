using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ApplicationServices.DTOs
{
    public class WalletInfoDTO
    {
        public string Jmbg { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BankAccount { get; set; }
        public short BankType { get; set; }
        public decimal Balance { get; set; }
        public decimal UsedDepositThisMonth { get; set; }
        public decimal MaximalDeposit { get; set; }
        public decimal UsedWithdrawThisMonth { get; set; }
        public decimal MaximalWithdraw { get; set; }
        public IList<TransactionDTO> TransactionDTOs { get; set; }
    }
}
