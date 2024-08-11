using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBookWeb.DataAddess.Data;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        private readonly ApplicationDBContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDBContext db)
        {
            _db = db;

            //_db.categories == dbSet /Starting approach
            this.dbSet = _db.Set<T>();

        }//Above declaration and this method is called dependency injection process 
        public void Add(T entity)
        {
            //_db.Categories.Add() /Starting approach
            dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter)
        {

            IQueryable<T> query = dbSet;
            query=query.Where(filter);
            return query.FirstOrDefault();
            //Category? categoryFromDB2 = _db.Categories.Where(u => u.Id==id).FirstOrDefault(); //Used for complex queries
        }

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> query = dbSet;
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
