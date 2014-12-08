using System;
using System.Linq;
using PIMS.Core.Models.ViewModels;


namespace PIMS.Core.Models
{

    public static class ModelParser
    {
        // Parse original Asset for possible view model edits, and update as needed.
        public static Asset ParseAsset(AssetSummaryVm assetVmWithEdits, Asset filteredPreEditAsset, out bool isModified)
        {
            // filteredPreEditAsset is an existing single asset filtered via current investor, ticker, and position/account for
            // comparisons to submitted view model edits. Submitted view model may contain 1 or more updated attributes.

            isModified = false;

            
           
            if (filteredPreEditAsset.Profile.TickerSymbol != assetVmWithEdits.TickerSymbol && !string.IsNullOrWhiteSpace(assetVmWithEdits.TickerSymbol)) {
                filteredPreEditAsset.Profile.TickerSymbol = assetVmWithEdits.TickerSymbol;
                isModified = true;
            }
            if (filteredPreEditAsset.Profile.TickerDescription != assetVmWithEdits.TickerSymbolDescription && !string.IsNullOrWhiteSpace(assetVmWithEdits.TickerSymbolDescription)) {
                filteredPreEditAsset.Profile.TickerDescription = assetVmWithEdits.TickerSymbolDescription.Trim();
                if (!isModified) isModified = true;
            }
            if (filteredPreEditAsset.AssetClass.Code != assetVmWithEdits.AssetClassification && !string.IsNullOrWhiteSpace(assetVmWithEdits.AssetClassification)) {
                filteredPreEditAsset.AssetClass.Code = assetVmWithEdits.AssetClassification.Trim();
                if (!isModified) isModified = true;
            }

            if (filteredPreEditAsset.Positions.First() != null && filteredPreEditAsset.Positions.First().Quantity != assetVmWithEdits.Quantity && assetVmWithEdits.Quantity > 0)
            {
                filteredPreEditAsset.Positions.First().Quantity = assetVmWithEdits.Quantity;
                if (!isModified) isModified = true;
            }
            if (assetVmWithEdits.UnitPrice != default(decimal) && filteredPreEditAsset.Positions.First().UnitCost != assetVmWithEdits.UnitPrice) {
                filteredPreEditAsset.Positions.First().Quantity = assetVmWithEdits.Quantity;
                if (!isModified) isModified = true;
            }
            if (!string.IsNullOrWhiteSpace(assetVmWithEdits.AccountType) && filteredPreEditAsset.Positions.First() != null && filteredPreEditAsset.Positions.First().Account.AccountTypeDesc != assetVmWithEdits.AccountType) {
                filteredPreEditAsset.Positions.First().Account.AccountTypeDesc = assetVmWithEdits.AccountType;
                if (!isModified) isModified = true;
            }
            
            if (filteredPreEditAsset.Profile.DividendFreq != assetVmWithEdits.DividendFrequency && !string.IsNullOrWhiteSpace(assetVmWithEdits.DividendFrequency)) {
                filteredPreEditAsset.Profile.DividendFreq = assetVmWithEdits.DividendFrequency.Trim().ToUpper();
                if (!isModified) isModified = true;
            }
            if (assetVmWithEdits.IncomeRecvd != default(decimal) && filteredPreEditAsset.Revenue.First().Actual != assetVmWithEdits.IncomeRecvd) {
                filteredPreEditAsset.Revenue.First().Actual = assetVmWithEdits.IncomeRecvd;
                if (!isModified) isModified = true;
            }

            if (!string.IsNullOrWhiteSpace(assetVmWithEdits.DateRecvd) && assetVmWithEdits.DateRecvd != default(DateTime).ToString("g")) {
                filteredPreEditAsset.Revenue.First().DateRecvd = assetVmWithEdits.DateRecvd;
                if (!isModified) isModified = true;
            }

           
            return filteredPreEditAsset;

        }
    }


}
