using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public interface IIncomeRepository
    {
        Income CreateIncome(Income newPosition);

        Income FetchIncome(long id);

        bool UpdateIncome(long id); 
    }
}