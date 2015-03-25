// build action = "compile"

using System;
using System.Collections;
using System.Collections.Generic;
using FluentNHibernate.Testing;
using NHibernate;
using NUnit.Framework;
using PIMS.Web.Api;
using PIMS.Core.Models;


namespace PIMS.IntegrationTest
{
    [TestFixture]
    public class VerifyThatNHibernateCan
    {
        /*
         * 11/1/2013:
         * Initial tests will verify individual mapping files as valid, before testing aggregate root "AssetMap".
         * VerifyTheMappings() : verifies retreived saved entity matches original; Object.Equals() is used to
         *                       compare retreived values vs. newly inserted table values.
         *                       
         * 1/9/14 : These tests verify that NHibernate is working correctly. WIP - to complete CRUD operations.
         * 3/24/15: Complete.
         *
         * per Ayende@Rahien:
         * When using NHibernate we generally want to test only three things: that properties are persisted, 
         * that cascade works as expected, and that queries return the correct result(s). 
        */

        private string _connString = string.Empty;
        private ISessionFactory _sessionFactory;
        
        [SetUp]
        public void Init() {

            // Use "...Test" backend. no need for connect string def here? already in CreateSessionFactory()?
             _connString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True";
            //_connString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS';Integrated Security=True";
            _sessionFactory = NHibernateConfiguration.CreateSessionFactory(_connString);
        }




