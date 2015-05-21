using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PIMS.Core.Models;
using PIMS.Data.Repositories;


namespace PIMS.Data.FakeRepositories
{
    public class InMemoryAccountTypeRepository : IGenericRepository<AccountType>
    {
        public string UrlAddress { get; set; }
        

        public IQueryable<AccountType> RetreiveAll()
        {
            var accountsListing = new List<AccountType>
                                  {
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/33f4b62f-bcd4-4d5f-8b2d-373d628a5dfc",
                                            AccountTypeDesc = "Roth-IRA",
                                            KeyId = new Guid("33f4b62f-bcd4-4d5f-8b2d-373d628a5dfc"),
                                            PositionRefId = new Guid("96b5f8bd-21f2-4921-bc5b-bcff6e39cbdb")
                                        },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/0ff19b90-8628-41fd-bef3-6b3f237f19a6",
                                            AccountTypeDesc = "CMA", 
                                            KeyId = new Guid("0ff19b90-8628-41fd-bef3-6b3f237f19a6"),
                                            PositionRefId = new Guid("0409fcc8-f12b-49f9-86fd-c572560cfe15")
                                         },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/edc913b3-5ec2-46c7-90d1-38e88a86776b",
                                            AccountTypeDesc = "IRRA", 
                                            KeyId = new Guid("edc913b3-5ec2-46c7-90d1-38e88a86776b"),
                                            PositionRefId = new Guid("d427c6be-37e4-45f2-93d9-a97956fd3931")
                                        },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/98228ef0-c3f1-43b1-b640-9c84e13bb99b",
                                            AccountTypeDesc = "IRRA", 
                                            KeyId = new Guid("98228ef0-c3f1-43b1-b640-9c84e13bb99b"),
                                            PositionRefId = new Guid("6172313a-20d1-47b6-9a7e-de1fe80a9cc8")
                                        },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/7bfe50e5-b1e5-416c-862a-f5fde64ec7c9",
                                            AccountTypeDesc = "Roth-IRA", 
                                            KeyId = new Guid("7bfe50e5-b1e5-416c-862a-f5fde64ec7c9"),
                                            PositionRefId = new Guid("f5cf9bd6-e870-40f0-afb2-b1c7337896aa")
                                        },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/be984208-a260-41fa-b125-e855aee37b4f",
                                            AccountTypeDesc = "ML-CMA", 
                                            KeyId = new Guid("be984208-a260-41fa-b125-e855aee37b4f"),
                                            PositionRefId = new Guid("a03174a3-d5cc-49b1-99aa-62f993bff553")
                                        },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/4aac6a37-69fc-4a64-83c8-279562a7be50",
                                            AccountTypeDesc = "Roth-IRA", 
                                            KeyId = new Guid("4aac6a37-69fc-4a64-83c8-279562a7be50"),
                                            PositionRefId = new Guid("11f5afe6-7fb9-429c-877a-a3c700a4e8aa")
                                        },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/9adb088c-d42d-4f0b-979b-7a1c17276637",
                                            AccountTypeDesc = "Roth-IRA", 
                                            KeyId = new Guid("9adb088c-d42d-4f0b-979b-7a1c17276637"),
                                            PositionRefId = new Guid("03345f0f-00d2-4bb8-ad18-3928217337ea")
                                        },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/AAPL/Position/Account/0a37302b-1ffd-4f7e-8f9b-da24d55fde15",
                                            AccountTypeDesc = "ML-CMA", 
                                            KeyId = new Guid("0a37302b-1ffd-4f7e-8f9b-da24d55fde15"),
                                            PositionRefId = new Guid("04838c46-9574-41e3-a866-a686b79e53c7")
                                        },
                                      new AccountType
                                        {
                                            Url = "http://localhost/Pims.Web.Api/api/Asset/VNR/Position/Account/4a2e9df2-7de0-4285-9234-a193adcb5449",
                                            AccountTypeDesc = "ML-CMA", 
                                            KeyId = new Guid("4a2e9df2-7de0-4285-9234-a193adcb5449"),
                                            PositionRefId = new Guid("2a64098f-e408-45ac-969f-6cfe566ef249")
                                        }
                                  };

            return accountsListing.AsQueryable();
        }


        public IQueryable<AccountType> Retreive(Expression<Func<AccountType, bool>> predicate)
        {
            return RetreiveAll().Where(predicate);
        }


        public AccountType RetreiveById(Guid key)
        {
            return RetreiveAll().Single(at => at.PositionRefId == key);
        }
        

        public bool Create(AccountType newEntity)
        {
            //TODO: call AccountTypeController.GetAllAccountsForInvestor() to get available types? How done in SQL?
            IList<AccountType> currentAccounts = Retreive(p => p.PositionRefId == newEntity.PositionRefId).ToList();
            currentAccounts.Add(newEntity);

            return true;
        }


        public bool Delete(Guid idGuid)
        {

            return false;
        }


        public bool Update(AccountType entity, object id)
        {
            // Update ALL referencing Asset Positions using the old account type.
            RetreiveById(entity.PositionRefId);
            

            return true;
        }

        
    }


    // For in-memory testing.
    public static class GenericRepositoryExtension
    {
        public static IList<string> RetreiveLookUpAccounts(this IGenericRepository<AccountType> test)
        {
             return new List<string>
                   {
                        "Roth-IRA",
                        "CMA",
                        "IRRA",
                        "SEP-IRA",
                        "Simple IRA",
                        "Money Market",
                        "IRA"
                   };
        }
    }


}
