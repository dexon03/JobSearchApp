using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Helpers;

public abstract class EfTestBase<TContext> where TContext : DbContext
{
    protected readonly Mock<TContext> MockContext;

    protected EfTestBase()
    {
        MockContext = new Mock<TContext>();
    }

    protected Mock<DbSet<T>> SetupMockDbSet<T>(List<T> data, Expression<Func<TContext, DbSet<T>>> dbSetSelector) where T : class
    {
        var mockSet = EfHelpers.CreateMockDbSet(data);
        MockContext.Setup(dbSetSelector).Returns(mockSet.Object);
        return mockSet;
    }
}
