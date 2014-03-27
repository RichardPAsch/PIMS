using System;
using System.Collections.Generic;
using PIMS.Core.Models;

//using PIMS.Data.Model;

namespace PIMS.Web.Api.TypeMappers
{
    public class UserMapper : IUserMapper
    {
        public User CreateUser(string username, string firstname, string lastname, string email, Guid userId)
        {
            return new User
                       {
                           UserId = userId,
                           UserName = username,
                           EMail = email,
                           FirstName = firstname,
                           LastName = lastname,
                           Links = new List<Link>
                                       {
                                           new Link
                                               {
                                                   Title = "self",
                                                   Rel = "self",
                                                   Href = "/api/users/" + userId
                                               },
                                           new Link
                                               {
                                                   Title = "All Users",
                                                   Rel = "all",
                                                   Href = "/api/users"
                                               }
                                       }
                       };
        }

        public User CreateUser(User modelUser)
        {
            return CreateUser(
                modelUser.UserName, 
                modelUser.FirstName,
                modelUser.LastName, 
                modelUser.EMail,
                modelUser.UserId);
        }
    }
}