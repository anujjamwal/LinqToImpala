﻿using System.Data.Common;
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
    public void TestMethod1() {
      var query = _dbContext.persons.Where(p => p.Name == "Anuj");

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Name = 'Anuj')", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForBooleanField()
    {
      var query = _dbContext.persons.Where(p => p.Married == false);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Married = 'false')", query.ToString());
    }
    
    [TestMethod]
    public void ShouldConstructWhereClauseForIntegerField()
    {
      var query = _dbContext.persons.Where(p => p.Age == 10);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Age = 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructTheIsNullQueryCorrectly() {
      var query = _dbContext.persons.Where(p => p.Name == null);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Name IS NULL)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructTheIsNotNullQueryCorrectly()
    {
      var query = _dbContext.persons.Where(p => p.Name != null);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Name IS NOT NULL)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseWithAndOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age == 30 & p.Married == false);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE ((Age = 30) AND (Married = 'false'))", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseWithAndAlsoOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age == 30 && p.Married == false);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE ((Age = 30) AND (Married = 'false'))", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseWithOrOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age == 30 | p.Married == false);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE ((Age = 30) OR (Married = 'false'))", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseWithOrElseOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age == 30 || p.Married == false);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE ((Age = 30) OR (Married = 'false'))", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForNotEqualOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age != 10);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Age <> 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForLessThanOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age < 10);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Age < 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForLessThanEqualToOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age <= 10);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Age <= 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForGreaterThanOperator()
    {
      var query = _dbContext.persons.Where(p => p.Age > 10);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Age > 10)", query.ToString());
    }

    [TestMethod]
    public void ShouldConstructWhereClauseForGreaterThanEqual()
    {
      var query = _dbContext.persons.Where(p => p.Age >= 10);

      Assert.AreEqual("SELECT * FROM ( SELECT * FROM Person) WHERE (Age >= 10)", query.ToString());
    }
  }
}
