using System.Diagnostics;
using NHibernate;


namespace PIMS.IntegrationTest
{
    // ReSharper disable once InconsistentNaming
    public class NHTestRepository1
    {
        private ISession _session;

        // ctor
        public NHTestRepository1(ISession session)
        {
            _session = session;
        }


        public void GetAllAssets()
        {
            Debug.Write("ok now");
            
        }
       

    }


}
