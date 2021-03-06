﻿using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using PIMS.Core.Models;



namespace PIMS.Data.Repositories
{
    public class ImportFileRepository 
    {
        private readonly ISession _nhSession;
        public string UrlAddress { get; set; }


        public ImportFileRepository(ISessionFactory sessFactory) {
            if (sessFactory == null)
                throw new ArgumentNullException("sessFactory");

            _nhSession = sessFactory.OpenSession();
            _nhSession.FlushMode = FlushMode.Auto;
        }



        //public IQueryable<Investor> RetreiveAll() {
        //    var investorQuery = (from investor in _nhSession.Query<Investor>() select investor);
        //    return investorQuery.AsQueryable();
        //}


        //public Investor RetreiveById(Guid idGuid) {
        //    return _nhSession.Get<Investor>(idGuid);
        //}


        //public IQueryable<Investor> Retreive(Expression<Func<Investor, bool>> predicate) {
        //    return RetreiveAll().Where(predicate);
        //}


        public bool SaveOrUpdateProfile(Profile newEntity) {
            //using (var trx = _nhSession.BeginTransaction()) {
            //    try {
            //        _nhSession.Save(newEntity);
            //        trx.Commit();
            //    }
            //    catch (Exception ex) {
            //        return false;
            //    }

                return true;
            //}
        }

        public bool SavePositions(Position[] newPositions) {
            //using (var trx = _nhSession.BeginTransaction()) {
            //    try {
            //        _nhSession.Save(newEntity);
            //        trx.Commit();
            //    }
            //    catch (Exception ex) {
            //        return false;
            //    }

            return true;
            //}
        }

        public bool SaveRevenue(Income[] newRevenue) {
            //using (var trx = _nhSession.BeginTransaction()) {
            //    try {
            //        _nhSession.Save(newEntity);
            //        trx.Commit();
            //    }
            //    catch (Exception ex) {
            //        return false;
            //    }

            return true;
            //}
        }


        //public bool Update(Investor entity, object id) {
        //    using (var trx = _nhSession.BeginTransaction()) {
        //        try {
        //            _nhSession.Merge(entity);
        //            trx.Commit();
        //        }
        //        catch (Exception) {
        //            return false;
        //        }
        //    }

        //    return true;
        //}


        //public bool Delete(Guid cGuid) {

        //    var deleteOk = true;
        //    var accountTypeToDelete = RetreiveById(cGuid);

        //    if (accountTypeToDelete == null)
        //        return false;


        //    using (var trx = _nhSession.BeginTransaction()) {
        //        try {
        //            _nhSession.Delete(accountTypeToDelete);
        //            trx.Commit();
        //        }
        //        catch (Exception) {
        //            deleteOk = false;
        //        }
        //    }

        //    return deleteOk;
        //}

    }
}


