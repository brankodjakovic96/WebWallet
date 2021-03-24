using Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Repositories
{
    public interface ICoreUnitOfWork : IUnitOfWork
    {
        IWalletRepository WalletRepository { get; }
        ITransactionRepository TransactionRepository { get; }
    }
}