        //TODO: investors, investorid shows up in browser listing of PIMS ?

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_Investor()
        {
            // Arrange
            var currSess = _sessionFactory.OpenSession();
           
            // Act
            new PersistenceSpecification<Investor>(currSess)
                .CheckProperty(i => i.Url, "http://localhost/Pims.Web.Api/api/Investor")
                .CheckProperty(i => i.AspNetUsersId, new Guid())
                .CheckProperty(i => i.LastName, "NhTestLname")
                .CheckProperty(i => i.FirstName, "NhTestFname")
                .CheckProperty(i => i.MiddleInitial, "T.")
                .CheckProperty(i => i.BirthDay, "10/10/2010")
                .CheckProperty(i => i.Address1, "NhTestAddr1")
                .CheckProperty(i => i.Address2, "NhTestAddr2")
                .CheckProperty(i => i.City, "NhTestCity")
                .CheckProperty(i => i.State, "CA")
                .CheckProperty(i => i.ZipCode, "94065")
                .CheckProperty(i => i.Phone, "6505927996")
                .CheckProperty(i => i.Mobile, "6505551212")
                .CheckProperty(i => i.EMailAddr, "NhTestEMailAddr")
                .CheckProperty(i => i.DateAdded, DateTime.Now.ToString("g"))


            // Assert 
            .VerifyTheMappings();
            currSess.Close();
            currSess.Dispose();
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_AssetClass()
        {
            // Arrange
            var currSess = _sessionFactory.OpenSession();

            // Act
            new PersistenceSpecification<AssetClass>(currSess)
                .CheckProperty(ac => ac.Url, "http://localhost/Pims.Web.Api/api/AssetClass")
                .CheckProperty(ac => ac.Code, "CS")
                .CheckProperty(ac => ac.Description, "Common Stock NHtest - " +  DateTime.Now.ToString("g"))
            
                

            // Assert 
            .VerifyTheMappings();
            currSess.Close();
            currSess.Dispose();
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_Profile()
        {
            // Arrange
            var currSess = _sessionFactory.OpenSession();

            // Act
            new PersistenceSpecification<Profile>(currSess)
                .CheckProperty(p => p.TickerSymbol, "AAPL")
                .CheckProperty(p => p.TickerDescription, "Apple Inc.")
                .CheckProperty(p => p.DividendFreq, "Q")
                .CheckProperty(p => p.DividendRate, 1.88M)
                .CheckProperty(p => p.DividendYield, 1.50M)
                .CheckProperty(p => p.EarningsPerShare, 7.39M)
                .CheckProperty(p => p.PE_Ratio, 17.86M)
                .CheckProperty(p => p.LastUpdate, DateTime.Now.ToString("g"))
                .CheckProperty(p => p.ExDividendDate, new DateTime(DateTime.Now.Year,DateTime.Now.Month,23).ToString("d"))
                .CheckProperty(p => p.DividendPayDate, new DateTime(DateTime.Now.Year, DateTime.Now.Month, 26).ToString("d"))
                .CheckProperty(p => p.Price, 132.07M)
    


            // Assert 
            .VerifyTheMappings();
            currSess.Close();
            currSess.Dispose();
        }
        
       
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_AccountType()
        {
            // Arrange
            var currSess = _sessionFactory.OpenSession();
           
            // Act
            new PersistenceSpecification<AccountType>(currSess)
                .CheckProperty(at => at.AccountTypeDesc, "NhTestRoth-IRA - " + DateTime.Now.ToString("g"))


            // Assert 
            .VerifyTheMappings();
            currSess.Close();
            currSess.Dispose();
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_Position()
        {
            // Arrange
            var currSess = _sessionFactory.OpenSession();
            var testDateTime = DateTime.Now.ToString("g");
            var currentAssetId = new Guid("d8d62a0d-67b6-49b9-9f7a-53175a115765");
            var currentAcctTypeId = new Guid("18d97297-bfe8-4746-9e2a-be9bc2c0e9e4"); // Roth-IRA
  
            // Act
            new PersistenceSpecification<Position>(currSess, new CustomEqualityComparer())
                    .CheckProperty(p => p.PositionAssetId, currentAssetId)
                    .CheckProperty(p => p.PurchaseDate, DateTime.Now.AddMonths(-3).ToString("d"))
                    .CheckProperty(p => p.Quantity, 250)
                    .CheckProperty(p => p.LastUpdate, testDateTime)
                    .CheckProperty(p => p.MarketPrice, 162.13M)
                    .CheckProperty(p => p.AcctTypeId, currentAcctTypeId)
 

           // Assert 
           .VerifyTheMappings();
           currSess.Close();
           currSess.Dispose();
            
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_Income()
        {
            // Arrange - uses existing Investor & Asset.
            var currSess = _sessionFactory.OpenSession();
            var currentAssetId = new Guid("d8d62a0d-67b6-49b9-9f7a-53175a115765");
            var testDateTime = DateTime.Now.ToString("g");

            // Act - PKs generated automatically, no checks required also for NH entity relationship attributes.
            // * Check Mappings * to avoid error: System.ApplicationException : For property 'AssetId' of type 'System.Guid' expected
            //                                    'd8d62a0d-67b6-49b9-9f7a-53175a115765' but got '00000000-0000-0000-0000-000000000000
            new PersistenceSpecification<Income>(currSess, new CustomEqualityComparer())
                .CheckProperty(i => i.Actual, 10.50M)
                .CheckProperty(i => i.Projected, 10.38M)
                .CheckProperty(i => i.DateRecvd, DateTime.Now.AddMinutes(-10).ToString("d"))
                .CheckProperty(i => i.LastUpdate, testDateTime)
                .CheckProperty(i => i.AssetId, currentAssetId)


            // Assert 
           .VerifyTheMappings();
           currSess.Close();
           currSess.Dispose();

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_necessary_composite_entities_within_a_single_CREATE_transaction()
        {
            // Mimics (C)reate process during Asset save.

            var currentDateTime = DateTime.Now;
            //const string urlBase = "http://localhost/Pims.Web.Api/api";
            var investorGuid = Guid.NewGuid();
            IList<Position> positionsToAdd = new List<Position>
                        {
                            new Position
                            {
                                PositionId = Guid.NewGuid(),
                                PositionAssetId = new Guid("D8D62A0D-67B6-49B9-9F7A-53175A115765"),
                                PurchaseDate = currentDateTime.AddMonths(-8).ToString("d"),
                                Quantity = int.Parse("125"),
                                MarketPrice = 32.12M,
                                AcctTypeId = new Guid("18D97297-BFE8-4746-9E2A-BE9BC2C0E9E4"), // Roth-IRA
                                LastUpdate = currentDateTime.ToString("g"),
                            },
                            new Position
                            {
                                PositionId = Guid.NewGuid(),
                                PositionAssetId = new Guid("D8D62A0D-67B6-49B9-9F7A-53175A115765"),
                                PurchaseDate = currentDateTime.AddMonths(-3).ToString("d"),
                                Quantity = 100,
                                MarketPrice = 40.62M,
                                AcctTypeId = new Guid("745AD04D-D7DF-4CD2-A5C1-ADA8032FCFD8"), // CMA
                                LastUpdate = currentDateTime.ToString("g"),
                            }
                        };

            IList<Income> revenueToAdd = new List<Income>
                        {
                            new Income
                            {
                                Actual = decimal.Parse("74.50"),
                                DateRecvd = currentDateTime.ToString("d"),
                                Projected = decimal.Parse("75.11"),
                                AssetId = new Guid("D8D62A0D-67B6-49B9-9F7A-53175A115765"),
                                LastUpdate = currentDateTime.ToString("g")
                            }
                        };

           
            

            using (var currSess = _sessionFactory.OpenSession())
            using (currSess.BeginTransaction())
            {
                // Arrange
                new PersistenceSpecification<Asset>(currSess, new CustomEqualityComparer())

                
                //  Act
                .CheckProperty(a => a.InvestorId, investorGuid)
                .CheckProperty(a => a.AssetClassId, new Guid("B9C7D922-A4D2-49B6-AEB0-A30600F02CC3")) // PFD stock

                .CheckReference(c => c.Profile, new Profile {
                    ProfileId = Guid.NewGuid(),
                    AssetId = new Guid("D8D62A0D-67B6-49B9-9F7A-53175A115765"),
                    TickerSymbol = "VZ",
                    TickerDescription = "Verizon Communications, Inc.",
                    Price = decimal.Parse("49.72"),
                    DividendYield = decimal.Parse("4.50"),
                    ExDividendDate = currentDateTime.AddDays(-11).ToString("d"),
                    DividendPayDate = currentDateTime.AddDays(5).ToString("d"),
                    PE_Ratio = decimal.Parse("20.55"),
                    DividendFreq = "M",
                    EarningsPerShare = decimal.Parse("2.42"),
                    LastUpdate = currentDateTime.ToString("g")
                })

                
                
                // Assert 
                .VerifyTheMappings();

                // Workaround for .CheckList(p => p.Positions, positionsToAdd), when Positions count >= 1
                foreach (var position in positionsToAdd) {currSess.Save(position);}

                // Workaround for .CheckList(r => r.Revenue, revenueToAdd), when Income count >= 1
                foreach (var income in revenueToAdd) { currSess.Save(income); }

                currSess.Transaction.Commit();
            }


        }


        //[Test]
        // ReSharper disable once InconsistentNaming
        //public void Correctly_Utilize_Its_UnitOfWork_Via_ISession()
        //{
        //    /* Ensure every db operation within a web request uses the same ISession object. */
        //    // Arrange
        //   // ** Can't mimic real-time request to test ISession **
        //   //StructureMap.IContainer smContainer = IoC.Initialize(_connString);
         

        //    // NHibernate.CheckSession() should be called as a result of ISession ctor injection
        //    const string urlBase = "http://localhost/PIMS.Web.API/api";
        //    //var client = CreateWebClient();
        //    var httpWebReq = (HttpWebRequest)WebRequest.Create(urlBase + "/values");
        //    httpWebReq.Connection = _connString;
           

        //    //var response = client.DownloadString(urlBase + "/values");
        //    var response = httpWebReq.GetResponse();
        //    var stream = response.GetResponseStream();
        //    if (stream != null)
        //    {
        //        var streamRdr = new StreamReader(stream);
        //        streamRdr.ReadToEnd();
        //    }


        //}


    }





    public class CustomEqualityComparer :  IEqualityComparer
    {
        public new bool Equals(object x, object y)
        {
            var objectsAreEqual = false;
            if (x == null || y == null){
                    return false;
            }

            switch (x.GetType().Name)
            {
                 case "Int32": {
                     objectsAreEqual = (x is Int32 && y is Int32) && x.Equals(y);
                        break;
                    }
                 case "Guid": {
                        objectsAreEqual = (x is Guid && y is Guid) && x.Equals(y);
                        break;
                    }
                 case "String": {
                         objectsAreEqual = (x is String && y is String) && x.Equals(y);
                         break;
                     }
                 case "Decimal": {
                     objectsAreEqual = (x is Decimal && y is Decimal) && x.Equals(y);
                         break;
                     }
                  case "Income": {
                      objectsAreEqual = (x is Income && y is Income) ? ((Income)x).IncomeId == ((Income)y).IncomeId : x.Equals(y);
                      break;
                  }
                  case "Position": {
                      objectsAreEqual = (x is Position && y is Position) ? ((Position)x).PositionId == ((Position)y).PositionId : x.Equals(y);
                      break;
                  }
                  case "AccountType": {
                      objectsAreEqual = (x is AccountType && y is AccountType) ? ((AccountType)x).KeyId == ((AccountType)y).KeyId : x.Equals(y);
                      break;
                      }
                  case "Asset": {
                          objectsAreEqual = (x is Asset && y is Asset) ? ((Asset)x).AssetId == ((Asset)y).AssetId : x.Equals(y);
                          break;
                      }
                  case "Profile": {  
                       objectsAreEqual = (x is Profile && y is Profile) ? ((Profile)x).ProfileId == ((Profile)y).ProfileId : x.Equals(y);
                       break;
                      }
                  case "Investor": {
                          objectsAreEqual = (x is Investor && y is Investor) ? ((Investor)x).InvestorId == ((Investor)y).InvestorId : x.Equals(y);
                          break;
                      }
                  case "AssetClass": {
                      objectsAreEqual = (x is AssetClass && y is AssetClass) ? ((AssetClass)x).KeyId == ((AssetClass)y).KeyId : x.Equals(y);
                          break;
                      }
              }

         
             return objectsAreEqual;
        }


        public int GetHashCode(object obj) {

            return obj.ToString().ToLower().GetHashCode();
        }

    }


 


}
