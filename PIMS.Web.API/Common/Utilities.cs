using System;
using System.Globalization;
using System.Linq;
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


        public static decimal CalculateUnitCost(decimal costBasis, int units)
        {
            return costBasis / units;
        }

        public static decimal CalculateValuation(decimal mktPrice, int units) {
            return mktPrice * units;
        }

        public static decimal CalculateCostBasis(decimal fees, decimal valuation)
        {
            return fees + valuation;
        }

        public static decimal CalculateDividendYield(decimal divRate, decimal unitPrice)
        {
            var yield = divRate * 12 / unitPrice * 100;
            return decimal.Round(decimal.Parse(yield.ToString(CultureInfo.InvariantCulture)), 2);
        }
    }

        
}