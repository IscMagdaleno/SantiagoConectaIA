using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SantiagoConectaIA.API.Services
{
    public static class JwtTokenFactory
    {
        public static SymmetricSecurityKey Llave(JwtParametros jwt)
        {
            if (string.IsNullOrEmpty(jwt.Secret))
            {
                throw new InvalidOperationException($"No está configurado el secreto JWT (parámetro {ParametrosAlias.JwtSecret}).");
            }

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));
        }

        public static string Crear(JwtParametros jwt, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: new SigningCredentials(Llave(jwt), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
