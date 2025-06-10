using System;
using System.Linq;
using System.Linq.Expressions;

namespace HG.CFDI.CORE.Utilities
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string primaryProperty, bool primaryDescending, string secondaryProperty, bool secondaryDescending)
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            // Primer campo
            var primaryPropertyAccess = Expression.Property(parameter, primaryProperty);
            var primarySelector = Expression.Lambda(primaryPropertyAccess, parameter);

            // Aplicar OrderBy u OrderByDescending
            var orderedQuery = primaryDescending
                ? Queryable.OrderByDescending(source, (dynamic)primarySelector)
                : Queryable.OrderBy(source, (dynamic)primarySelector);

            // Segundo campo
            var secondaryPropertyAccess = Expression.Property(parameter, secondaryProperty);
            var secondarySelector = Expression.Lambda(secondaryPropertyAccess, parameter);

            // Aplicar ThenBy u ThenByDescending
            return secondaryDescending
                ? Queryable.ThenByDescending((IOrderedQueryable<T>)orderedQuery, (dynamic)secondarySelector)
                : Queryable.ThenBy((IOrderedQueryable<T>)orderedQuery, (dynamic)secondarySelector);
        }
    }
}
