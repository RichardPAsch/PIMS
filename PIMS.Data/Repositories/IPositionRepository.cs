using System.Linq;
using PIMS.Core.Models;


namespace PIMS.Data.Repositories
{
    public interface IPositionRepository
    {
        Position CreatePosition(Position newPosition);

        Position FetchPosition(long id);

        bool UpdatePosition(long id);

    }
}
