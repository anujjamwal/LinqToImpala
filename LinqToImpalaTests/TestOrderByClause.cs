using System;
using System.Data.Common;
using System.Linq;
using ImpalaToLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LinqToImpalaTests
{
  [TestClass]
  public class TestOrderByClause
  {
    ImpalaDbContext _dbContext;

    [TestInitialize]
    public void Setup()
    {
      var connection = new Mock<DbConnection>();
      _dbContext = new ImpalaDbContext(new ImpalaQueryProvider(connection.Object));
    }

    [TestMethod]
    public void ShouldBuildQueryWithOrderByClause()
    {
      var query = _dbContext.persons.OrderBy(m => m.Name);

      Assert.AreEqual("SELECT T0.* FROM ( SELECT T1.* FROM Person T1) T0 WHERE (T0.Name = 'Anuj')", query.ToString());
    }
  }
}
