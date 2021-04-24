using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using flutterBackEnd.Models;
using flutterBackEnd.Providers;
using flutterBackEnd.Results;
using System.Linq;
using System.Data.Entity;
/*  important we must enable cors seperately for the Token endpoint as enabling all at startup is NOT good enough 
*  we must add the following line 
*  And inside GrantResourceOwnerCredentials method:    in applicationOauthProvider.cs

context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" }); 
*/
namespace flutterBackEnd.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                return null;
            }

            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

            foreach (IdentityUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                
                 ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }
        // POST api/Account/StatusOfDaysinMonth 
        //the user has set the status for days in the month representin if they are avaiable or not

        [Route("StatusOfDaysinMonth")]
        [HttpPost]
        public IHttpActionResult StatusOfDaysinMonth([FromUri]String month, [FromUri]string EMail, [FromBody]datesandStatusDTO values)
        //      public IHttpActionResult StatusOfDaysinMonth(datesandStatusDTO values)
        {

            ApplicationUser user = db.Users.Where(u => u.Email == EMail).SingleOrDefault();
            BookedDates booked = new BookedDates();
            booked.month = Int32.Parse(month); 
            booked.status = "";
            int count = values.status[0];
            for (int i = 0; i < count; i++)
            {
                booked.status = booked.status + values.status[i] + ",";

            }
            booked.user = user;
            db.statusforDays.Add(booked);
            db.SaveChanges();
            return Ok();
        }
        // GET api/Account/getMonthStatus
        [AllowAnonymous]
        [Route("GetMonthStatus")]
        public List<BookedDatesDTO > GetMonthStatus(string month)
        {


            int m = Int32.Parse(month);
            List<BookedDates> status = db.statusforDays.Where(i => i.month == m).ToList();
            List<BookedDatesDTO> dtos = new List<BookedDatesDTO>();
            for (int j = 0; j < status.Count(); j++)
            {
                BookedDatesDTO dto = new BookedDatesDTO
                {
                    id = j,
                    Name = status[j].user.UserName,
                    month = m,
                    level = status[j].user.skillLevel,
                    isCaptain = false,
                    numTimesCaptain = status[j].user.timesCaptain,
                    status = status[j].status


                };
                dtos.Add(dto);

            }
            return dtos;
        }
        // GET api/Account/getMonthStatus
        [AllowAnonymous]
        [Route("GetPlayersinMatch")]
        public String  GetPlayersinMatch(String Name,String day1)
        {
            int day = Int32.Parse(day1);
            List<Match> matchesforday = db.matchs.Where(m => m.day == day).ToList();
            //foreach(Match match in matchesforday)
            //{
            //    if (match.players.Contains(Name))
            //    {
            //        return match.players;
                    
            //    }
            //}
            
           return "not found";
        }
        [AllowAnonymous]
        [Route("GetAllMatchs")]
        public IEnumerable<MatchDTO> GetAllMatchs()
        {
            List<Match> matches = db.matchs.Include(m => m.players).ToList();
           
            List <MatchDTO> dtos = new List<MatchDTO>();
            //do not want to pass the whole user object   
            foreach( Match m in matches)
            {
                MatchDTO dto = new MatchDTO();
                dto.day = m.day;
                dto.month = m.month ;
                dto.level = m.skillLevel ;
                dto.Captain = m.captain ;
                dto.players = new List<String>();
                for (int i = 0; i < m.players.Count; i++)
                {
          
                    dto.players.Add(m.players[i].Email);
          //          dto.players = dto.players + ",";
                }
                dtos.Add(dto);
              
            };
            return  dtos;
            
        }
        [AllowAnonymous]
        [Route("freezedatabase")]
        [HttpPost]
        public IHttpActionResult freezedatabase(string state)
        {
            ApplicationUser user = db.Users.Where(u => u.Email == "jmcfet@icloud.com").SingleOrDefault();
            user.bFreezeDB = user.bFreezeDB == 1 ? 0 : 1 ;
          
            db.SaveChanges();
            return Ok();
        }
       
        [Route("isfreezedatabase")]
        [HttpGet]
        public IHttpActionResult isfreezedatabase()
        {
            ApplicationUser user = db.Users.Where(u => u.Email == "jmcfet@icloud.com").SingleOrDefault();
            if (user.bFreezeDB == 0)
                return Ok();
            
            return NotFound();
        }
        // GET api/Account/Login
        [AllowAnonymous]
        [Route("GetUsers")]
        public IEnumerable<userdto> GetUsers()
        {
            //if (!ModelState.IsValid)
            //{
            //    InvalidModelStateResult test = BadRequest(ModelState);
            //    return BadRequest(ModelState);
            //}
            userdto dto = null;
            List<userdto> info = new List<userdto>();

            try
            {
               

                List<ApplicationUser> users =  UserManager.Users.ToList();
                foreach(ApplicationUser user in users)
                {
                    dto = new userdto()
                    {
                        level = user.skillLevel,
                        timesCaptain = user.timesCaptain,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    };
                    info.Add(dto);
                }
                


            }
            catch (Exception e)
            {
                int mm = 0;
            }
            return info;



        }
        // GET api/Account/Login
       
        [Route("GetUser")]
        public userdto GetUser(string email)
        {
            //if (!ModelState.IsValid)
            //{
            //    InvalidModelStateResult test = BadRequest(ModelState);
            //    return BadRequest(ModelState);
            //}
            userdto dto = null;
           
            try
            {


                ApplicationUser user = db.Users.Where(u => u.UserName == email).SingleOrDefault();
              
                dto = new userdto()
                {
                    level = user.skillLevel,
                    timesCaptain = user.timesCaptain,
                    Name = user.UserName,
                    isFrozen = user.bFreezeDB,
                    Email = user.Email
                };
                   
            
            }
            catch (Exception e)
            {
                int mm = 0;
            }
            return dto;



        }
        [AllowAnonymous]
        [Route("zeroCaptainCounts")]
        public IHttpActionResult zeroCaptainCounts()
        {
            List<ApplicationUser> users = UserManager.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                user.timesCaptain = 0;
            }
            db.SaveChanges();
            return Ok();
        }
       

    [Route("GetUserInfoforMonth")]
        public List<MatchDTO> GetUserInfoforMonth(String email,int month)
        {
            //if (!ModelState.IsValid)
            //{
            //    InvalidModelStateResult test = BadRequest(ModelState);
            //    return BadRequest(ModelState);
            //}
            userdto dto = null;
            List<MatchDTO> dtos = new List<MatchDTO>();
            
            try
            {


                ApplicationUser user = db.Users.Include(m => m.Matches).Where(u => u.Email == email).SingleOrDefault();
          //      dto = new userdto()
           //     {
           //         level = user.skillLevel,
          //          timesCaptain = user.timesCaptain,
          //          Name = user.UserName,
          //          Email = user.Email
         //       };
             //   dto.matchs = new List<MatchDTO>();
                foreach (Match match in user.Matches)
                {
                    if (match.month != month)
                        continue;
                    MatchDTO matchdto = new MatchDTO();
                    matchdto.day = match.day;
                    matchdto.month = match.month;
                    matchdto.level = match.skillLevel;
                    matchdto.Captain = match.captain;
                    matchdto.players = new List<String>();
                    for (int i = 0; i < match.players.Count; i++)
                    {
          //              matchdto.players = matchdto.players + match.players[i].Email + ',';
                        matchdto.players.Add(match.players[i].Email);
                    }
                    dtos.Add(matchdto);
                }



            }
            catch (Exception e)
            {
                int mm = 0;
            }
            return dtos;



        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            ClubMember member = db.members.Where(m => m.Name == model.Email).SingleOrDefault();
            if (member == null)
                return NotFound();
            var user = new ApplicationUser() { UserName = model.UserID, Email = model.Email, memberName = model.Name  };
            
            IdentityResult result = await UserManager.CreateAsync(user, model.Password );

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }
        [AllowAnonymous]
        [Route("Matches")]
        public  IHttpActionResult Matches(List<MatchDTO> matches)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
           
            foreach(MatchDTO dto in matches)
            {
                Match m = new Match(
                     
                    );
                m.day = dto.day;
                m.month = dto.month;
                m.skillLevel = dto.level;
                m.captain = dto.Captain;
                m.players = new List<ApplicationUser>();
                if (m.captain != "not")
                {
                    ApplicationUser u = db.Users.Where(u1 => u1.UserName == m.captain).SingleOrDefault();
                    u.timesCaptain += 1;
                }

                for (int i = 0; i < dto.players.Count; i++)
                {
                    string name = dto.players[i];
                    ApplicationUser user = db.Users.Where(u1 => u1.UserName == name).SingleOrDefault();
                    m.players.Add(user);
                }
                   
                     
               db.matchs.Add(m);
            
            }
          
            db.SaveChanges();    
            return Ok();
        }

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result); 
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
    public class userdto
    {
        public string Email { get; set; }
        public int isFrozen { get; set; }
        public int level { get; set; }
        public String Name { get; set; }
        public int timesCaptain { get; set; }
        public String PhoneNumber { get; set; }

        public List<MatchDTO> matchs { get; set; }

    }
}
