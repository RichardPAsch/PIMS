//using System;
//using System.Configuration;
//using Microsoft.AspNet.Identity;
//using NHibernate.AspNet.Identity;
//using NUnit.Framework;
//using PIMS.Core.Models;
//using PIMS.Infrastructure.UserProfile;
//using PIMS.Web.Api;
//using PIMS.Web.Api.Controllers;


namespace PIMS.UnitTest.Security
{
    //[TestFixture]
    //public class VerifyAuthentication
    //{
    //    private AccountController _ctrl;
    //    //private const string UrlBase = "http://localhost/PIMS.Web.API/api/Account";
    //    private RegistrationModel _registration;
    //    private UserManager<ApplicationUser> _userMgr;
    //    private readonly string _connString = ConfigurationManager.ConnectionStrings["PIMS-ConnString"].ConnectionString;
        


    //    [SetUp]
    //    public void Init()
    //    {
    //        _registration = new RegistrationModel
    //                        {
    //                            UserName = "RPA_" + DateTime.Now.Minute + DateTime.Now.Second,
    //                            Password = "pwrd1",
    //                            ConfirmPassword = "pwrd1"
    //                        };

    //        _userMgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(NHibernateConfiguration.CreateSessionFactory(_connString).OpenSession()));
    //    }


    //    [Test]
    //    // ReSharper disable once InconsistentNaming
    //    public void Controller_Can_POST_a_new_Registerd_User() {

    //        // Arrange
    //        _ctrl = new AccountController(_userMgr); 
    //        var request = TestHelpers.GetHttpRequestMessage();

    //        // Act
    //        var result = _ctrl.RegisterUser(_registration, request);
  

    //        // Assert
    //        Assert.IsTrue(result.IsCompleted);
    //    }

        

    //}
}

