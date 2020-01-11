using AutoMapper;
using Identity_auth.Configurations;
using Identity_auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Identity_auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IdentityServerValuesConfiguration _identityServerValuesConfiguration;

        public AccountController(
            ILogger<AccountController> logger,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IMapper mapper,
            IdentityServerValuesConfiguration identityServerValuesConfiguration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _identityServerValuesConfiguration = identityServerValuesConfiguration;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("splash")]
        public async Task<IActionResult> Splash()
        {
            return Ok("Welcome to splash route!");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<RedirectResult> Login(
            [FromForm]LoginModel model,
            CancellationToken ct = default)
        {
            _logger.LogInformation($"User with email: {model.Email} try to login.");

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                HttpResponseMessage responseSuccess = new HttpResponseMessage(HttpStatusCode.Moved);
                responseSuccess.Headers.Location = new Uri($"https://localhost:44352{model.ReturnUrl}");

                RedirectResult redirectResult = new RedirectResult($"https://localhost:44352{model.ReturnUrl}", true);


                return redirectResult;
            }
            else if (result.IsLockedOut)
            {
                return new RedirectResult("");
            }

            HttpResponseMessage responseBadRequest = new HttpResponseMessage(HttpStatusCode.Moved);
            responseBadRequest.Headers.Location = new Uri($"{_identityServerValuesConfiguration.Root}/400");

            return new RedirectResult("");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<IActionResult> Register(
            [FromForm]RegisterModel model,
            CancellationToken ct = default)
        {
            _logger.LogInformation($"User with email: {model.Email} try to register.");

            IdentityUser existUser = await _userManager.FindByNameAsync(model.Email);

            // user was registerd by google:
            if (existUser != null)
            {
                return await UpdateUserTemporaryPassword(model, ct);
            }

            IdentityUser user = new IdentityUser(model.Email);
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);

                return Created("null", user);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("external-google-login")]
        public async Task<IActionResult> GoogleLogin(
            [FromForm]LoginModel model,
            CancellationToken ct = default)
        {
            _logger.LogInformation($"User with email: {model.Email} try to register with google auth.");

            IdentityUser existUser = await _userManager.FindByNameAsync(model.Email);

            if (existUser != null)
            {
                return Redirect(model.ReturnUrl);
            }

            string tempPassword = Guid.NewGuid().ToString();

            RegisterModel newGoogleRegisterUser = _mapper.Map<RegisterModel>(model);
            newGoogleRegisterUser.Password = tempPassword;
            newGoogleRegisterUser.ConfirmPassword = tempPassword;

            return await InternalRegister(newGoogleRegisterUser, ct);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("external-facebook-login")]
        public async Task<IActionResult> FacebookLogin(
            [FromForm]FacebookLoginModel model,
            CancellationToken ct = default)
        {
            if (model.Email == null)
            {
                _logger.LogInformation($"Redirect without internal registartion.");

                return Redirect(model.ReturnUrl);
            }

            IdentityUser existUser = await _userManager.FindByNameAsync(model.Email);

            if (existUser != null)
            {
                return Redirect(model.ReturnUrl);
            }

            _logger.LogInformation($"User with email: {model.Email} try to register with facebook auth.");

            string tempPassword = Guid.NewGuid().ToString();

            RegisterModel fbRegisterModel = _mapper.Map<RegisterModel>(model);
            fbRegisterModel.Password = tempPassword;
            fbRegisterModel.ConfirmPassword = tempPassword;

            return await InternalRegister(fbRegisterModel, ct);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation($"Logout endpoint was triggered.");

            await _signInManager.SignOutAsync();

            return Ok();
        }

        [HttpGet]
        [Route("data")]
        public async Task<IActionResult> GetDataAsync(CancellationToken ct = default)
        {
            _logger.LogInformation($"Getting test data.");

            LoginModel dataForTesting = new LoginModel()
            {
                ExternalProviders = new List<ExternalProvider>(),
                Password = "123123",
                ReturnUrl = "https://google.com",
                Email = "Test user"
            };

            return await Task.FromResult(Ok(dataForTesting));
        }

        private async Task<bool> InternalLogin(
            LoginModel model,
            CancellationToken ct = default)
        {
            _logger.LogInformation($"User: {model.Email} trigger google login.");

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            return result.Succeeded;
        }

        private async Task<IActionResult> InternalRegister(
            RegisterModel model,
            CancellationToken ct = default)
        {
            _logger.LogInformation($"Internal register for user with email: {model.Email}.");

            IdentityUser user = new IdentityUser(model.Email);
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);

                LoginModel newInteranlUser = _mapper.Map<LoginModel>(model);

                bool isSuccesssfullyLogined = await InternalLogin(newInteranlUser, ct);

                if (isSuccesssfullyLogined)
                {
                    return Redirect(model.ReturnUrl);
                }

                return BadRequest();
            }

            return BadRequest(result.Errors);
        }

        private async Task<IActionResult> UpdateUserTemporaryPassword(
            RegisterModel model,
            CancellationToken ct = default)
        {
            _logger.LogInformation($"Updating password for user {model.Email}.");

            IdentityUser userForUpdate = await _userManager.FindByNameAsync(model.Email);

            await _userManager.RemovePasswordAsync(userForUpdate);

            await _userManager.AddPasswordAsync(userForUpdate, model.Password);

            await _signInManager.SignInAsync(userForUpdate, false);

            return Created("null", userForUpdate);
        }
    }
}