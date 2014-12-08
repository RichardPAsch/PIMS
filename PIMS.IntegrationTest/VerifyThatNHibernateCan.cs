// build action = "compile"

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
         * PK (Ids) automatically SQL Server-generated.
         * VerifyTheMappings() : verifies retreived saved entity matches original; Object.Equals() is used to
         *                       compare retreived values vs. newly inserted table values.
         *                       
         * TODO: Cleanup code, make sure ALL CRUD operations work!
         * 1/9/14: These tests verify that NHibernate is working correctly. WIP - to complete CRUD operations.
         *
         * Ayende@Rahien:
         * When using NHibernate we generally want to test only three things: that properties are persisted, 
         * that cascade works as expected, and that queries return the correct result. 
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





        /// <summary>
        /// First test series: test INDIVIDUAL
        ///      1) domain entity mappings, 
        ///      2) their respective table persistence, and
        ///      3) entity data is accurately saved via roundtrip PersistenceSpecification()
        /// </summary>
        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public void Correctly_Map_And_Persist_Income()
        //{
        //    // Arrange
        //    var currSess = _sessionFactory.OpenSession();
        //    new PersistenceSpecification<Asset>(currSess, new CustomEqualityComparer())

        //    // Act
        //    .CheckReference(c => c.Income, new Income
        //                                   {
        //                                        IncomeId = new Guid(),
        //                                        DateRecvd = DateTime.Now.ToLocalTime().ToString("g"),
        //                                        Actual = decimal.Parse("182.11"),
        //                                        Projected = decimal.Parse("190.26")
        //                                    })

        //    // Assert 
        //    .VerifyTheMappings();
        //    currSess.Close();
        //    currSess.Dispose();
        //}



        /// <summary>
        /// Second test series: test all COLLECTIVE 
        ///         1) child entities of an single Asset are correctly persisted.
        /// </summary>
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_ALL_composite_entities_within_a_single_transaction()
        {
            var currentDateTimeUtc = DateTime.UtcNow;

            using (var currSess = _sessionFactory.OpenSession())
                using (currSess.BeginTransaction())
                {
                    // Arrange
                    new PersistenceSpecification<Asset>(currSess, new CustomEqualityComparer())
                    

                    //  Act
                    .CheckReference(c => c.Revenue, new List<Income> 
                    {
                        new Income
                        {
                            Actual = decimal.Parse("74.50"),
                            DateRecvd = currentDateTimeUtc.ToString("g"),
                            Projected = decimal.Parse("75.11"),
                            IncomeId = new Guid(),
                            Account = "Roth-IRA"
                        }
                    })
                    .CheckReference(c => c.Revenue, new List<Position>
                        {
                            new Position
                            {
                                PositionId = new Guid(),
                                PurchaseDate = currentDateTimeUtc.ToString("d"),
                                Quantity = int.Parse("125"),
                                UnitCost = decimal.Parse("53.11"),
                                Account = new AccountType{AccountTypeDesc = "Roth-IRA", KeyId = Guid.NewGuid(), Url = "api/at"},
                                LastUpdate = "10/11/2013 15:44"
                            },
                            new Position
                            {
                                PositionId = new Guid(),
                                PurchaseDate = currentDateTimeUtc.ToString("d"),
                                Quantity = int.Parse("19"),
                                UnitCost = decimal.Parse("84.90"),
                                Account = new AccountType{AccountTypeDesc = "CMA", KeyId = Guid.NewGuid(), Url = "api/at"},
                                LastUpdate = "11/21/2014 10:04"
                            }
                        })
                    //.CheckReference(c => c.Position, new Position {
                    //    PositionId = new Guid(),
                    //    PurchaseDate = currentDateTimeUtc.ToString("d"),
                    //    Quantity = int.Parse("125"),
                    //    UnitCost = decimal.Parse("53.11")
                    //})

                    .CheckReference(c => c.Profile, new Profile {
                        ProfileId = Guid.NewGuid(),
                        AssetId = new Guid(),
                        TickerSymbol = "GSK",
                        TickerDescription = "GlaxoSmithKline",
                        DividendRate = decimal.Parse("0.7913"),
                        DividendYield = decimal.Parse("5.08"),
                        ExDividendDate = currentDateTimeUtc.ToString("d"),
                        DividendPayDate = currentDateTimeUtc.AddDays(5).ToString("d"),
                        PE_Ratio = decimal.Parse("14.00"),
                        DividendFreq = "M",
                        EarningsPerShare = decimal.Parse("1.90"),
                        LastUpdate = currentDateTimeUtc.ToString("g")
                    })
                    
                    .CheckReference(c => c.Investor, new Investor {
                        AspNetUsersId = Guid.NewGuid(),
                        FirstName = "Richard",
                        LastName = "Asch",
                        Address1 = "10 Main Street",
                        Address2 = "Unit 25102",
                        City = "Redwood Shores",
                        BirthDay = currentDateTimeUtc.ToString("d"),
                        InvestorId = new Guid(),
                        Phone = "65039381073",
                        Mobile = "6509286543",
                        EMailAddr = "me@rpclassics.net"
                    })

                    .CheckReference(c => c.AssetClass, new AssetClass {
                        KeyId = Guid.NewGuid(),
                        Code = "CSQ",
                        Description = "Calamos Fund"
                        
                    })

                    // Assert 
                    .VerifyTheMappings();
                    currSess.Transaction.Commit();
                }

            
        }



        //[Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Utilize_Its_UnitOfWork_Via_ISession()
        {
            /* Ensure every db operation within a web request uses the same ISession object. */
            // Arrange
           // ** Can't mimic real-time request to test ISession **
           //StructureMap.IContainer smContainer = IoC.Initialize(_connString);
         

            // NHibernate.CheckSession() should be called as a result of ISession ctor injection
            const string urlBase = "http://localhost/PIMS.Web.API/api";
            //var client = CreateWebClient();
            var httpWebReq = (HttpWebRequest)WebRequest.Create(urlBase + "/values");
            httpWebReq.Connection = _connString;
           

            //var response = client.DownloadString(urlBase + "/values");
            var response = httpWebReq.GetResponse();
            var stream = response.GetResponseStream();
            if (stream != null)
            {
                var streamRdr = new StreamReader(stream);
                streamRdr.ReadToEnd();
            }


        }


    }



    public class CustomEqualityComparer :  IEqualityComparer
    {
        public new bool Equals(object x, object y)
        {
            var objectsAreEqual = false;
            if (x == null || y == null){
                    return false;
            }

            switch (x.GetType().Name) {
                  case "Income": {
                      objectsAreEqual = (x is Income && y is Income) ? ((Income)x).IncomeId == ((Income)y).IncomeId : x.Equals(y);
                      break;
                  }
                  case "Position": {
                      objectsAreEqual = (x is Position && y is Position) ? ((Position)x).PositionId == ((Position)y).PositionId : x.Equals(y);
                      break;
                  }
                  case "Profile": {
                       objectsAreEqual = (x is Profile && y is Profile) ? ((Profile)x).AssetId == ((Profile)y).AssetId : x.Equals(y);
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
            //throw new NotImplementedException();
            return obj.ToString().ToLower().GetHashCode();
        }

    }


 


}
