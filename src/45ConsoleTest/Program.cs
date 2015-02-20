using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;

using Psns.Common.Persistence.Definitions;
using Psns.Common.Persistence.EntityFramework;

namespace _45ConsoleTest
{
    public class Related : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Model : IIdentifiable
    {
        public Model()
        {
            Related = new List<Related>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Related> Related { get; set; }
    }

    public class ModelContext : Context
    {
        public DbSet<Model> Models { get; set; }
        public DbSet<Related> Relateds { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var changeCount = 0;

            using(var repository = new Repository<Model>(new ModelContext()))
            {
                var created = repository.Create(new Model
                {
                    Name = "created one",
                    Related = new List<Related>
                    {
                        new Related { Name = "One" },
                        new Related { Name = "Two" }
                    }
                });

                changeCount += repository.SaveChanges();
            }

            using(var repository = new Repository<Model>(new ModelContext()))
            {
                var modified = repository.Update(new Model
                {
                    Id = 1,
                    Name = "updated one",
                    Related = new List<Related>
                    {
                        new Related { Id = 2, Name = "Two" }    
                    }
                }, "Related");
                changeCount += repository.SaveChanges();
            }

            Console.WriteLine(changeCount);
            Console.ReadKey();
        }
    }
}
