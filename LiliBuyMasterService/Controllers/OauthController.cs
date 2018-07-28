using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LiliBuyMasterService.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LiliBuyMasterService.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class OauthController : Controller
    {

        public IConfiguration Configuration { get; }

        public OauthController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpPost("authenticate")]
        public IActionResult RequestToken([FromBody]TokenRequest request)
        {
            if (request != null)
            {
                //验证账号密码,这里只是为了demo，正式场景应该是与DB之类的数据源比对
                if ("smilesb101".Equals(request.UserName) && "123456".Equals(request.Password))
                {
                    var claims = new[] {
                        //加入用户的名称
                        new Claim(ClaimTypes.Name,request.UserName)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:SecurityKey"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var authTime = DateTime.UtcNow;
                    var expiresAt = authTime.AddDays(7);

                    var token = new JwtSecurityToken(
                        issuer: "lilibuy.com",
                        audience: "lilibuy.com",
                        claims: claims,
                        expires: expiresAt,
                        signingCredentials: creds);

                    return Ok(new
                    {
                        access_token = new JwtSecurityTokenHandler().WriteToken(token),
                        token_type = "Bearer",
                        profile = new
                        {
                            name = request.UserName,
                            auth_time = new DateTimeOffset(authTime).ToUnixTimeSeconds(),
                            expires_at = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
                        }
                    });
                }
            }

            return BadRequest("Could not verify username and password.Pls check your information.");
        }
    }
}
