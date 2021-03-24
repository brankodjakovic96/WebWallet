using Common.EfCoreDataAccess;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure.DataAccess.EfCoreDataAccess.Repositories
{
    public class WalletRepository : EfCoreRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(CoreEfCoreDbContext context) : base(context)
        {
        }
    }
}
