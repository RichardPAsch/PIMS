using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Microsoft.AspNet.Identity;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PIMS.Core.Models;
using PIMS.Web.Api;
using PIMS.Web.Api.Controllers;


namespace PIMS.IntegrationTest.Security
{
    [TestFixture]
    public class VerifyAuthorization
    {

        private const string UrlBase = "http://localhost/PIMS.Web.Api/Api/Account";
        private LoginModel _login;
        private UserManager<ApplicationUser> _userMgr;


        // TODO:  why doesn't this read the correct conn string from web.config, instead of machine.config ?
        //private readonly string _connString = ConfigurationManager.ConnectionStrings["PIMS-ConnString"].ConnectionString;
        private const string ConnString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True";



        [SetUp]
        public void Init()
        {
            // orig - working
            _userMgr = new UserManager<ApplicationUser>(
                new NHibernate.AspNet.Identity.UserStore<ApplicationUser>(NHibernateConfiguration.CreateSessionFactory(ConnString).OpenSession()));
            
            // Min length for password = 6.
            _login = new LoginModel
                            {
                                UserName = "TestUser0828a",
                                Password = "pwrd0828h"
                            };
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_can_sucessfully_grant_a_token()
        {
            using (var client = new HttpClient())
            {
                // Arrange - using login credentials of authenticated user.
                // ** Exceeding AccessTokenExpireTimeSpan since creation, will result in "invalid_grant" server error. **
                var post = new Dictionary<string, string>
                           {
                               {"grant_type", "password"},
                               {"username", "TestUser072214a"},
                               {"password", "pwrd0722a"}
                           };

                // Act
                var response = await client.PostAsync(UrlBase + "/token", new FormUrlEncodedContent(post));
                var content = await response.Content.ReadAsStringAsync();


                // Assert
                Assert.IsNotNullOrEmpty(content);
                Assert.IsTrue(content.IndexOf("bearer", StringComparison.Ordinal) > 0);

            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_returns_an_accessToken_via_a_successful_login()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                // Prerequisite: login data to be used, MUST be based on a previously successfull user registration, generated token 
                //               lifetime will be based on AccessTokenExpireTimeSpan in StartUp.cs.
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var serializerSettings = new JsonSerializerSettings();
                var serializer = JsonSerializer.Create(serializerSettings);
                var jasonArray = JObject.FromObject(_login, serializer);
                HttpContent content = new StringContent(jasonArray.ToString()); // entity body & content headers
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                // Act
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);
                var accessToken = loginResponse.Content.ReadAsStringAsync().Result;


                // Assert
                Assert.IsTrue(loginResponse.StatusCode == HttpStatusCode.OK);
                Assert.IsNotNullOrEmpty(accessToken);
                Assert.IsTrue(loginResponse.Content.Headers.ContentLength >= 500 );

            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_not_allow_use_of_an_expired_token_on_the_service()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                // a. Create token, with expiration time, via login on an existing authenticated user.
                // b. Try to access WebApi, via just created token, using configured expired token time of 5 sec.
                //    [ * Reset AccessTokenExpireTimeSpan for other tests or WebApi use * ]

                // a:
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_login, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);
                var accessToken = loginResponse.Content.ReadAsStringAsync().Result;
               

                // Act
                // b:
                Thread.Sleep(TimeSpan.FromSeconds(6));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                var authorizationResponse = await client.GetAsync(client.BaseAddress + "/SecurityTestMethod"); 


                // Assert
                Assert.IsNotNullOrEmpty(accessToken);
                Assert.IsTrue(loginResponse.Content.Headers.ContentLength >= 500);
                Assert.IsTrue(authorizationResponse.StatusCode == HttpStatusCode.Unauthorized);
            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_allow_use_of_a_token_on_the_service_after_a_successful_login()
        {
            using (var client = new HttpClient())
            {
                // After a successful login, we'll use the issued access token to gain access to the web service. The SAME UNEXPIRED
                // token will be used for each subsequent web service interaction, as would be the case where the client
                // would be supplying the granted token in a real use case scenario. If the token has expired, a new login will be required.
                // Running this test multiple times on same user with an unexpired token, will result in an "unauthorized" (401) access error.
                // [* Expiration is based on : AccessTokenExpireTimeSpan configuration in StartUpAuth *]

                // Arrange
                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_login, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);
                var accessToken = loginResponse.Content.ReadAsStringAsync().Result;

                // Reinitialize returned token w/o [Welcome xxx] message, if needed.
                // accessToken.Substring(1, accessToken.Length - (accessToken.IndexOf(']') - accessToken.IndexOf('[') )-3);
                // Reinitialize returned token w/o default escape sequences.
                accessToken = accessToken.Substring(1, accessToken.Length -2 ); 


                // Act
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var testMethodResponse = await client.GetAsync(client.BaseAddress + "/SecurityTestMethod");
                var responseContent = await testMethodResponse.Content.ReadAsStringAsync();

                // Assert
                Assert.IsNotNullOrEmpty(accessToken);
                Assert.IsTrue(loginResponse.Content.Headers.ContentLength >= 500);
                Assert.IsTrue(testMethodResponse.StatusCode == HttpStatusCode.OK);
                Assert.IsTrue(responseContent.IndexOf("granted", StringComparison.Ordinal) > 1);
            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_not_allow_invalid_username_login() 
        {
            using (var client = new HttpClient()) 
            {
                // Arrange
                _login = new LoginModel
                            {
                                UserName = "TestUs0731a",
                                Password = "pwrd0731a"
                            };

                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_login, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                // Act
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);

                // Assert
                Assert.IsTrue(loginResponse.StatusCode == HttpStatusCode.InternalServerError);
            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_not_allow_invalid_password_login() {
            using (var client = new HttpClient()) {
                // Arrange
                _login = new LoginModel {
                    UserName = "TestUser0731a",
                    Password = "pwr 0731a"
                };

                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_login, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                // Act
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);

                // Assert
                Assert.IsTrue(loginResponse.StatusCode == HttpStatusCode.InternalServerError);
            }

        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public async void Middleware_will_not_return_an_accessToken_via_an_unsuccessful_login()
        {
            using (var client = new HttpClient())
            {
                // Arrange
                _login = new LoginModel {
                    UserName = "TestUser0731a",
                    Password = "prd0731a"
                };

                client.BaseAddress = new Uri(UrlBase);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var settings = new JsonSerializerSettings();
                var ser = JsonSerializer.Create(settings);
                var j = JObject.FromObject(_login, ser);
                HttpContent content = new StringContent(j.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");


                // Act
                var loginResponse = await client.PostAsync(client.BaseAddress + "/LoginAsync", content);
                var accessToken = loginResponse.Content.ReadAsStringAsync().Result;


                // Assert
                Assert.IsTrue(loginResponse.StatusCode == HttpStatusCode.InternalServerError);
                Assert.IsTrue(accessToken.IndexOf("exceptionMessage", StringComparison.Ordinal) >= 1);
            }

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Middleware_can_sucessfully_update_a_users_password_upon_request()
        {
            #region - Unsuccessful tests/code via Moq for UserManager. Defer ?
                //var userMgrMock = new Mock<UserManager<ApplicationUser>>(
                //                  new UserStore<ApplicationUser>(NHibernateConfiguration.CreateSessionFactory(ConnString).OpenSession()));
                //userMgrMock.Setup(x => x.ChangePasswordAsync(loggedInUser.Id, _login.Password, "pwrd0827b"))
                //                        .ReturnsAsync(new IdentityResult());

                //var userStoreMock = new Mock<UserStore<ApplicationUser>>(NHibernateConfiguration.CreateSessionFactory(ConnString).OpenSession());
                //var userMgr2Mock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object);
                //userMgr2Mock.Setup(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                //                        .ReturnsAsync(new IdentityResult()); 

                // returns: IdentityResult.Succeeded = false - 8/27/14; 8/28 - definitely a Moq setup issue!
                //userMgrMock.Setup(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                //                        .ReturnsAsync(new IdentityResult()); 
                //var acctCtrl = new AccountController(userMgrMock.Object); //no
                //var acctCtrl = new AccountController(userMgr2Mock.Object); 

                //var identity = new GenericIdentity(loggedInUser.UserName);
                //var claimsIdentity = new ClaimsIdentity();
                //identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", loggedInUser.Id));
                //identity.AddClaims(claimsCollection);
                //var principal = new GenericPrincipal(identity, new[] {"user"});
                //requestCtx.SetupGet(s => s.Principal).Returns(principal);
                //requestMsg.SetRequestContext(requestCtx.Object);
                //var descriptor = new HttpControllerDescriptor();
                //var controller = new Mock<IHttpController>();
                //acctCtrl.ControllerContext.RequestContext.Principal = principal;
                //acctCtrl.ControllerContext = new HttpControllerContext(requestCtx.Object, requestMsg, descriptor, controller.Object );    
                //var testCall = client.PostAsJsonAsync("http://localhost/Pims.Web.Api/api/Account/ManageAsync", editedPassword).Result;
                // Mimic user login.
                // Validate currently logged in User and their existence in ASP.NET Identity.
                //var loggedInUser2 = _userMgr.FindAsync(_login.UserName, _login.Password).Result;

                // mimic SignInAsync()
                //var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                //authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                //var claimsIdentity = await _userMgr.CreateIdentityAsync(loggedInUser, DefaultAuthenticationTypes.ApplicationCookie);
                //authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true }, claimsIdentity);



                //// Identity created with added claim.
                //var identity = new GenericIdentity(loggedInUser.UserName);
                //identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", loggedUserId));

                //// Principal created with associated Identity.
                //var principal = new GenericPrincipal(identity, new[] {"user"});

                //var urlHelper = new Mock<UrlHelper>();
                //urlHelper.Setup(s => s.Link(It.IsAny<string>(), It.IsAny<object>()))
                //    .Returns("http://localhost/PIMS.Web.Api/api/Account");

                //// HttpRequestContext created
                //var requestCtx = new Mock<HttpRequestContext>();
                //requestCtx.Setup(s => s.Url).Returns(urlHelper.Object);
                //requestCtx.SetupGet(s => s.Principal).Returns(principal);

                //// Set HttpRequestMessage on HttpRequestContext via HttpConfiguration object.
                //var config = new HttpConfiguration();
                //config.Routes.MapHttpRoute(
                //                            name: "ManageRoute",
                //                            routeTemplate: "api/Account/ManageAsync" 
                //                           );

                //var requestMsg = TestHelpers.GetHttpRequestMessage(
                //                                HttpMethod.Post,
                //                                UrlBase + "/ManageAsync",
                //                                new AccountController(_userMgr),
                //                                "ManageRoute",
                //                                "api/Account/ManageAsync",
                //                                new { }
                //                                );

                //// Set required objects for HttpControllerContext.
                //requestMsg.SetRequestContext(requestCtx.Object);
                //var descriptor = new HttpControllerDescriptor();
                //var controller = new Mock<IHttpController>();
                //var accountCtrl = new AccountController(_userMgr)
                //           {
                //               ControllerContext = new HttpControllerContext
                //                                            (
                //                                               requestCtx.Object,
                //                                               requestMsg, 
                //                                               descriptor, 
                //                                               controller.Object
                //                                            )
                //           };

                //client.DefaultRequestHeaders.Add("UserId", loggedInUser.Id);
                //client.BaseAddress = new Uri(UrlBase);
                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //var settings = new JsonSerializerSettings();
                //var serializer = JsonSerializer.Create(settings);
                //var editedPassword = new ManageUserModel
                //                            {
                //                                OldPassword = _login.Password,
                //                                NewPassword = "pwrd0811c",
                //                                ConfirmPassword = "pwrd0811c"
                //                                //UserId = loggedUserId
                //                            };

                //var j = JObject.FromObject(editedData, serializer);
                //HttpContent content = new StringContent(j.ToString());
                //content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //var contentData = content.ReadAsStringAsync();

                // Act
                //var claimsIdentityFactory = new PimsClaimsIdentityFactory();
                //var claimsIdentity = claimsIdentityFactory.CreateClaimsIdentity(_login);
                //IList<Claim> claimsCollection = new List<Claim>
                //                                {
                //                                    new Claim(ClaimTypes.Name, "Richard"),
                //                                    new Claim(ClaimTypes.PostalCode, "94065"),
                //                                    new Claim(ClaimTypes.MobilePhone, "650.465.3609"),
                //                                    new Claim(ClaimTypes.Locality, "Redwood Shores")
                //                                };
                //NHibernate.AspNet.Identity.IdentityUserClaim nhClaim = new IdentityUserClaim();

                //var claimsIdentity = new ClaimsIdentity(claimsCollection, "PIMS test authType");
                //claimsIdentity.AddClaims(claimsCollection);
                //var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            #endregion

            using (new HttpClient())
            {
                // Arrange 
                var loggedInUser = _userMgr.FindAsync(_login.UserName, _login.Password).Result;
                var acctCtrl = new AccountController(_userMgr);
                
                // Assumes valid loggedInUser.
                IList<Claim> claimsCollection = new List<Claim>
                                                {
                                                    new Claim(ClaimTypes.Name, loggedInUser.UserName),
                                                    new Claim(ClaimTypes.NameIdentifier, loggedInUser.Id),
                                                    new Claim(ClaimTypes.PostalCode, "94065"),
                                                    new Claim(ClaimTypes.StateOrProvince, "California")
                                                };

                // Associate claims with Identity 
                var claimsIdentity = new ClaimsIdentity(claimsCollection, "PIMS web site");

                // Associate Identity with Principal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                Thread.CurrentPrincipal = claimsPrincipal;                             
     
                // Setup: RequestContext-RequestMessage-HttpConfiguration
                var requestCtx = new Mock<HttpRequestContext>();
                requestCtx.SetupGet(s => s.Principal).Returns(claimsPrincipal);
                var config = new HttpConfiguration();
                var route = config.Routes.MapHttpRoute(
                    name: "ManageRoute",
                    routeTemplate: "api/{controller}/ManageAsync",
                    defaults: new {}
                    );

                var routeData = new HttpRouteData(route, new HttpRouteValueDictionary {{"controller", "Account"}});
                var requestMsg = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Pims.Web.Api/api/Account/ManageAsync");
                requestMsg.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                requestMsg.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());
           
                acctCtrl.ControllerContext = new HttpControllerContext(config, routeData, requestMsg);
                acctCtrl.Request = requestMsg;
                acctCtrl.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
             
                acctCtrl.ControllerContext.RequestContext.Principal = claimsPrincipal;
              
               
                var userEdits = new ManageUserModel {
                                                        OldPassword = _login.Password,
                                                        NewPassword = "pwrd0828g",
                                                        ConfirmPassword = "pwrd0828g"
                                                    };

                
                // Act
                // Confirm userEdits & _login passwords are configured correctly.
                var actionResult = acctCtrl.ManageAsync(userEdits).Result;
                var loggedInUserModified = _userMgr.FindAsync(_login.UserName, userEdits.NewPassword).Result;
                // Create response message.
                var responseMsg = actionResult.ExecuteAsync(new CancellationToken(false));

                
                // Assert
                
                Assert.AreEqual(responseMsg.Result.StatusCode, HttpStatusCode.OK);
                Assert.IsNotNullOrEmpty(loggedInUserModified.UserName);

            }
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Middleware_can_not_sucessfully_update_a_users_account_based_on_invalid_password()
        {
            using (new HttpClient())
            {
                // Arrange 
                var loggedInUserValid = _userMgr.FindAsync(_login.UserName, _login.Password).Result;
                var acctCtrl = new AccountController(_userMgr);

                // Assumes valid loggedInUser.
                IList<Claim> claimsCollection = new List<Claim>
                                                {
                                                    new Claim(ClaimTypes.Name, loggedInUserValid.UserName),
                                                    new Claim(ClaimTypes.NameIdentifier, loggedInUserValid.Id),
                                                    new Claim(ClaimTypes.PostalCode, "94065"),
                                                    new Claim(ClaimTypes.StateOrProvince, "California")
                                                };

                // Associate claims with Identity 
                var claimsIdentity = new ClaimsIdentity(claimsCollection, "PIMS web site");

                // Associate Identity with Principal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                Thread.CurrentPrincipal = claimsPrincipal;

                // Setup: RequestContext-RequestMessage-HttpConfiguration
                var requestCtx = new Mock<HttpRequestContext>();
                requestCtx.SetupGet(s => s.Principal).Returns(claimsPrincipal);
                var config = new HttpConfiguration();
                var route = config.Routes.MapHttpRoute(
                    name: "ManageRoute",
                    routeTemplate: "api/{controller}/ManageAsync",
                    defaults: new { }
                    );

                var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "Account" } });
                var requestMsg = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Pims.Web.Api/api/Account/ManageAsync");
                requestMsg.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                requestMsg.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

                acctCtrl.ControllerContext = new HttpControllerContext(config, routeData, requestMsg);
                acctCtrl.Request = requestMsg;
                acctCtrl.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

                acctCtrl.ControllerContext.RequestContext.Principal = claimsPrincipal;

                // Change to invalid password.
                var userEdits = new ManageUserModel {
                                                        OldPassword = "abadpassword",
                                                        NewPassword = "pwrd0828h",
                                                        ConfirmPassword = "pwrd0828h"
                                                    };


                // Act

                var actionResult =  acctCtrl.ManageAsync(userEdits).Result;
                // Create response message.
                var responseMsg = actionResult.ExecuteAsync(new CancellationToken(false));


                // Assert

                Assert.AreNotEqual(responseMsg.Result.StatusCode, HttpStatusCode.OK);
                Assert.IsTrue(responseMsg.Result.ReasonPhrase.Contains("Unable to update password"));

            }
        }


        [Test]
        // ReSharper disable once InconsistentNaming
        public void Middleware_can_not_sucessfully_update_a_users_account_based_on_invalid_password_confirmations()
        {
            using (new HttpClient())
            {
                // Arrange 
                var loggedInUserValid = _userMgr.FindAsync(_login.UserName, _login.Password).Result;
                var acctCtrl = new AccountController(_userMgr);

                // Assumes valid loggedInUser.
                IList<Claim> claimsCollection = new List<Claim>
                                                {
                                                    new Claim(ClaimTypes.Name, loggedInUserValid.UserName),
                                                    new Claim(ClaimTypes.NameIdentifier, loggedInUserValid.Id),
                                                    new Claim(ClaimTypes.PostalCode, "94065"),
                                                    new Claim(ClaimTypes.StateOrProvince, "California")
                                                };

                // Associate claims with Identity 
                var claimsIdentity = new ClaimsIdentity(claimsCollection, "PIMS web site");

                // Associate Identity with Principal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                Thread.CurrentPrincipal = claimsPrincipal;

                // Setup: RequestContext-RequestMessage-HttpConfiguration
                var requestCtx = new Mock<HttpRequestContext>();
                requestCtx.SetupGet(s => s.Principal).Returns(claimsPrincipal);
                var config = new HttpConfiguration();
                var route = config.Routes.MapHttpRoute(
                    name: "ManageRoute",
                    routeTemplate: "api/{controller}/ManageAsync",
                    defaults: new { }
                    );

                var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "Account" } });
                var requestMsg = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Pims.Web.Api/api/Account/ManageAsync");
                requestMsg.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                requestMsg.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());

                acctCtrl.ControllerContext = new HttpControllerContext(config, routeData, requestMsg);
                acctCtrl.Request = requestMsg;
                acctCtrl.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

                acctCtrl.ControllerContext.RequestContext.Principal = claimsPrincipal;

                // Change to invalid confirmation passwords.
                var userEdits = new ManageUserModel
                                {
                                    OldPassword = _login.Password,
                                    NewPassword = "pwrd0828i",
                                    ConfirmPassword = "pwrd0828j"
                                };


                // Act
                var actionResult = acctCtrl.ManageAsync(userEdits).Result;
                var loggedInUserUnModified = _userMgr.FindAsync(_login.UserName, userEdits.OldPassword).Result;
                // Create response message.
                var responseMsg = actionResult.ExecuteAsync(new CancellationToken(false));


                // Assert
                Assert.IsTrue(responseMsg.Result.StatusCode == HttpStatusCode.BadRequest);
                Assert.IsNotNullOrEmpty(loggedInUserUnModified.Id);

            }
        }
    }
}
