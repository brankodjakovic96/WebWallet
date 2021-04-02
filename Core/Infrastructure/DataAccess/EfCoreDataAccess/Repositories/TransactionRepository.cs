using Common.EfCoreDataAccess;
using Core.Domain.Entities;
using Core.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Infrastructure.DataAccess.EfCoreDataAccess.Repositories
{
    public class TransactionRepository : EfCoreRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(CoreEfCoreDbContext context) : base(context)
        {
        }
    }
}
