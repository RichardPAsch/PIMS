﻿

namespace PIMS.Core.Security
{
    public class PimsIdentityService : IPimsIdentityService
    {
        public string CurrentUser
        {
            // temp until integration test begun.
            get
            {
                return "rpasch@rpclassics.net"; // login name
                //return "Asch";

                // For PROD:
                //return Thread.CurrentPrincipal.Identity.Name;
            }
        }
    }
}

