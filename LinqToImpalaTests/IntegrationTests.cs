using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using ImpalaToLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
