using System.Data.Common;
using System.Linq;
using ImpalaToLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LinqToImpalaTests
{
  [TestClass]
  public class WhereClauseTest
  {
    [TestMethod]
    public void TestMethod1() {
      var connection = new Mock<DbConnection>();
      var dbContext = new ImpalaDbContext(new ImpalaQueryProvider(connection.Object));

      var query = dbContext.persons.Where(p => p.Name == "Anuj");

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Name = 'Anuj')", query.ToString());
    }
  }
}
