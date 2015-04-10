using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using NHibernate;
using NUnit.Framework;
using PIMS.Core.Models;
using PIMS.Data.Repositories;
using PIMS.Web.Api.Controllers;



namespace PIMS.IntegrationTest
{
    [TestFixture]
    public class VerifyAssetClassController
    {

        /*
            API tests WebApi endpoints, mimicking client-based calls, as in sending resource-based URL requests; this 
            tests from the client's point of view, and most closely resembles real world scenarios.
            All response information is derived via a test database.
            These tests also verify IoC/DI is working correctly.
        */

        private ISessionFactory _nhSessionFactory;
        private AssetClassController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api";
        IGenericRepository<AssetClass> _repository;  


        [SetUp]
        public void Init() {
            _nhSessionFactory = NhDatabaseConfiguration.CreateSessionFactory();
            _repository = new AssetClassRepository(_nhSessionFactory);
        }



        [Test]
        // ReSharper disable once InconsistentNaming
        public async Task Can_GET_All_Asset_Classifications()
        {
            // Arrange
            _ctrl = new AssetClassController(_repository) {
                Request = new HttpRequestMessage { RequestUri = new Uri(UrlBase + "/AssetClass") },
                Configuration = new HttpConfiguration()
            };
            

            // Act
            var classifications = await _ctrl.GetAll(); 


            // Assert
            Assert.IsNotNull(classifications);
            Assert.GreaterOrEqual(classifications.Count(), 15);
            Assert.That(classifications.First(x => x.Code.Trim() == "PFD").Code, Is.Unique);
            Assert.That(classifications.First(x => x.Code.Trim() == "PFD").Description, Is.EqualTo("Preferred Stock"));
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_an_Asset_Classification_By_Code()
        {

            // Arrange
            _ctrl = new AssetClassController(_repository) {
                Request = new HttpRequestMessage { RequestUri = new Uri(UrlBase + "/AssetClass/ETF") },
                Configuration = new HttpConfiguration()
            };

            // Act
            var assetClass = await _ctrl.GetByClassification("ETF") as OkNegotiatedContentResult<IQueryable<AssetClass>>;
            
            
            // Assert
            Assert.IsNotNull(assetClass);
            Assert.IsTrue(assetClass.Content.First().Code.Trim().ToUpper() == "ETF");
            Assert.That(assetClass.Content.ToList(), Is.Unique);

        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_an_Asset_Classification_By_Id()
        {
            // Arrange
            _ctrl = new AssetClassController(_repository) {
                Request = new HttpRequestMessage { RequestUri = new Uri(UrlBase + "/AssetClass/567f2176-2098-4800-bdf3-a2fc00a6be5a") },
                Configuration = new HttpConfiguration()
            };

            // Act
            var assetClass = await _ctrl.GetByClassificationId(new Guid("567f2176-2098-4800-bdf3-a2fc00a6be5a")) as OkNegotiatedContentResult<AssetClass>;

            // Assert
            Assert.IsNotNull(assetClass);
            Assert.IsTrue(assetClass.Content.KeyId == new Guid("567f2176-2098-4800-bdf3-a2fc00a6be5a"));
            Assert.IsTrue(assetClass.Content.Code.Trim().ToUpper() == "ETF");

        }

        
        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_POST_a_New_Asset_Classification()
        {

            // Arrange
            _ctrl = new AssetClassController(_repository) {
                Request = new HttpRequestMessage { RequestUri = new Uri(UrlBase + "/AssetClass") },
                Configuration = new HttpConfiguration()
            };

            var newClassification = new AssetClass
                                        {
                                            Code = "TEST",
                                            Description = DateTime.Now.ToString("g")
                                        };

            // Act
            //var debugJsonForFiddler = TestHelpers.ObjectToJson(newClassification);
            var response = await _ctrl.CreateNewAssetClass(newClassification) as CreatedNegotiatedContentResult<AssetClass>;
            
            
            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.Location.AbsoluteUri.Contains("api/AssetClass/TEST"), Is.True);
 
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_PUT_Update_an_Asset_Classification_By_Asset_Class()
        {
           
            // Arrange
            _ctrl = new AssetClassController(_repository) {
                Request = new HttpRequestMessage { RequestUri = new Uri(UrlBase + "/AssetClass/TEST") },
                Configuration = new HttpConfiguration()
            };

                var editedClassification = new AssetClass
                                    {
                                        KeyId = new Guid("3437791f-3e3f-4f76-a266-a47600c39453"),
                                        Code = "TEST",
                                        Description = DateTime.Now.ToString("g")
                                    };


            // Act
            //var debugJsonForFiddler = TestHelpers.ObjectToJson(editedClassification);
            var assetClassResult = await _ctrl.UpdateAssetClass(editedClassification, editedClassification.Code.Trim()) as OkNegotiatedContentResult<AssetClass>;
                

            // Assert
            Assert.IsNotNull(assetClassResult);
           
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_DELETE_an_Asset_Classification_By_Id() {

            // Arrange
            var existingGuid = new Guid();
            var preTestCtrl = new AssetClassController(_repository)
                                         {
                                            Request = new HttpRequestMessage { RequestUri = new Uri(UrlBase + "/AssetClass/TEST") },
                                            Configuration = new HttpConfiguration()
                                         };

            var existingAssetClass = await preTestCtrl.GetByClassification("TEST") as OkNegotiatedContentResult<IQueryable<AssetClass>>;
            if (existingAssetClass != null)
            {
                existingGuid = new Guid(existingAssetClass.Content.First().KeyId.ToString(CultureInfo.InvariantCulture.ToString()));
            }
            else
            {
                Assert.False(existingAssetClass == null);
            }


            _ctrl = new AssetClassController(_repository) {
                Request = new HttpRequestMessage { RequestUri = new Uri(UrlBase + "/AssetClass/" + existingGuid) },
                Configuration = new HttpConfiguration()
            };

            // Act
            var assetClass = await _ctrl.Delete(existingGuid) as OkNegotiatedContentResult<AssetClass>;


            // Assert
            Assert.IsNull(assetClass);
  
        }


        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async void Cannot_GET_an_Asset_Classification_By_An_Invalid_Asset_Class() {

        //    using (var client = new HttpClient()) {
        //        // Arrange
        //        client.BaseAddress = new Uri(UrlBase + "/AssetClass/RPA");
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        // Act
        //        var resp = await client.GetAsync(client.BaseAddress);
        //        var assetClass = await resp.Content.ReadAsAsync<Core.Models.AssetClass>();

        //        // Assert
        //        Assert.IsTrue(resp.StatusCode == HttpStatusCode.NotFound);
        //        Assert.IsNull(assetClass);
        //    }
        //}


        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public async void Can_GET_an_Asset_Classification_By_Asset_Class() {

        //    using (var client = new HttpClient()) {
        //        // Arrange
        //        client.BaseAddress = new Uri(UrlBase + "/AssetClass/ETF");
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        // Act
        //        HttpResponseMessage resp = await client.GetAsync(client.BaseAddress);
        //        var assetClass = await resp.Content.ReadAsAsync<Core.Models.AssetClass>();



        //        // Assert
        //        Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
        //        Assert.IsNotNull(assetClass);
        //        Assert.IsTrue(assetClass.Code.ToUpper().Trim() == "ETF");
        //        Assert.IsFalse(assetClass.Description == string.Empty);
        //    }
        //}


        


        //[Test]
        //// ReSharper disable once InconsistentNaming
        //public void Cannot_POST_a_duplicate_Asset_Classification() {

        //    using (var client = new HttpClient()) {

        //        // Arrange
        //        client.BaseAddress = new Uri(UrlBase + "/AssetClass/");
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        var newClassification = new Core.Models.AssetClass {
        //            Code = "CEF",
        //            Description = "Closed-End Fund"
        //        };

        //        // Act
        //        var response = client.PostAsJsonAsync(client.BaseAddress.ToString(), newClassification).Result;
              
        //        // Assert
        //        Assert.IsTrue(response.StatusCode == HttpStatusCode.Conflict);


        //    }

        //}

    }


}
