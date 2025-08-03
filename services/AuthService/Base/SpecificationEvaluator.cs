using System.Text.Json.Serialization;
using AuthService.Base;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Base;

public class SpecificationEvaluator<TEntity> where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> QueryInput, ISpecification<TEntity> spec)
    {
        var query = QueryInput;

        // Apply filtering based on the Criteria
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // Apply each include (each function in the Includes list)
        query = spec.Includes.Aggregate(query, (current, include) => include(current));

        // Apply ordering if any
        if (spec.OrderBy != null)
        {
            query = spec.OrderBy(query);
        }

        // Apply pagination
        if (spec.Skip.HasValue)
        {
            query = query.Skip(spec.Skip.Value);
        }
        if (spec.Take.HasValue)
        {
            query = query.Take(spec.Take.Value);
        }

        return query;
    }
}
