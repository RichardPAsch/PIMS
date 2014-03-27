using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using NUnit.Framework;
using PIMS.Core.Models;
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

        private AssetClassController _ctrl;
        private const string UrlBase = "http://localhost/PIMS.Web.API/api";
      

        public void Init()
        {
            //_sessionFactory = NHibernateConfiguration.CreateSessionFactory(ConnString); // needed?
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_All_Asset_Classifications() {

            using (var client = new HttpClient())
                {
                    // Arrange
                    client.BaseAddress = new Uri(UrlBase + "/AssetClass");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Act
                    HttpResponseMessage resp = await client.GetAsync(client.BaseAddress);
                    var classes = await resp.Content.ReadAsAsync<IEnumerable<AssetClass>>();


                    // Assert
                    Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                    Assert.IsNotNull(classes);
                    // Avoid posssible multiple enumerations.
                    var assetClasses = classes as AssetClass[] ?? classes.ToArray();
                    Assert.IsTrue(assetClasses.First().Code != string.Empty);
                    Assert.IsTrue(assetClasses.First().Description != string.Empty);
                    Assert.GreaterOrEqual(assetClasses.Count(), 4);
               }
        }
        
        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_an_Asset_Classification_By_Asset_Class() {

            using (var client = new HttpClient()) {
                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/C3");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Act
                HttpResponseMessage resp = await client.GetAsync(client.BaseAddress);
                var assetClass = await resp.Content.ReadAsAsync<AssetClass>();

                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNull(assetClass);
                Assert.IsTrue(assetClass.Code.ToUpper().Trim() == "C3");
                Assert.IsFalse(assetClass.Description == string.Empty);
            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Cannot_GET_an_Asset_Classification_By_An_Invalid_Asset_Class() {

            using (var client = new HttpClient()) {
                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/C9");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Act
                HttpResponseMessage resp = await client.GetAsync(client.BaseAddress);
                var assetClass = await resp.Content.ReadAsAsync<AssetClass>();

                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.NotFound);
                Assert.IsNull(assetClass);
            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_GET_an_Asset_Classification_By_Asset_Id() {

            using (var client = new HttpClient()) {
                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/a3cce8b1-a52e-4f1b-8b9d-353b50f0d3bb");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Act
                HttpResponseMessage resp = await client.GetAsync(client.BaseAddress);
                var assetClass = await resp.Content.ReadAsAsync<AssetClass>();

                // Assert
                Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNull(assetClass);
                Assert.IsTrue(assetClass.KeyId == new Guid(" a3cce8b1-a52e-4f1b-8b9d-353b50f0d3bb"));
                Assert.IsFalse(assetClass.Description == string.Empty);
            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Can_POST_a_New_Asset_Classification() {

            using (var client = new HttpClient()) {

                // Arrange
                client.BaseAddress = new Uri(UrlBase + "/AssetClass/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var newClassification = new AssetClass {
                                                            Code = "CEF",
                                                            Description = "Closed-End Fund"
                                                        };
                

                // Act
                var response = client.PostAsJsonAsync(client.BaseAddress.ToString(), newClassification).Result;// OK! data saved!
                //HttpResponseMessage resp = await client.GetAsync(client.BaseAddress);
                //var classes = await resp.Content.ReadAsAsync<IEnumerable<AssetClass>>();


                //// Assert
                //Assert.IsTrue(resp.StatusCode == HttpStatusCode.OK);
                //Assert.IsNotNull(classes);
                //// Avoid posssible multiple enumerations.
                //var assetClasses = classes as AssetClass[] ?? classes.ToArray();
                //Assert.IsTrue(assetClasses.First().Code != string.Empty);
                //Assert.IsTrue(assetClasses.First().Description != string.Empty);
                //Assert.GreaterOrEqual(assetClasses.Count(), 4);
            }
            
        }
      

    }


}
