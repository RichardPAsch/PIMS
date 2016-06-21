using System;
using System.Linq;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.Repositories;


namespace PIMS.Web.Api.Common
{

    public static class Utilities
    {
        //private static IPimsIdentityService _identityService;


        public static Guid GetInvestorId(IGenericRepository<Investor> repositoryInvestor, string investorLogin) {
            return repositoryInvestor.Retreive(i => i.EMailAddr.Trim() == investorLogin.Trim()).First().InvestorId;
        }

        public static string GetBaseUrl(string sourceUrl)
        {
            var baseIdx = sourceUrl.LastIndexOf("api/", System.StringComparison.Ordinal);
            return sourceUrl.Substring(0, baseIdx + 4);
        }

        public static string GetInvestor(IPimsIdentityService identityService)
        {
            return identityService.CurrentUser ?? "rpasch2@rpclassics.net";
        }

        public static int GetDaysInMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }
    }

        
}