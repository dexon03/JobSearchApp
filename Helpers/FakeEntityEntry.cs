using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Helpers;

public class FakeEntityEntry<T> : EntityEntry<T> where T : class
{
    public override T Entity { get; }

    public FakeEntityEntry(T entity) : base(null)
    {
        Entity = entity;
    }
}
