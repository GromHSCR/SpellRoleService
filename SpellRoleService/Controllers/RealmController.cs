using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity.Owin;
using SpellRoleService.Models;

namespace SpellRoleService.Controllers
{
	[Authorize]
	public class RealmController : Controller
	{
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;
		private RoleServiceContext _roleServiceContext;

		public RealmController(RoleServiceContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			this._roleServiceContext = context;
		}

		public RealmController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
		{
			this.UserManager = userManager;
			this.SignInManager = signInManager;
		}

		public ApplicationSignInManager SignInManager
		{
			get
			{
				return this._signInManager ?? this.HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set
			{
				this._signInManager = value;
			}
		}

		public ApplicationUserManager UserManager
		{
			get
			{
				return this._userManager ?? this.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				this._userManager = value;
			}
		}

		public ActionResult Realms()
		{
			return this.View();
		}

		public ActionResult AddRealm()
		{
			return this.View();
		}

		public async Task<Realm> AddRealm(Realm model)
		{
			return this._roleServiceContext.Realms.Add(model);
		}
	}
}