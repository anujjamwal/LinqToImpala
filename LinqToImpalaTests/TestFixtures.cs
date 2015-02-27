using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using ImpalaToLinq;
using ImpalaToLinq.ImpalaToLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToImpalaTests 
{
  class Config
  {
    public readonly static Boolean RunIntegration = false;
  }

  class IntegrationTestRunner : IDisposable {
    public  ImpalaDbContext DbContext { get; private set; }
    public DbConnection Connection { get; private set; }
    public IntegrationTestRunner() {
      if (!Config.RunIntegration) {
        Assert.Inconclusive("Integration Tests Ignored. Check the [RunIntegration] property in [Config] class in [TestFixtures.cs] file");
      }
      Connection = new OdbcConnection("DSN=olga_impala");
      Connection.Open();
      DbContext = new ImpalaDbContext(new ImpalaQueryProvider(Connection));
    }

    public void Dispose() {
      Connection.Dispose();
    }
  }

  class Person {
    public string ID { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public bool Married { get; set; }
  }

  [Table("customer_master_table")]
  class Customer
  {
    [Column("customernumber")]
    public string ID { get; set; }
    [Column("dealercustomername")]
    public string Name { get; set; }
  }


  class ImpalaDbContext {
    public ImpalaQueryable<Person> persons { get; private set; }
    public ImpalaQueryable<Customer> customers { get; private set; }

    public ImpalaDbContext(IQueryProvider queryProvider) {
      persons = new ImpalaQueryable<Person>(queryProvider);
      customers = new ImpalaQueryable<Customer>(queryProvider);
    }
  }
}
