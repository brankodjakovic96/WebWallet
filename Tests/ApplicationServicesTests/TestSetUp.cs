using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.ApplicationServicesTests
{
    [TestClass]
    public class TestSetUp
    {

        [AssemblyInitialize()]
        public static async Task AssemblyInit(TestContext context)
        {
            var dbContextFactory = new DbContextFactory();
            using (var dbContext = dbContextFactory.CreateDbContext(new string[] { }))
            {
                await dbContext.Database.EnsureCreatedAsync();
            }
        }

        [AssemblyCleanup()]
        public static async Task AssemblyCleanup()
        {
            var dbContextFactory = new DbContextFactory();
            using (var dbContext = dbContextFactory.CreateDbContext(new string[] { }))
            {
                await dbContext.Database.EnsureDeletedAsync();
            }
        }
    }
}
