using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Shop.Models;
using Microsoft.IdentityModel.Tokens;

namespace Shop.Services
{
	public static class TokenService
	{
		public static string GenerateToken(User user)
		{
			// gera o token				
			var tokenHandler = new JwtSecurityTokenHandler();
			// nossa chave em bytes
			var key = Encoding.ASCII.GetBytes(Settings.Secret);
			// descrição do que vai ter no token
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
					{
										new Claim(ClaimTypes.Name, user.Username.ToString()),
										new Claim(ClaimTypes.Role, user.Role.ToString())
					}),
				// expirar token
				Expires = DateTime.UtcNow.AddHours(2),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			// criar o token com as informação que passei
			var token = tokenHandler.CreateToken(tokenDescriptor);
			// gera a string do token
			return tokenHandler.WriteToken(token);
		}
	}
}