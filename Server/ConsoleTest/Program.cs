using GameDB;
using Microsoft.EntityFrameworkCore;

namespace ConsoleTest
{
    // Create Read Update Delete
    internal class Program
    {
        static void Main(string[] args)
        {
            //using (GameDbContext db = new GameDbContext())
            //{
            //    TestDb testDb = new TestDb();
            //    testDb.Name = "Rookiss";

            //    db.Tests.Add(testDb);

            //    db.SaveChanges();
            //}

            //using (GameDbContext db = new GameDbContext())
            //{
            //    TestDb? findDb = db.Tests.FirstOrDefault(t => t.Name == "Rookiss");
            //    if (findDb != null)
            //    {
            //        int check = findDb.TestDbId;
            //    }
            //}

            //using (GameDbContext db = new GameDbContext())
            //{
            //    TestDb? findDb = db.Tests.FirstOrDefault(t => t.Name == "Rookiss");
            //    if (findDb != null)
            //    {
            //        findDb.Name = "Handsome Rookiss";
            //        db.SaveChanges();
            //    }
            //}

            using (GameDbContext db = new GameDbContext())
            {
                // search n delete
                TestDb? findDb = db.Tests.FirstOrDefault(a => a.Name.Contains("Rookiss"));
                if (findDb != null)
                {
                    db.Tests.Remove(findDb);
                    db.SaveChanges();
                }

                // Entity Tracking
                {
                    TestDb testDb = new TestDb()
                    {
                        TestDbId = 1
                    };

                    var entry = db.Tests.Entry(testDb);
                    entry.State = EntityState.Deleted;
                    db.SaveChanges();
                }

                // EF Core 7
                {
                    db.Tests.Where(a => a.Name.Contains("Rookiss")).ExecuteDelete();
                }
            }
        }
    }
}
