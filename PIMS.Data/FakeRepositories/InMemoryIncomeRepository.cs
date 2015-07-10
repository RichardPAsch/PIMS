using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryIncomeRepository : IGenericRepository<Income>
    {
        public string UrlAddress { get; set; }

        // Retreive() used by Aggregate root repository linking, when fetching child aggregate Incomes.
        public IQueryable<Income> RetreiveAll()
        {
            var incomeListing = new List<Income>
                {
                         new Income
                            {
                                Url = "",
                                AssetId = new Guid("55224b18-5777-48b1-a9a1-28fb74d385f3"),
                                Actual = 56.11M, 
                                DateRecvd = new DateTime(2014,04,17),
                                Projected = 52.56M,
                                IncomeId = new Guid("967c6f6c-9e1c-4be9-a247-461f21cf1d0f"),
                                Account = "Roth-IRA"
                            },
                            new Income
                            {
                                Url = "",
                                Actual = 73.19M, 
                                DateRecvd = new DateTime(2014,05,15),
                                AssetId = new Guid("55224b18-5777-48b1-a9a1-28fb74d385f3"),
                                Projected = 72.56M,
                                IncomeId = new Guid("120fdca2-4704-4db5-b63f-cea29a9ffeee"),
                                Account = "ML-CMA"
                            } ,
                            new Income
                            {
                                Url = "",
                                Actual = 131.22M, 
                                DateRecvd = new DateTime(2014,09,25),
                                AssetId = new Guid("216d1d40-e63c-4fc8-bee9-0dddf343c0aa"),
                                Projected = 99.50M,
                                IncomeId = new Guid("1cf85157-fe6b-4204-8d8b-f7647535372d"),
                                Account = "Roth-IRA"
                            } ,
                            new Income
                            {
                                Url = "",
                                Actual = 53.19M, 
                                DateRecvd = new DateTime(2014,07,10),
                                AssetId = new Guid("55224b18-5777-48b1-a9a1-28fb74d385f3"),
                                Projected = 72.56M,
                                IncomeId = new Guid("589e0468-ee30-4649-9ee0-abc4f1dbed94"),
                                Account = "Roth-IRA"
                            } ,
                            new Income
                            {
                                Url = "",
                                Actual = 60.99M, 
                                DateRecvd = new DateTime(2014,08,13),
                                AssetId = new Guid("55224b18-5777-48b1-a9a1-28fb74d385f3"),
                                Projected = 72.56M,
                                IncomeId = new Guid("8760c26c-b881-410b-a711-0401ec5838a3"),
                                Account = "Roth-IRA"
                            } ,
                            new Income
                            {
                                Url = "", //UrlAddress + "/YHO/Income",
                                Actual = 5.31M, 
                                DateRecvd = new DateTime(2012,01,10), 
                                AssetId = new Guid("f9cea918-798b-4323-884f-917090b23858"),
                                Projected = 62.26M,
                                IncomeId = new Guid("6425020c-35db-4766-922d-8a40fab66f5c"),
                                Account = "ML-CMA"
                            } ,
                            new Income
                            {
                                Url = "", 
                                Actual = 191.09M, 
                                DateRecvd = new DateTime(2014,01,7), 
                                AssetId = new Guid("cc950e42-1f08-49b9-880b-a35f4d60b317"),
                                Projected = 198.44M,
                                IncomeId = new Guid("b48712d4-b245-427f-8678-2e457483aeb6"),
                                Account = "IRRA"
                            } ,
                            new Income
                            {
                                Url = "", 
                                Actual = 216.91M, 
                                DateRecvd = new DateTime(2014,04,30), 
                                AssetId = new Guid("1a4edffd-bc30-44be-a1df-98e096308ac9"),
                                Projected = 212.56M,
                                IncomeId = new Guid("a142835a-2eac-4070-84dd-844bc2ad3a4f"),
                                Account = "ML-CMA"
                            } ,
                             new Income
                            {
                                Url = "", 
                                Actual = 211.60M, 
                                DateRecvd = new DateTime(2014,11,05), 
                                AssetId = new Guid("1a4edffd-bc30-44be-a1df-98e096308ac9"),
                                Projected = 212.56M,
                                IncomeId = new Guid("aaf72244-b50f-4c2b-8442-70303270ccd8"),
                                Account = "ML-CMA"
                            } ,
                            new Income
                            {
                                Url = "", 
                                Actual = 56.29M, 
                                DateRecvd = new DateTime(2013,12,19), 
                                AssetId = new Guid("216d1d40-e63c-4fc8-bee9-0dddf343c0aa"), 
                                Projected = 52.56M,
                                IncomeId = new Guid("243975ec-523f-4380-99b6-7910f059eb5e"),
                                Account = "Roth-IRA"
                            } 
                        
                };

            return incomeListing.AsQueryable();

        }

        public IQueryable<Income> Retreive(Expression<Func<Income, bool>> predicate) {
            return RetreiveAll().Where(predicate);
        }

        public Income RetreiveById(Guid key) {
            return null;
        }


        // The following 3 methods are superceeded by Aggregate functionality located with the
        // aggregate root, e.g., AssetRepository.
        public bool Create(Income newEntity) {
            return false;
        }

        public bool Delete(Guid idGuid) {
            return false;
        }

        public bool Update(Income entity, object id) {
            return false;
        }


    }

}
