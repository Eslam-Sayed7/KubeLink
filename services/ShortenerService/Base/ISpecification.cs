using System.Linq.Expressions;

namespace ShortenerService.Base;
public interface ISpecification<T> 
{
    Expression<Func<T , bool>> Criteria { get; }
    
    List<Func<IQueryable<T>, IQueryable<T>>> Includes { get; }
    
    Func<IQueryable<T>, IOrderedQueryable<T>> OrderBy { get; }

    int? Skip { get; }
    int? Take { get; }
}