using Microsoft.EntityFrameworkCore;
using Repository;

namespace Tests.UnitTests;

public class TestDatabase : DatabaseContext
{
    public TestDatabase() : base(new DbContextOptionsBuilder<DatabaseContext>().UseInMemoryDatabase("Database").Options)
    {
    }
}
