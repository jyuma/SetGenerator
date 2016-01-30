using NHibernate;

namespace SetGenerator.Data
{
    public static class NHibernateExtensions
    {
        public static IQueryOver<T, T> Get<T>(this ISession session) where T : class
        {
            return session.QueryOver<T>();
        }
    }
}
