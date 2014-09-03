using NHibernate.AspNet.Identity;


namespace PIMS.Core.Models
{
    // ** Extensible part of new ASP.NET Identity membership system, as used by OWIN/Katana middleware. **

    /*
     * Additional profile data (e.g., birthDay) for the user can be added to this class; 
     * please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    */ 

    // "ApplicationUser" - do not change name, as it maps to corresponding db table, and will result in run-time error.



    // Extend the functionality of the default IdentityUser entity as needed:
    //  1. extend the corresponding view model 
    //  2. add new attribute(s) to UI for initialization(s)
    //  3. add new attributes(s) to AspNetUsers
    public class ApplicationUser : IdentityUser
    {
        //public string FavoriteMovie { get; set; }
    }



}
