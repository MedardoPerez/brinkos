using brinkos.Dominio.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace repositorios.service.Core
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : Entity
    {
        private readonly IQueryableUnitOfWork _unitOfWork;

        /// <summary>
        /// Create a new instance of repository
        /// </summary>
        /// <param name="unitOfWork">Associated Unit Of Work</param>
        public Repository(IQueryableUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
                throw new ArgumentNullException("unitOfWork");

            _unitOfWork = unitOfWork;
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _unitOfWork;
            }
        }

        public void Add(TEntity item)
        {
            if (item != null)
            {
                item.FechaTransaccion = DateTime.Now;
                item.DescripcionTransaccion = "Insert";
                item.RowVersion = new byte[0];
                GetSet().Add(item); // add new item in this set
            }
            else
            {
                //LoggerFactory.CreateLog().LogInfo("info_CannotAddNullEntity", typeof(TEntity).ToString());
            }
        }

        public void AddRange(IEnumerable<TEntity> items)
        {
            if (items != null)
            {
                // add new items to the set.
                GetDbSet().AddRange(items);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> GetAll()
        {
            return GetSet();
        }

        public IEnumerable<TEntity> GetFiltered(Dictionary<string, object> filters)
        {
            var set = GetSet();

            if (filters != null && filters.Any())
            {
                // Creates the filter expression.
                Expression<Func<TEntity, bool>> filterExpression = null;

                foreach (KeyValuePair<string, object> filter in filters)
                {
                    if (filterExpression == null)
                    {
                        filterExpression = BuildContainsFuncFor<TEntity>(filter.Key, filter.Value);
                    }
                    else
                    {
                        filterExpression = AndAlso(filterExpression, BuildContainsFuncFor<TEntity>(filter.Key, filter.Value));
                    }
                }

                //= BuildContainsFuncFor<TEntity>(fieldToFilter, valueToFilter);

                var filterCombinedExpression = filterExpression.Compile();

                return set.Where(filterCombinedExpression);
            }

            return set;
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter)
        {
            return GetSet().Where(filter);
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, List<string> includes)
        {
            IQueryable<TEntity> items = GetSet();

            if (includes != null && includes.Any())
            {
                // Adding Includes to filter.
                items = includes.Aggregate(items, (current, include) => current.Include(include));
            }

            return items.Where(filter);
        }

        public virtual TEntity GetSingle(Expression<Func<TEntity, bool>> filter)
        {
            return GetSet().FirstOrDefault(filter);
        }

        public virtual TEntity GetSingle(Expression<Func<TEntity, bool>> filter, List<string> includes)
        {
            IQueryable<TEntity> items = GetSet();

            if (includes != null && includes.Any())
            {
                // Adding Includes to filter.
                items = includes.Aggregate(items, (current, include) => current.Include(include));
            }

            return items.FirstOrDefault(filter);
        }

        public void Merge(TEntity persisted, TEntity current)
        {
            throw new NotImplementedException();
        }

        public void Remove(TEntity item)
        {
            if (item != null)
            {
                //attach item if not exist
                _unitOfWork.Attach(item);

                //set as "removed"
                GetSet().Remove(item);
            }
        }

        public void RemoveRange(IEnumerable<TEntity> items)
        {
            if (items != null)
            {
                //attach items if not exist
                _unitOfWork.Attach(items);

                //set items as "removed"
                GetDbSet().RemoveRange(items);
            }
        }

        #region MetodosPrivados

        private IDbSet<TEntity> GetSet()
        {
            return _unitOfWork.CreateSet<TEntity>();
        }

        private DbSet<TEntity> GetDbSet()
        {
            return _unitOfWork.CreateSet<TEntity>();
        }

        private static Expression<Func<T, bool>> BuildContainsFuncFor<T>(string propertyName, object propertyValue)
        {
            var parameterExp = Expression.Parameter(typeof(T), "type");
            var propertyExp = Expression.Property(parameterExp, propertyName);

            // Si el tipo de la propiedad no es string entonces la expresion resultante
            // buscara encontrar el valor exacto y no que contenga un valor
            if (propertyExp.Type != typeof(string))
            {
                return GetExpression<T>(propertyName, "Equal", propertyValue.ToString());
                //return GetExpression<T>(propertyName, "Equal", propertyValue.ToString()).Compile();
            }

            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            if (propertyValue == null)
            {
                propertyValue = string.Empty;
            }

            var someValue = Expression.Constant(propertyValue.ToString().ToUpper(), typeof(string));
            var containsMethodExp = Expression.Call(Expression.Call(propertyExp, "ToUpper", null), method, someValue);

            var expression = Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
            return expression;

            //return expression.Compile();
        }

        private static Expression<Func<T, bool>> GetExpression<T>(string propertyName, string operatorType, string propertyValue)
        {
            var isNegated = operatorType.StartsWith("!");
            if (isNegated)
                operatorType = operatorType.Substring(1);

            var parameter = Expression.Parameter(typeof(T), "type");
            var property = Expression.Property(parameter, propertyName);

            // Cast propertyValue to correct property type
            var td = TypeDescriptor.GetConverter(property.Type);

            var constantValue = Expression.Constant(td.ConvertFromString(propertyValue), property.Type);

            // Check if specified method is an Expression member
            var operatorMethod = typeof(Expression).GetMethod(operatorType, new[] { typeof(MemberExpression), typeof(ConstantExpression) });

            Expression expression;

            if (operatorMethod == null)
            {
                // Execute against type members
                var method = property.Type.GetMethod(operatorType, new[] { property.Type });
                expression = Expression.Call(property, method, constantValue);
            }
            else
            {
                // Execute the passed operator method (e.g. Expression.GreaterThan)
                expression = (Expression)operatorMethod.Invoke(null, new object[] { property, constantValue });
            }

            if (isNegated)
                expression = Expression.Not(expression);

            return Expression.Lambda<Func<T, bool>>(expression, parameter);
        }

        private static Expression<Func<T, bool>> AndAlso<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            // need to detect whether they use the same
            // parameter instance; if not, they need fixing
            ParameterExpression param = expr1.Parameters[0];
            if (ReferenceEquals(param, expr2.Parameters[0]))
            {
                // simple version
                return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, expr2.Body), param);
            }
            // otherwise, keep expr1 "as is" and invoke expr2
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, Expression.Invoke(expr2, param)), param);
        }

        #endregion MetodosPrivados

        public PagedCollection GetPagedAndFiltered(DynamicFilter filterDef)
        {
            if (filterDef == null) throw new ArgumentNullException("filterDef");

            IQueryable<TEntity> items = !string.IsNullOrWhiteSpace(filterDef.Filtro)
                                            ? GetSet().Where(filterDef.Filtro, filterDef.Valores)
                                            : GetSet();

            if (filterDef.Includes != null && filterDef.Includes.Any())
            {
                // Adding Includes to filter.
                items = filterDef.Includes.Aggregate(items, (current, include) => current.Include(include));
            }

            int totalItems = items.Count();

            if (filterDef.PageSize != 0)
            {
                // adding sort ceiteria.
                if (filterDef.SortFields != null && filterDef.SortFields.Any())
                {
                    string orderKey = filterDef.Ascending ? "ASC" : "DESC";

                    var order = String.Join(" " + orderKey + ", ", filterDef.SortFields.ToArray());

                    if (!order.EndsWith(orderKey))
                    {
                        order += " " + orderKey;
                    }

                    items = items.OrderBy(order);

                    items = items.Skip(filterDef.PageSize * filterDef.PageIndex);
                }
                items = items.Take(filterDef.PageSize);
            }

            var pagedItems = items.ToList();

            return new PagedCollection(filterDef.PageIndex, filterDef.PageSize, pagedItems, totalItems,
                                       pagedItems.Count);
        }
    }
}