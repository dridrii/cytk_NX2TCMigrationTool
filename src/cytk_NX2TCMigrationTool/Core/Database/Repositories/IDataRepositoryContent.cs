using System;
using System.Collections.Generic;

namespace cytk_NX2TCMigrationTool.src.Core.Database.Repositories
{
    public interface IDataRepository<T> where T : class
    {
        T GetById(string id);
        IEnumerable<T> GetAll();
        void Add(T entity);
        void Update(T entity);
        void Delete(string id);
    }
}