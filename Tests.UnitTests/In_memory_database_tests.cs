namespace Tests.UnitTests;

public class In_memory_database_tests
{
    [Test]
    public async Task Create_empty_in_memory_database()
    {
        var database = new TestDatabase();
        await database.SaveChangesAsync();
        Assert.Pass();
    }
}
