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
    ImpalaDbContext _dbContext;

    [TestInitialize]
    public void Setup() {
      var connection = new Mock<DbConnection>();
      _dbContext = new ImpalaDbContext(new ImpalaQueryProvider(connection.Object));
    }

    [TestMethod]
    public void ShouldWhereClauseQueryForStringEqualityCheckFromVariable()
    {
      var name = "Anuj";
      var query = _dbContext.persons.Where(p => p.Name == name);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Name = 'Anuj')", query.ToString());
    }

    [TestMethod]
    public void ShouldWhereClauseQueryForStringEqualityCheck() {
      var query = _dbContext.persons.Where(p => p.Name == "Anuj");

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Name = 'Anuj')", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForBooleanField()
    {
      var query = _dbContext.persons.Where(p => p.Married == false);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Married = 'false')", query.ToString());
    }
    
    [TestMethod]
    public void ShouldConstructWhereClauseForIntegerField()
    {
      var query = _dbContext.persons.Where(p => p.Age == 10);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Age = 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructTheIsNullQueryCorrectly() {
      var query = _dbContext.persons.Where(p => p.Name == null);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Name IS NULL)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructTheIsNotNullQueryCorrectly()
    {
      var query = _dbContext.persons.Where(p => p.Name != null);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Name IS NOT NULL)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseWithAndOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age == 30 & p.Married == false);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE ((T0.Age = 30) AND (T0.Married = 'false'))", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseWithAndAlsoOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age == 30 && p.Married == false);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE ((T0.Age = 30) AND (T0.Married = 'false'))", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseWithOrOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age == 30 | p.Married == false);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE ((T0.Age = 30) OR (T0.Married = 'false'))", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseWithOrElseOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age == 30 || p.Married == false);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE ((T0.Age = 30) OR (T0.Married = 'false'))", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForNotEqualOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age != 10);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Age <> 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForLessThanOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age < 10);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Age < 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForLessThanEqualToOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age <= 10);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Age <= 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForGreaterThanOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age > 10);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Age > 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForGreaterThanEqual()
    {
      var query = _dbContext.persons.Where(p => p.Age >= 10);

      Assert.AreEqual("SELECT T0.* FROM (Person) T0 WHERE (T0.Age >= 10)", query.ToString());
    }
  }

  [TestClass]
  public class WhereClauseWithModelAttributeTest {
    private ImpalaDbContext _dbContext;

    [TestInitialize]
    public void Setup() {
      var connection = new Mock<DbConnection>();
      _dbContext = new ImpalaDbContext(new ImpalaQueryProvider(connection.Object));
    }

    [TestMethod]
    public void ShouldWhereClauseQueryForStringEqualityCheckFromVariable() {
      var name = "Anuj";
      var query = _dbContext.customers.Where(p => p.Name == name);

      Assert.AreEqual("SELECT T0.* FROM (customer_master_table) T0 WHERE (T0.dealercustomername = 'Anuj')", query.ToString());
    }
  }
}
