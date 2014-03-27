using NHibernate;
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
            var profileRepo = _smContainer.GetInstance<IProfileRepository>();
            var assetRepo = _smContainer.GetInstance<IAssetRepository>();
            var incomeRepo = _smContainer.GetInstance<IIncomeRepository>();
            var positionRepo = _smContainer.GetInstance<IPositionRepository>();
            var nHSessFactory = _smContainer.Model.HasImplementationsFor<ISessionFactory>();

            // Assert
            Assert.That(profileRepo, Is.TypeOf<ProfileRepository>());
            Assert.That(positionRepo, Is.TypeOf<PositionRepository>());
            Assert.That(assetRepo, Is.TypeOf<AssetRepository>());
            Assert.That(incomeRepo, Is.TypeOf<IncomeRepository>());
            Assert.IsTrue(nHSessFactory);

        }



       

    }
}
