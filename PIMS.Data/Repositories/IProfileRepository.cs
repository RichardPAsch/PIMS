using PIMS.Core.Models;



namespace PIMS.Data.Repositories
{
    public interface IProfileRepository
    {
        Profile CreateProfile(Profile newProfile);

        Profile FetchProfile(long id);

        bool UpdateProfile(long id);
    }
}
