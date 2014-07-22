using System;
using NHibernate.Mapping.ByCode;


namespace PIMS.Core.Security.Nhibernate.Identity
{
    public class UuidHexCombGeneratorDef : IGeneratorDef
    {
        private readonly object _param;

        public UuidHexCombGeneratorDef(string format) {
            if (format == null)
                throw new ArgumentNullException("format");

            _param = new { format = format };
        }

        #region Implementation of IGeneratorDef

        public string Class {
            get { return "uuid.hex"; }
        }

        public object Params {
            get { return _param; }
        }

        public System.Type DefaultReturnType {
            get { return typeof(string); }
        }

        public bool SupportedAsCollectionElementId {
            get { return false; }
        }

        #endregion
    }

}

