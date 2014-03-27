using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using NHibernate;
using NHibernate.Context;
using PIMS.Data.Repositories;
using StructureMap;



namespace PIMS.Web.Api.Controllers
{
    public class HomeController : ApiController
    {
        //private readonly ISession _session;

        //// test of IoC/DI
        //public HomeController(ISession session)
        //{
        //    _session = session;
            
        //}


        //public string Index2()
        //{
        //    if (HttpContext.Session != null)
        //    {
        //        var s = HttpContext.Session.SessionID;
                
        //    }
        //    //var y = typeof (CurrentSessionContext).GUID;
        //    //var x = WebApiApplication.NhSessionFactory;
        //    ViewBag.Controller = "Home";
        //    ViewBag.Action = "Index";
        //    //return View(); RPA: 9/25
        //    return "<div style='background-color:yellow'><b>Controller is: " + ViewBag.Controller + ";  Action is: " + ViewBag.Action + "</b></div>";
        //}

        //public ActionResult Index() {

        //    return View(); 
        //}


    }
}
