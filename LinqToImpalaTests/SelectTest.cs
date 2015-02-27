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
    public void TestMethod1()
    {
      var connection = new Mock<DbConnection>();
      var dbContext = new ImpalaDbContext(new ImpalaQueryProvider(connection.Object));

      var query = dbContext.persons;

      Assert.AreEqual("SELECT * FROM Person", query.ToString());
    }
  }
}
