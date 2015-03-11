using System;
using System.Data.Common;
using System.Linq;
using ImpalaToLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LinqToImpalaTests
{
  [TestClass]
  public class OrderByTest
  {
    private ImpalaDbContext _dbContext;

    [TestInitialize]
    public void setup() {
      var connection = new Mock<DbConnection>();
      _dbContext = new ImpalaDbContext(new ImpalaQueryProvider(connection.Object));
    }

    [TestMethod]
    public void ShouldBuildBasicOrderByQuery()
    {
      var query = _dbContext.persons.OrderBy(m => m.Age);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 ORDER BY T0.Age", query.ToString());
    }

    [TestMethod]
    public void ShouldBuildMultipleOrderByQuery()
    {
      var query = _dbContext.persons.OrderBy(m => m.Married).ThenBy(m => m.Age);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 ORDER BY T0.Married T0.Age", query.ToString());
    }

    [TestMethod]
    public void ShouldBuildBasicOrderByDescendingQuery()
    {
      var query = _dbContext.persons.OrderByDescending(m => m.Age);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 ORDER BY T0.Age DESC", query.ToString());
    }

    [TestMethod]
    public void ShouldBuildMultipleOrderByDescendingQuery()
    {
      var query = _dbContext.persons.OrderByDescending(m => m.Married).ThenByDescending(m => m.Age);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 ORDER BY T0.Married DESC T0.Age DESC", query.ToString());
    }

    [TestMethod]
    public void ShouldBuildMixedOrderByAndDescendingQuery()
    {
      var query = _dbContext.persons.OrderBy(m => m.Married).ThenByDescending(m => m.Age);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 ORDER BY T0.Married T0.Age DESC", query.ToString());
    }

    [TestMethod]
    public void ShouldBuildBasicSelectQueryWithDataAttributesOnModel()
    {
      var query = _dbContext.customers.OrderBy(m => m.Name);

      Assert.AreEqual("SELECT T0.* FROM (customer_master_table) T0 ORDER BY T0.dealercustomername", query.ToString());
    }

    [TestMethod]
    public void ShouldBuildOrderByWithWhereClause() {
      var query = _dbContext.persons.Where(m => m.Age > 20).OrderBy(m => m.Married).ThenByDescending(m => m.Age);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Age > 20) ORDER BY T0.Married T0.Age DESC", query.ToString());
    }
  }
}
