using System;
using PIMS.Core.Models;

namespace PIMS.Web.Api.TypeMappers
{
    public interface IUserMapper
    {
        User CreateUser(string username, string firstname, string lastname, string email, Guid userId);
        User CreateUser(User modelUser);
    }
}