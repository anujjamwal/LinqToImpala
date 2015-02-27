using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToImpalaTests
{
  [TestClass]
  public class IntegrationTests
  {
    [TestMethod]
    public void TestWhereClause() {
      using(var runner = new IntegrationTestRunner()) {
        var query = runner.DbContext.persons.Where(m => m.Age == 10).ToList();
      } 
    }
  }
}
