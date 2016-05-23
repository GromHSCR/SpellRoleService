using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SpellRoleService.Models;

namespace SpellRoleService.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;

		public AccountController()
		{
		}

		public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

		//
		// GET: /Account/Login
		[AllowAnonymous]
		public ActionResult Login(string returnUrl)
		{
			this.ViewBag.ReturnUrl = returnUrl;
			return this.View();
		}

		//
		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
		{
			if (!this.ModelState.IsValid)
			{
				return this.View(model);
			}

			// Сбои при входе не приводят к блокированию учетной записи
			// Чтобы ошибки при вводе пароля инициировали блокирование учетной записи, замените на shouldLockout: true
			var result = await this.SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
			switch (result)
			{
				case SignInStatus.Success:
					return this.RedirectToLocal(returnUrl);
				case SignInStatus.LockedOut:
					return this.View("Lockout");
				case SignInStatus.RequiresVerification:
					return this.RedirectToAction("VerifyCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
				case SignInStatus.Failure:
					return this.RedirectToAction("Login", "Account");
				default:
					this.ModelState.AddModelError("", "Неудачная попытка входа.");
					return this.View(model);
			}
		}

		//
		// GET: /Account/VerifyCode
		[AllowAnonymous]
		public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
		{
			// Требовать предварительный вход пользователя с помощью имени пользователя и пароля или внешнего имени входа
			if (!await this.SignInManager.HasBeenVerifiedAsync())
			{
				return this.View("Error");
			}
			return this.View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
		}

		//
		// POST: /Account/VerifyCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
		{
			if (!this.ModelState.IsValid)
			{
				return this.View(model);
			}

			// Приведенный ниже код защищает от атак методом подбора, направленных на двухфакторные коды. 
			// Если пользователь введет неправильные коды за указанное время, его учетная запись 
			// будет заблокирована на заданный период. 
			// Параметры блокирования учетных записей можно настроить в IdentityConfig
			var result = await this.SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
			switch (result)
			{
				case SignInStatus.Success:
					return this.RedirectToLocal(model.ReturnUrl);
				case SignInStatus.LockedOut:
					return this.View("Lockout");
				case SignInStatus.Failure:
				default:
					this.ModelState.AddModelError("", "Неправильный код.");
					return this.View(model);
			}
		}

		//
		// GET: /Account/Register
		[AllowAnonymous]
		public ActionResult Register()
		{
			return this.View();
		}

		//
		// POST: /Account/Register
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Register(RegisterViewModel model)
		{
			if (this.ModelState.IsValid)
			{
				var user = new ApplicationUser { UserName = string.IsNullOrWhiteSpace(model.UserName) ? model.Email : model.UserName, Email = model.Email, PhoneNumber = model.Phone };
				var result = await this.UserManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					// Дополнительные сведения о том, как включить подтверждение учетной записи и сброс пароля, см. по адресу: http://go.microsoft.com/fwlink/?LinkID=320771
					// Отправка сообщения электронной почты с этой ссылкой
					string code = await this.UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
					var callbackUrl = this.Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: this.Request.Url.Scheme);
					await this.UserManager.SendEmailAsync(user.Id, "Подтверждение учетной записи", "Ваш код подтверждения: " + code + "<br/>Подтвердите вашу учетную запись, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");

					return this.RedirectToAction("Index", "Home");
				}
				this.AddErrors(result);
			}

			// Появление этого сообщения означает наличие ошибки; повторное отображение формы
			return this.View(model);
		}

		//
		// GET: /Account/ConfirmEmail
		[AllowAnonymous]
		public async Task<ActionResult> ConfirmEmail(string userId, string code)
		{
			if (userId == null || code == null)
			{
				return this.View("Error");
			}
			var result = await this.UserManager.ConfirmEmailAsync(userId, code);
			var user = this.UserManager.Users.SingleOrDefault(p => p.Id == userId);
			await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

			return this.View(result.Succeeded ? "ConfirmEmail" : "Error");
		}

		//
		// GET: /Account/ForgotPassword
		[AllowAnonymous]
		public ActionResult ForgotPassword()
		{
			return this.View();
		}

		//
		// POST: /Account/ForgotPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (this.ModelState.IsValid)
			{
				var user = await this.UserManager.FindByNameAsync(model.Email);
				if (user == null || !(await this.UserManager.IsEmailConfirmedAsync(user.Id)))
				{
					// Не показывать, что пользователь не существует или не подтвержден
					return this.View("ForgotPasswordConfirmation");
				}

				// Дополнительные сведения о том, как включить подтверждение учетной записи и сброс пароля, см. по адресу: http://go.microsoft.com/fwlink/?LinkID=320771
				// Отправка сообщения электронной почты с этой ссылкой
				string code = await this.UserManager.GeneratePasswordResetTokenAsync(user.Id);
				var callbackUrl = this.Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: this.Request.Url.Scheme);
				await this.UserManager.SendEmailAsync(user.Id, "Сброс пароля", "Сбросьте ваш пароль, щелкнув <a href=\"" + callbackUrl + "\">здесь</a>");
				return this.RedirectToAction("ForgotPasswordConfirmation", "Account");
			}

			// Появление этого сообщения означает наличие ошибки; повторное отображение формы
			return this.View(model);
		}

		//
		// GET: /Account/ForgotPasswordConfirmation
		[AllowAnonymous]
		public ActionResult ForgotPasswordConfirmation()
		{
			return this.View();
		}

		//
		// GET: /Account/ResetPassword
		[AllowAnonymous]
		public ActionResult ResetPassword(string code)
		{
			return code == null ? this.View("Error") : this.View();
		}

		//
		// POST: /Account/ResetPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!this.ModelState.IsValid)
			{
				return this.View(model);
			}
			var user = await this.UserManager.FindByNameAsync(model.Email);
			if (user == null)
			{
				// Не показывать, что пользователь не существует
				return this.RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			var result = await this.UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
			if (result.Succeeded)
			{
				return this.RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			this.AddErrors(result);
			return this.View();
		}

		//
		// GET: /Account/ResetPasswordConfirmation
		[AllowAnonymous]
		public ActionResult ResetPasswordConfirmation()
		{
			return this.View();
		}

		//
		// POST: /Account/ExternalLogin
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult ExternalLogin(string provider, string returnUrl)
		{
			// Запрос перенаправления к внешнему поставщику входа
			return new ChallengeResult(provider, this.Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
		}

		//
		// GET: /Account/SendCode
		[AllowAnonymous]
		public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
		{
			var userId = await this.SignInManager.GetVerifiedUserIdAsync();
			if (userId == null)
			{
				return this.View("Error");
			}
			var userFactors = await this.UserManager.GetValidTwoFactorProvidersAsync(userId);
			var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
			return this.View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
		}

		//
		// POST: /Account/SendCode
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> SendCode(SendCodeViewModel model)
		{
			if (!this.ModelState.IsValid)
			{
				return this.View();
			}

			// Создание и отправка маркера
			if (!await this.SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
			{
				return this.View("Error");
			}
			return this.RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
		}

		//
		// GET: /Account/ExternalLoginCallback
		[AllowAnonymous]
		public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
		{
			var loginInfo = await this.AuthenticationManager.GetExternalLoginInfoAsync();
			if (loginInfo == null)
			{
				return this.RedirectToAction("Login");
			}

			// Выполнение входа пользователя посредством данного внешнего поставщика входа, если у пользователя уже есть имя входа
			var result = await this.SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
			switch (result)
			{
				case SignInStatus.Success:
					return this.RedirectToLocal(returnUrl);
				case SignInStatus.LockedOut:
					return this.View("Lockout");
				case SignInStatus.RequiresVerification:
					return this.RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
				case SignInStatus.Failure:
				default:
					// Если у пользователя нет учетной записи, то ему предлагается создать ее
					this.ViewBag.ReturnUrl = returnUrl;
					this.ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
					return this.View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
			}
		}

		//
		// POST: /Account/ExternalLoginConfirmation
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
		{
			if (this.User.Identity.IsAuthenticated)
			{
				return this.RedirectToAction("Index", "Manage");
			}

			if (this.ModelState.IsValid)
			{
				// Получение сведений о пользователе от внешнего поставщика входа
				var info = await this.AuthenticationManager.GetExternalLoginInfoAsync();
				if (info == null)
				{
					return this.View("ExternalLoginFailure");
				}
				var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
				var result = await this.UserManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await this.UserManager.AddLoginAsync(user.Id, info.Login);
					if (result.Succeeded)
					{
						await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
						return this.RedirectToLocal(returnUrl);
					}
				}
				this.AddErrors(result);
			}

			this.ViewBag.ReturnUrl = returnUrl;
			return this.View(model);
		}

		//
		// POST: /Account/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			return this.RedirectToAction("Index", "Home");
		}

		//
		// GET: /Account/ExternalLoginFailure
		[AllowAnonymous]
		public ActionResult ExternalLoginFailure()
		{
			return this.View();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this._userManager != null)
				{
					this._userManager.Dispose();
					this._userManager = null;
				}

				if (this._signInManager != null)
				{
					this._signInManager.Dispose();
					this._signInManager = null;
				}
			}

			base.Dispose(disposing);
		}

		#region Вспомогательные приложения
		// Используется для защиты от XSRF-атак при добавлении внешних имен входа
		private const string XsrfKey = "XsrfId";

		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return this.HttpContext.GetOwinContext().Authentication;
			}
		}

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				this.ModelState.AddModelError("", error);
			}
		}

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (this.Url.IsLocalUrl(returnUrl))
			{
				return this.Redirect(returnUrl);
			}
			return this.RedirectToAction("Index", "Home");
		}

		internal class ChallengeResult : HttpUnauthorizedResult
		{
			public ChallengeResult(string provider, string redirectUri)
				: this(provider, redirectUri, null)
			{
			}

			public ChallengeResult(string provider, string redirectUri, string userId)
			{
				this.LoginProvider = provider;
				this.RedirectUri = redirectUri;
				this.UserId = userId;
			}

			public string LoginProvider { get; set; }
			public string RedirectUri { get; set; }
			public string UserId { get; set; }

			public override void ExecuteResult(ControllerContext context)
			{
				var properties = new AuthenticationProperties { RedirectUri = this.RedirectUri };
				if (this.UserId != null)
				{
					properties.Dictionary[XsrfKey] = this.UserId;
				}
				context.HttpContext.GetOwinContext().Authentication.Challenge(properties, this.LoginProvider);
			}
		}
		#endregion
	}
}