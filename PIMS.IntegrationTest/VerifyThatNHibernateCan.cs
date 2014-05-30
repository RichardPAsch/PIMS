// build action = "compile"

using System;
using System.Collections;
using System.IO;
using System.Net;
using FluentNHibernate.Testing;
using NHibernate;
using NUnit.Framework;
using PIMS.Core.Models;
using PIMS.Web.Api;


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
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_Income()
        {
            // Arrange
            var currSess = _sessionFactory.OpenSession();
            new PersistenceSpecification<Asset>(currSess, new CustomEqualityComparer())

            // Act
            .CheckReference(c => c.Income, new Income
                                           {
                                                IncomeId = new Guid(),
                                                DateRecvd = DateTime.Now.ToLocalTime(),
                                                Actual = decimal.Parse("182.11"),
                                                Projected = decimal.Parse("190.26")
                                            })

            // Assert 
            .VerifyTheMappings();
            currSess.Close();
            currSess.Dispose();
        }



        /// <summary>
        /// Second test series: test all COLLECTIVE 
        ///         1) child entities of an single Asset are correctly persisted.
        /// </summary>
        [Test]
        // ReSharper disable once InconsistentNaming
        public void Correctly_Map_And_Persist_ALL_composite_entities_within_a_single_transaction()
        {

            var todaysDateTime = DateTime.Now.ToLocalTime();
            using (var currSess = _sessionFactory.OpenSession())
                using (currSess.BeginTransaction())
                {
                    // Arrange
                    new PersistenceSpecification<Asset>(currSess, new CustomEqualityComparer())
                    

                        //  Act
                        .CheckReference(c => c.Income, new Income
                                                       {
                                                                        Actual = decimal.Parse("74.50"),
                                                                        DateRecvd = todaysDateTime,
                                                                        Projected = decimal.Parse("75.11"),
                                                                        IncomeId = new Guid()
                                                                    })

                        .CheckReference(c => c.Position, new Position
                                                         {
                                                                            PositionId = new Guid(),
                                                                            PurchaseDate = todaysDateTime,
                                                                            MarketPrice = decimal.Parse("54.26"),
                                                                            Quantity = decimal.Parse("125"),
                                                                            TotalValue = decimal.Parse("89.95"),
                                                                            UnitPrice = decimal.Parse("53.11")
                                                                        })

                        .CheckReference(c => c.Profile, new Profile
                                                        {
                                                                          ProfileId = new Guid(),
                                                                          TickerSymbol = "GSK",
                                                                          TickerDescription = "GlaxoSmithKline",
                                                                          DividendRate = decimal.Parse("0.7913"),
                                                                          DividendYield = decimal.Parse("5.08"),
                                                                          PE_Ratio = decimal.Parse("14.00"),
                                                                          DividendFreq = "M",
                                                                          EarningsPerShare = decimal.Parse("1.90"),
                                                                          SharePrice = 46.11M,
                                                                          LastUpdate = todaysDateTime
                                                                      })

                        .CheckReference(c => c.User, new User
                                                     {
                                                                    LastName = "Asch",
                                                                    FirstName = "Richard P.",
                                                                    Password = "mypassword",
                                                                    UserName = "rpa",
                                                                    EMail = "rpasch@rpclassics.net"
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






/*
         private static WebClient CreateWebClient()
        {
             var webClient = new WebClient();
             return webClient;

        }
*/
       

    }



    public class CustomEqualityComparer :  IEqualityComparer
    {
        public new bool Equals(object x, object y)
        {
            bool objectsAreEqual = false;
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
                       objectsAreEqual = (x is Profile && y is Profile) ? ((Profile)x).ProfileId == ((Profile)y).ProfileId : x.Equals(y);
                       break;
                      }
                  case "User": {
                          objectsAreEqual = (x is User && y is User) ? ((User)x).UserId == ((User)y).UserId : x.Equals(y);
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
