using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace AttendanceTracker.Models.Repository
{
    public interface IDataRepository<T>
       where T : class
    {
        IQueryable<T> GetAll();

        IQueryable<T> Get(Expression<Func<T, bool>> predicate);


        //T Get(int key);

        //T Get(long key);

        //T Get(string key);

        //IQueryable<T> GetAndIncludes<T>(string[] includes = null) where T : class;

        T InsertOnCommit(T entity);

        T UpdateOnCommit(T entity);

        void DeleteOnCommit(T entity);

        void DeletePermanentOnCommit(T entity);

        void CommitChanges();

        //List<GetSlotDetailResult> GetSlotDetail(int PlantId,DateTime loadingDate);

        //List<GetSlotDetailResultPlant> GetSlotDetailPlant(int PlantId,DateTime loadingDate);
    }
}
