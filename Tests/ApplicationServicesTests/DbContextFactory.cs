using Core.Infrastructure.DataAccess.EfCoreDataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.ApplicationServicesTests
{
    public class DbContextFactory : IDesignTimeDbContextFactory<CoreEfCoreDbContext>
    {
        public CoreEfCoreDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<CoreEfCoreDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=testDB;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;

            return new CoreEfCoreDbContext(options);
        }
    }
}
