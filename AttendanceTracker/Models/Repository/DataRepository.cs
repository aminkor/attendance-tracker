﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AttendanceTracker.Models.Repository;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AttendanceTracker.Models.Repository
{
    public class DataRepository<T> : IDataRepository<T>
       where T : class
    {

        protected readonly AttendanceTracker_DevContext _context;

        public DataRepository(AttendanceTracker_DevContext context)
        {

            _context = context;
        }

        public IQueryable<T> GetAll()
        {
            return _context.Set<T>();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        //public T Get(int key)
        //{

        //    var record = _context.Set<T>().Find(key);

        //    return record;
        //}

        //public T Get(long key)
        //{
        //    var record = _context.Set<T>().Find(key);

        //    return record;
        //}

        //public T Get(string key)
        //{
        //    var record = _context.Set<T>().Find(key);

        //    return record;
        //}

        //public IQueryable<T> GetAndIncludes<T>(string[] includes = null) where T : class
        //{
        //    //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
        //    if (includes != null && includes.Count() > 0)
        //    {
        //        var query = _context.Set<T>().Include(includes.First());
        //        foreach (var include in includes.Skip(1))
        //            query = query.Include(include);
        //        return query.AsQueryable();
        //    }

        //    return _context.Set<T>().AsQueryable();
        //}

        public virtual T InsertOnCommit(T entity)
        {

            _context.Entry(entity).State = EntityState.Added;
            
            return entity;
        }

        public virtual T UpdateOnCommit(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public virtual void DeleteOnCommit(T entity)
        {

            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void DeletePermanentOnCommit(T entity)
        {
            _context.Entry(entity).State = EntityState.Deleted;
            // _context.Set<T>().Remove(entity);
        }

        public virtual void CommitChanges()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (ValidationException ex)
            {
                Console.WriteLine(ex.ValidationResult.ErrorMessage);
                //foreach (var eve in ex.EntityValidationErrors)
                //{
                //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //                      eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //    foreach (var ve in eve.ValidationErrors)
                //    {
                //        Console.WriteLine(@"- Property: ""{0}"", Error: ""{1}""",
                //                          ve.PropertyName, ve.ErrorMessage);
                //    }
                //}
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        //public List<GetSlotDetailResult> GetSlotDetail(int PlantId, DateTime loadingDate)
        //{
        //    //var result = new List<GetSlotDetailResult>();
        //    //try
        //    //{
        //    //var query = "Execute SpGetSlotBookDetails @PlantId";

        //    //var sqlParamPlantId = new SqlParameter("@PlantId", PlantId);

        //    //result = _context.GetGetSlotDetailResults.FromSqlRaw(query, sqlParamPlantId).ToList();

        //    //        var parameters = new[] {
        //    //             new SqlParameter("@PlantId", SqlDbType.BigInt) { Direction = ParameterDirection.InputOutput, Value = PlantId }
        //    //};

        //    //        var result = _context.GetGetSlotDetailResults.FromSqlRaw("[dbo].[SpGetSlotBookDetails] @PlantId", parameters).ToList();

        //    SqlParameter param1 = new SqlParameter("@PlantId", PlantId);
        //    SqlParameter param2 = new SqlParameter("@loadingDate", loadingDate);

        //    var result = _context.GetGetSlotDetailResults.FromSqlRaw("[dbo].[SpGetSlotBookDetails] @PlantId,@loadingDate", param1,
        //    param2).ToList();

        //    //}
        //    //catch(Exception e)
        //    //{
        //    //    Console.WriteLine(e);
        //    //}
        //    return result;
        //}

        //public List<GetSlotDetailResultPlant> GetSlotDetailPlant(int PlantId,DateTime loadingDate)
        //{
        //    //var query = "Execute SpGetSlotBookDetails_Plant2 @PlantId";

        //    //var sqlParamPlantId = new SqlParameter("@PlantId", PlantId);

        //    //var result = _context.GetGetSlotDetailResultsPlant.FromSqlRaw(query, sqlParamPlantId).ToList();

        //    //var parameters = new[] {
        //    //         new SqlParameter("@PlantId", SqlDbType.BigInt) { Direction = ParameterDirection.InputOutput, Value = PlantId }
        //    //                       };

        //    //var result = _context.GetGetSlotDetailResultsPlant.FromSqlRaw("[dbo].[SpGetSlotBookDetails_Plant2] @PlantId", parameters).ToList();

        //    SqlParameter param1 = new SqlParameter("@PlantId", PlantId);
        //    SqlParameter param2 = new SqlParameter("@loadingDate", loadingDate);

        //    var result = _context.GetGetSlotDetailResultsPlant.FromSqlRaw("[dbo].[SpGetSlotBookDetails_Plant2] @PlantId,@loadingDate", param1,
        //    param2).ToList();



        //    return result;
        //}
    }
}
