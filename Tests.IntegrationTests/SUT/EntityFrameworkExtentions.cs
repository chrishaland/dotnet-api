using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public static class EntityFrameworkExtentions
    {
        public static async Task DeleteAllAsync<T>(this DbContext context) where T : class
        {
            foreach (var p in context.Set<T>())
            {
                context.Entry(p).State = EntityState.Deleted;
            }

            await context.SaveChangesAsync();
        }
    }
}
