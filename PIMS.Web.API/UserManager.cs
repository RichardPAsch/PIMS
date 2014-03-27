using PIMS.Core.Models;
using PIMS.Data.Repositories;
using PIMS.Web.Api.TypeMappers;
using PIMS.Web.Common.Security;


namespace PIMS.Web.Api
{
    public class UserManager : IUserManager
    {
        // see p.72 as reference
        private readonly IMembershipInfoProvider _membershipAdapter;
        private readonly IUserRepository _userRepository;
        private readonly IUserMapper _userMapper;

        // ctor 
        public UserManager(IMembershipInfoProvider membershipAdapter, IUserRepository userRepository, IUserMapper userMapper) {
                                        _membershipAdapter = membershipAdapter;
                                        _userRepository = userRepository;
                                        _userMapper = userMapper;
        }


        public User CreateUser(string username, string password, string firstname, string lastname, string email) {

            var wrapper = _membershipAdapter.CreateUser(username, password, email);

            _userRepository.SaveUser(wrapper.UserId, firstname, lastname);

            var user = _userMapper.CreateUser(username, firstname, lastname, email, wrapper.UserId);

            return user;
           

        }
    }
}
