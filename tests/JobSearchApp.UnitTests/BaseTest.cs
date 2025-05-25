// using JobSearchApp.Data;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace JobSearchApp.UnitTests;
//
// public class BaseTest : IDisposable
// {
//     private readonly IAppDbContext db;
//     private readonly IDbContextFactory<AppDbContext> dbContextFactory;
//     public IAppDbContext db
//     {
//         get
//         {
//             return this.db;
//         }
//     }
//
//     public IDbContextFactory<AppDbContext> DBContextFactory
//     {
//         get
//         {
//             return this.dbContextFactory;
//         }
//     }
//
//     public BaseTest()
//     {
//         var services = new ServiceCollection();
//
//         services.AddDbContextFactory<AppDbContext>(options => 
//             options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
//
//         var serviceProvider = services.BuildServiceProvider();
//
//         this.dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
//         this.db = this.dbContextFactory.CreateDbContext();
//     }
//
//     public void Dispose()
//     {
//         if (db != null)
//         {
//             db.Dispose();
//         }
//     }
// }
