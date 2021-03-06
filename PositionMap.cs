﻿using FluentNHibernate.Mapping;
using PIMS.Core.Models;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class PositionMap : ClassMap<Position>
    {
        public PositionMap()
        {
            Table("Position");
            Id(x => x.PositionId, "PositionId")
                .GeneratedBy
                .GuidComb();

            Map(x => x.PositionAssetId);
            Map(x => x.AcctTypeId, "PositionAccountTypeId");
            Map(x => x.PurchaseDate);
            Map(x => x.Quantity);
            Map(x => x.UnitCost).Precision(7).Scale(3);
            Map(x => x.LastUpdate);
            Map(x => x.InvestorKey);   
            Map(x => x.Url);
            Map(x => x.PositionDate);  
            Map(x => x.Status);
            Map(x => x.Fees).Precision(7).Scale(2);
            
            

            // M:1
            // One or more Positions may be associated with an Asset.
            // ** Avoid cryptic NH "Invalid index n for this SqlParameterCollection with Count = n" error,
            // via Insert() & Update(). Applicable to FK exposure in M:1 relationships. **
            References(x => x.PositionAsset)    // references 'one' side of Asset/Position relationship.
                 .Column("PositionAssetId")     // FK 
                 .Update()
                 //.Not.Update() // commented 5.8.2018
                 .Not.Insert();


            // M:1
            // One or more Positions on one or more Assets may be associated with an AccountType.
            References(x => x.Account)
                .Column("PositionAccountTypeId")
                .Not.Update()
                .Not.Insert();


            // 1:M
            // Each Position will have one or more Income records.
            HasMany(x => x.PositionIncomes)  
                .Table("Income")
                .KeyColumns.Add("IncomePositionId");   // references Income FK


            // 1:M added - 4.7.17
            // Each Position will have one or more Transction records.
            HasMany(x => x.PositionTransactions)
                .Table("Transactions")
                .KeyColumn("TransactionPositionId")  // references Transactions FK commented 5.4.17
                .Not.KeyNullable()
                .KeyUpdate()
                .Inverse();                          // Make Transaction side responsible for saving.

        }
    }
}
