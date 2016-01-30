using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using SetGenerator.Domain.Entities;

namespace SetGenerator.Data.Repositories
{
    public interface IRepositoryBase<T>
    {
        ICollection<T> GetAll();
        int Add(T element);
        T Get(int id);
        void Update(T element);
        void Delete(int id);
        int Count();
    }

    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : EntityBase
    {
        protected readonly ISession Session;
        protected IQueryOver<T, T> QueryBase;

        protected RepositoryBase(ISession session)
        {
            Session = session;
            QueryBase = session.Get<T>();
        }

        public virtual ICollection<T> GetAll()
        {
            return QueryBase
                .TransformUsing(Transformers.DistinctRootEntity)
                .List();
        }

        public virtual int Add(T element)
        {
            int id;

            using (var transaction = Session.BeginTransaction())
            {
                id = (int)Session.Save(element);
                transaction.Commit();
            }

            return id;
        }

        public virtual T Get(int id)
        {
            return QueryBase.Where(x => x.Id == id).SingleOrDefault();
        }

        public virtual void Update(T element)
        {
            using (var transaction = Session.BeginTransaction())
            {
                Session.Update(element);
                transaction.Commit();
            }
        }

        public virtual void Delete(int id)
        {
            using (var transaction = Session.BeginTransaction())
            {
                var element = Session.Load<T>(id);
                Session.Delete(element);
                transaction.Commit();
            }
        }

        public int Count()
        {
            var totalcount = Session.QueryOver<T>().ToRowCountQuery().FutureValue<int>().Value;
            return totalcount;
        }
    }
}
