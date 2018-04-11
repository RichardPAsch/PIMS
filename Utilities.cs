using System;
using System.Globalization;
using System.Linq;
using System.Web.Http.Results;
using PIMS.Core.Models;
using PIMS.Core.Models.ViewModels;
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

       
        public static IQueryable<Guid> GetAspNetUserId(IGenericRepository<Investor> repositoryInvestor, Guid investorId)
        {
            return repositoryInvestor.Retreive(x => x.InvestorId == investorId).Select(i => i.AspNetUsersId);
        }


        public static string GetBaseUrl(string sourceUrl)
        {
            var baseIdx = sourceUrl.LastIndexOf("api/", System.StringComparison.Ordinal);
            return sourceUrl.Substring(0, baseIdx + 4);
        }


        public static string GetInvestor(IPimsIdentityService identityService)
        {
            return identityService.CurrentUser ?? "maryblow@yahoo.com";
        }


        public static int GetDaysInMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }


        public static Transaction MapVmToTransaction(TransactionVm sourceData) {
            return new Transaction {
                Action = sourceData.TransactionEvent,
                TransactionId = sourceData.TransactionId,
                TransactionPositionId = sourceData.PositionId,
                Units = sourceData.Units,
                MktPrice = sourceData.MktPrice,
                Valuation = sourceData.Valuation,
                Fees = sourceData.Fees,
                CostBasis = sourceData.CostBasis,
                UnitCost = sourceData.UnitCost,
                Date = DateTime.Now
            };
        }


        public static Position MapVmToPosition(PositionVm sourceData) {

            return new Position {
                // ReSharper disable once PossibleInvalidOperationException
                PurchaseDate = (DateTime)sourceData.DateOfPurchase,
                PositionDate = sourceData.DatePositionAdded != null ? DateTime.Parse(sourceData.DatePositionAdded.ToString()) : DateTime.Now,
                Quantity = sourceData.Qty,
                UnitCost = sourceData.UnitCost,
                AcctTypeId = sourceData.PostEditPositionAccount == null
                                    ? new Guid(sourceData.PreEditPositionAccount)
                                    : new Guid(sourceData.PostEditPositionAccount),
                PositionAssetId = sourceData.ReferencedAssetId,
                LastUpdate = DateTime.Now,
                Status = char.Parse(sourceData.Status),
                Fees = sourceData.TransactionFees,
                PositionId = sourceData.CreatedPositionId,
                InvestorKey = sourceData.LoggedInInvestor
            };
            
        }


        public static decimal CalculateUnitCost(decimal costBasis, decimal units)
        {
            return costBasis / units;
        }


        public static decimal CalculateValuation(decimal mktPrice, decimal units) {
            return mktPrice * units;
        }


        public static decimal CalculateCostBasis(decimal fees, decimal valuation)
        {
            return fees + valuation;
        }


        public static decimal CalculateDividendYield(decimal divRate, decimal unitPrice)
        {
            // Dividend yield = ANNUALIZED dividend/share price.
            var yield = divRate * 12 / unitPrice * 100;

            // Due to occasional erroneous divRate results by Tiingo API, e.g., ACRNX, we'll cap
            // the resultant yield to avoid an 'out of range' exception upon posting to the db.
            return yield >= 25 
                ? decimal.Round(decimal.Parse("25")) 
                : decimal.Round(decimal.Parse(yield.ToString(CultureInfo.InvariantCulture)), 2);
        }


        public static string GetWebServerBaseUri(string uriPath)
        {
            var idx1 = uriPath.IndexOf("PIMS", StringComparison.Ordinal);
            return uriPath.Substring(0,idx1);
        }


        public static string ParseAccountTypeFromDescription(string accountDesc)
        {
            // Some custodial agents, e.g., Merrill Lynch, may define accounts with some variation.
            if (accountDesc.IndexOf("IRRA", StringComparison.Ordinal) == 0)
                accountDesc = "IRA";

            // Any account description that includes superfluous data, e.g.,account number, will
            // be truncated to the appropriate account type. Primarily used during XLSX revenue processing.

            if (accountDesc.ToUpper().IndexOf("IRA", StringComparison.Ordinal) >= 0 && accountDesc.ToUpper().IndexOf("ROTH", StringComparison.Ordinal) == -1)
                return "IRA";
            if (accountDesc.ToUpper().IndexOf("ROTH", StringComparison.Ordinal) >= 0 && accountDesc.ToUpper().IndexOf("IRA", StringComparison.Ordinal) >= 0)
                return "ROTH-IRA";

            return accountDesc.ToUpper().IndexOf("CMA", StringComparison.Ordinal) >= 0 ? "CMA" : null;
        }
    }

        
}