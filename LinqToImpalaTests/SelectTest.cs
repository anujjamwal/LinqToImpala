using System;
using System.Data.Common;
using System.Linq;
using ImpalaToLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LinqToImpalaTests
{
  [TestClass]
  public class SelectTest
  {

    [TestMethod]
    public void ShouldBuildBasicSelectQuery()
    {
      var connection = new Mock<DbConnection>();
      var dbContext = new ImpalaDbContext(new ImpalaQueryProvider(connection.Object));

      var query = dbContext.persons;

      Assert.AreEqual("SELECT T0.* FROM Person T0", query.ToString());
    }

    [TestMethod]
    public void ShouldBuildBasicSelectQueryWithDataAttributesOnModel()
    {
      var connection = new Mock<DbConnection>();
      var dbContext = new ImpalaDbContext(new ImpalaQueryProvider(connection.Object));

      var query = dbContext.customers;

      Assert.AreEqual("SELECT T0.* FROM customer_master_table T0", query.ToString());
    }
  }
}
