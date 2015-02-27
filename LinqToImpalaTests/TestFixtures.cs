using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ImpalaToLinq.ImpalaToLinq;

namespace LinqToImpalaTests 
{
  [Table("Persons")]
  class Person {
    public string ID { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public bool Married { get; set; }
  }

  class ImpalaDbContext {
    public ImpalaQueryable<Person> persons { get; private set; }

    public ImpalaDbContext(IQueryProvider queryProvider) {
      persons = new ImpalaQueryable<Person>(queryProvider);
    }
  }
}
