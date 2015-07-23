using System;
using System.Linq;
using PIMS.Core.Models.ViewModels;


namespace PIMS.Core.Models
{

    public static class ModelParser
    {
        // Parse original Asset for possible view model edits, and update as needed. Except for:
        // Read-only : 'TickerSymbol' - change would require Asset deletion/recreation.
        //           : 'AccountType'  - change would require duplicate checks & merging logic
        //                              [see PositionController.UpdatePositionsByAsset()].

        public static Asset ParseAssetForUpdates(AssetSummaryVm assetPostEdits, Asset assetPreEdits, out bool isModified)
        {
            isModified = false;
  
            if (assetPreEdits.Profile.TickerDescription != assetPostEdits.TickerSymbolDescription && !string.IsNullOrWhiteSpace(assetPostEdits.TickerSymbolDescription)) {
                assetPreEdits.Profile.TickerDescription = assetPostEdits.TickerSymbolDescription.Trim();
                 isModified = true;
            }
            if (assetPreEdits.AssetClass.Code != assetPostEdits.AssetClassification && !string.IsNullOrWhiteSpace(assetPostEdits.AssetClassification)) {
                assetPreEdits.AssetClass.Code = assetPostEdits.AssetClassification.Trim();
                if (!isModified) isModified = true;
            }

           
            // Position attributes.
            if (!assetPreEdits.Positions.Any()) return assetPreEdits;
            var referencedPositionId = new Guid();
            var assetPreEditPositionRecord = assetPreEdits.Positions.Where(p => p.Account.AccountTypeDesc.Trim() == assetPostEdits.AccountTypePreEdit.Trim()).AsQueryable();
            if (assetPreEditPositionRecord.First().Quantity != assetPostEdits.Quantity && assetPostEdits.Quantity > 0) 
            {
                assetPreEditPositionRecord.First().Quantity = assetPostEdits.Quantity;
                assetPreEditPositionRecord.First().LastUpdate = DateTime.Now;
                referencedPositionId = assetPreEditPositionRecord.First().PositionId;
                
                if (!isModified) isModified = true;
            }
            if (assetPreEditPositionRecord.First().MarketPrice != assetPostEdits.UnitPrice) {
                assetPreEditPositionRecord.First().MarketPrice = assetPostEdits.UnitPrice;
                if (!isModified) isModified = true;
            }
            

            if (assetPreEdits.Profile.DividendFreq != assetPostEdits.DividendFrequency && !string.IsNullOrWhiteSpace(assetPostEdits.DividendFrequency)) {
                assetPreEdits.Profile.DividendFreq = assetPostEdits.DividendFrequency.Trim().ToUpper();
                if (!isModified) isModified = true;
            }


            // Income attributes.
            if (!assetPreEdits.Revenue.Any()) return assetPreEdits;
            var assetPreEditIncomeRecord = assetPreEdits.Revenue.Where(r => r.IncomePositionId == referencedPositionId).AsQueryable();
            if (assetPreEditIncomeRecord.First().Actual != assetPostEdits.IncomeRecvd)
            {
                assetPreEditIncomeRecord.First().Actual = assetPostEdits.IncomeRecvd;
                assetPreEditIncomeRecord.First().LastUpdate = DateTime.Now;
                if (!isModified) isModified = true; 
            }
            if (assetPreEditIncomeRecord.First().DateRecvd == assetPostEdits.DateRecvd) return assetPreEdits;
            assetPreEditIncomeRecord.First().DateRecvd = assetPostEdits.DateRecvd;
            if (!isModified) isModified = true;

            return assetPreEdits;
        }
    }


}
