using System;
using PIMS.Core.Models;

namespace PIMS.Data.Repositories
{
    public interface IUserRepository
    {
        void SaveUser(Guid userId, string firstname, string lastname);
        User GetUser(Guid userId);
    }
}
