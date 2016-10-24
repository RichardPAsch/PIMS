

using System;
using System.Threading;

namespace PIMS.Core.Security
{
    public class PimsIdentityService : IPimsIdentityService
    {
        public string CurrentUser
        {
            // temp until integration test begun.
            get
            {
                // temp: for debug/development
                //return "rpasch@rpclassics.net"; // login name
                
                // For PROD:
                return Thread.CurrentPrincipal.Identity.Name;
               
            }
        }
    }
}

