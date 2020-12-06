using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PasswordManager.Core.Repositories {

	public interface IRepository<TEntity> where TEntity : class, new() {
		Task<IEnumerable<TEntity>> GetAsync(
			Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			string includeProperties = null,
			bool asNoTracking = true);

		Task<TEntity> GetByIdAsync(int id);

		IQueryable<TEntity> GetWithRawSql(string query,
			params object[] parameters);

		Task AddAsync(TEntity entity);

		///<summary>This method does not update navigation properties of <paramref name="entity"/>.
		///Override it for this purpose.</summary>
		/// <returns>False if record not found</returns>
		bool Update(TEntity entity);

		///<summary>This method does not delete navigation properties of <paramref name="entity"/>that are not have cascade deletion. 
		///Override it for this purpose.</summary>
		/// <returns>False if record not found</returns>
		bool Delete(TEntity entity);


		bool HasDataChanged { get; }
	}
}
