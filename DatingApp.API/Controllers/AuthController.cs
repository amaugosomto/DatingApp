using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository _repo;
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;

    public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
    {
      _config = config;
      _mapper = mapper;
      _repo = repo;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
    {
      // Validate request
      userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
      if (await _repo.UserExists(userForRegisterDto.Username))
        return BadRequest("Username already exists");

      var userToCreate = new User
      {
        Username = userForRegisterDto.Username
      };

      var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);
      return StatusCode(201);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {
      // Send to the repo to make sure the user exists
      var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
      if (userFromRepo == null)
        return Unauthorized();

      // Creating a token claim, containing two claims(User id and username)
      var claims = new[]
      {
          new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
          new Claim(ClaimTypes.Name, userFromRepo.Username)
      };

      // Making sure that it is a valid token, the server needs to sign by creating a security key and getting the secret key
      // from the "appsettings.json file"
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

      // Using the key as part of the signing credentials and then encripting this key with a hashing algorithm
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      // Then create a token descriptor, passing claims as the subject, give it an expiry date and then pass in the signing credentials
      // created from above
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(1),
        SigningCredentials = creds
      };

      // Creating a new JWT security token handler
      var tokenHandler = new JwtSecurityTokenHandler();

      // The step above allows us create the token based on the token descriptor passed
      var token = tokenHandler.CreateToken(tokenDescriptor);

      var user = _mapper.Map<UserForListDto>(userFromRepo);

      // Using the token variable to write the token in the response sent back to the client
      return Ok( new {
        token = tokenHandler.WriteToken(token),
        user
      });
    }
  }
}