using System;

namespace PIMS.Web.Common.Security
{
    public interface IUserSession
    {
        Guid UserId { get; }
        string Firstname { get; }
        string Lastname { get; }
        string Username { get; }
        string Email { get; }
    }
}