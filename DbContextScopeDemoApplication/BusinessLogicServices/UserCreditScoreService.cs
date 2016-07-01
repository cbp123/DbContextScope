using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.DbContextScope.Interfaces;
using Numero3.EntityFramework.Demo.DatabaseContext;
using Numero3.EntityFramework.Demo.DomainModel;

namespace Numero3.EntityFramework.Demo.BusinessLogicServices
{
	public class UserCreditScoreService
	{
		private readonly IDbContextScopeFactory _dbContextScopeFactory;

		public UserCreditScoreService(IDbContextScopeFactory dbContextScopeFactory)
		{
			if (dbContextScopeFactory == null) throw new ArgumentNullException(nameof(dbContextScopeFactory));
			_dbContextScopeFactory = dbContextScopeFactory;
		}

		public void UpdateCreditScoreForAllUsers()
		{
			/*
			 * Demo of DbContextScope + parallel programming.
			 */

			using (IDbContextScope dbContextScope = _dbContextScopeFactory.Create())
			{
				//-- Get all users
				UserManagementDbContext dbContext = dbContextScope.DbContexts.Get<UserManagementDbContext>();
				List<Guid> userIds = dbContext.Users.Select(u => u.Id).ToList();

				Console.WriteLine("Found {0} users in the database. Will calculate and store their credit scores in parallel.", userIds.Count);

				//-- Calculate and store the credit score of each user
				// We're going to imagine that calculating a credit score of a user takes some time. 
				// So we'll do it in parallel.

				// You MUST call SuppressAmbientContext() when kicking off a parallel execution flow 
				// within a DbContextScope. Otherwise, this DbContextScope will remain the ambient scope
				// in the parallel flows of execution, potentially leading to multiple threads
				// accessing the same DbContext instance.
				using (_dbContextScopeFactory.SuppressAmbientContext())
				{
					Parallel.ForEach(userIds, UpdateCreditScore);
				}

				// Note: SaveChanges() isn't going to do anything in this instance since all the changes
				// were actually made and saved in separate DbContextScopes created in separate threads.
				dbContextScope.SaveChanges();
			}
		}

		public void UpdateCreditScore(Guid userId)
		{
			using (IDbContextScope dbContextScope = _dbContextScopeFactory.Create())
			{
				UserManagementDbContext dbContext = dbContextScope.DbContexts.Get<UserManagementDbContext>();
#if EF6
				User user = dbContext.Users.Find(userId);
#elif EFCore
                var user = dbContext.Users.SingleOrDefault(x => x.Id == userId);
#endif
                if (user == null)
					throw new ArgumentException($"Invalid userId provided: {userId}. Couldn't find a User with this ID.");

				// Simulate the calculation of a credit score taking some time
				Random random = new Random(Thread.CurrentThread.ManagedThreadId);
				Thread.Sleep(random.Next(300, 1000));

				user.CreditScore = random.Next(1, 100);
				dbContextScope.SaveChanges();
			}
		}
	}
}
