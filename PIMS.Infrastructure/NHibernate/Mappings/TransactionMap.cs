﻿using FluentNHibernate.Mapping;
using Transaction = PIMS.Core.Models.Transaction;


namespace PIMS.Infrastructure.NHibernate.Mappings
{
    public class TransactionMap : ClassMap<Transaction>
    {
        public TransactionMap()
        {
            Table("Transactions");
            Id(x => x.TransactionId, "TransactionId")
                .GeneratedBy
                .GuidComb();

            Map(x => x.Action);
            Map(x => x.Units);
            Map(x => x.MktPrice).Precision(7).Scale(3);
            Map(x => x.Fees).Precision(6).Scale(2);
            Map(x => x.Date);
            Map(x => x.UnitCost).Precision(9).Scale(4);
            Map(x => x.CostBasis).Precision(8).Scale(2);
            Map(x => x.Valuation).Precision(9).Scale(2);
            Map(x => x.TransactionPositionId);  

            /*
                FK MUST 'Allow Nulls", which allows insertion of a null value into the table. 
                Therefore, if you have a non-nullable column, with NO default value, a SQL Exception will be generated.
            */

            // M:1
            // One or more Transaction records may be associated with a Position.
            // References other side (one) of Position NH 1:M relationship.
            References(x => x.TransactionPosition) // NH mapping for '1' side of relation to Position.
                .Column("TransactionPositionId") // NH FK column in Transactions table.
                .Not.Update()
                .Not.Insert();
        }
    }



}
