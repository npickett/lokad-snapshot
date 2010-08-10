#region Copyright (c) Lokad 2009-2010
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using System.Linq;
using System.Web.Mvc;
using Lokad.Cloud.Snapshot.Cloud.Activators;
using Lokad.Cloud.Snapshot.Cloud.Commands;
using Lokad.Cloud.Snapshot.Cloud.Reports;
using Lokad.Cloud.Snapshot.Cloud.UI.Authorization;
using Lokad.Cloud.Snapshot.Framework;
using Lokad.Cloud.Storage;

namespace Lokad.Cloud.Snapshot.Cloud.UI.Controllers
{
	public class SnapshotsController : Controller
	{
		private static void Send<T>(T message)
		{
			GlobalSetup.Container.Resolve<CloudInfrastructureProviders>().QueueStorage.Put(TypeMapper.GetStorageName(typeof(T)), message);
		}

		private static CloudTable<T> Table<T>(string name)
		{
			return new CloudTable<T>(GlobalSetup.Container.Resolve<CloudInfrastructureProviders>().TableStorage, name);
		}

		[AuthorizeOrRedirect]
		public ActionResult Index()
		{
			var query = Table<CompleteSnapshotReport>(Names.CompleteSnapshotReportsTable)
				.Get().Select(entity => entity.Value)
				.OrderBy(s => s.AccountName)
				.ThenByDescending(s => s.Completed);

			return View(query);
		}

		[AuthorizeOrRedirect]
		public ActionResult Create()
		{
			return View();
		}

		[AuthorizeOrRedirect]
		[HttpPost]
		public ActionResult Create(FormCollection collection)
		{
			try
			{
				Send(new StartSnapshotCommand
					{
						AccountName = collection["AccountName"],
						SnapshotId = IdHelper.NewId(),
						Credentials = Accounts.BuildCredentials(collection["AccountName"], collection["AccountKey"]),
					});

				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}

		[AuthorizeOrRedirect]
		public ActionResult Delete(string account, string id)
		{
			var entity = Table<CompleteSnapshotReport>(Names.CompleteSnapshotReportsTable).Get(account, id);
			if(!entity.HasValue)
			{
				return RedirectToAction("Index");
			}

			return View(entity.Value.Value);
		}

		[AuthorizeOrRedirect]
		[HttpPost]
		public ActionResult Delete(string account, string id, FormCollection collection)
		{
			try
			{
				Send(new DeleteSnapshotCommand
					{
						AccountName = account,
						SnapshotId = id,
						Credentials = Accounts.BuildSnapshotOnlyCredentials()
					});

				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}

		[AuthorizeOrRedirect]
		public ActionResult Restore(string account, string id)
		{
			var entity = Table<CompleteSnapshotReport>(Names.CompleteSnapshotReportsTable).Get(account, id);
			if (!entity.HasValue)
			{
				return RedirectToAction("Index");
			}

			return View(entity.Value.Value);
		}

		[AuthorizeOrRedirect]
		[HttpPost]
		public ActionResult Restore(string account, string id, FormCollection collection)
		{
			try
			{
				Send(new StartRestoreCommand
					{
						AccountName = account,
						SnapshotId = id,
						RestoreId = IdHelper.NewId(),
						Credentials = Accounts.BuildCredentials(collection["RestoreAccountName"], collection["RestoreAccountKey"])
					});

				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}
	}
}
