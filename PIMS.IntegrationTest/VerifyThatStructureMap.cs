using NHibernate;
using PIMS.Core.Models;
using PIMS.Data.Repositories;
using NUnit.Framework;
using PIMS.Web.Api.DependencyResolution;



namespace PIMS.IntegrationTest
{

    [TestFixture]
    public class VerifyThatStructureMap
    {
        StructureMap.IContainer _smContainer;

        [SetUp]
        public void Init()
        {
            // Uses System Under Test (WebApi) for IoC/DI initializations.
            _smContainer = IoC.Initialize();
        }
        

        [Test]
        // ReSharper disable once InconsistentNaming
        public void Is_correctly_configured_via_valid_registration_mappings() {

            // Act
            var configurationForProfileExist = _smContainer.Model.HasImplementationsFor<IGenericRepository<Profile>>();
            var configurationForPositionExist = _smContainer.Model.HasImplementationsFor<IGenericRepository<Position>>();
            var configurationForIncomeExist = _smContainer.Model.HasImplementationsFor<IGenericRepository<Income>>();
            var configurationForAssetExist = _smContainer.Model.HasImplementationsFor<IGenericRepository<Asset>>();
            //var positionRepo = _smContainer.GetInstance<IPositionRepository>();
            var nHSessFactory = _smContainer.Model.HasImplementationsFor<ISessionFactory>();

            // Assert
            Assert.IsTrue(configurationForAssetExist && configurationForIncomeExist && configurationForPositionExist && configurationForProfileExist);
            //Assert.That(positionRepo, Is.TypeOf<PositionRepository>());
            Assert.IsTrue(nHSessFactory);

        }



       

    }
}
