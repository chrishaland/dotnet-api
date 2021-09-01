using Microsoft.EntityFrameworkCore;
using Repository;

namespace Tests.UnitTests
{
    public class TestDatabase : Database
    {
        public TestDatabase() : base(new DbContextOptionsBuilder<Database>().UseInMemoryDatabase("Database").Options)
        {
        }
    }
}
