using EngramaCoreStandar.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CiudadanoModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.Share.Objects.CiudadanoModule;
using SantiagoConectaIA.Share.PostModels.CiudadanoModule;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core
{
    public class CiudadanoDomain : ICiudadanoDomain
    {
        private static readonly Regex DigitsOnly = new(@"\D", RegexOptions.Compiled);

        private readonly ICiudadanoRepository _repository;
        private readonly IConfiguration _configuration;

        public CiudadanoDomain(ICiudadanoRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<Response<Ciudadano>> Registrar(PostSaveCiudadano postModel)
        {
            try
            {
                var alias = (postModel.vchAlias ?? string.Empty).Trim();
                var telefono = NormalizePhone(postModel.vchTelefono);
                var pin = postModel.vchPassword ?? string.Empty;

                var validationError = Validate(alias, telefono, pin, requireAlias: true);
                if (validationError != null)
                {
                    return Response<Ciudadano>.BadResult(validationError, new Ciudadano());
                }

                var result = await _repository.spSaveCiudadano(new spSaveCiudadano.Request
                {
                    vchAlias = alias,
                    vchTelefono = telefono,
                    vchPassword = pin
                });

                if (result == null || !result.bResult || result.iIdCiudadano <= 0)
                {
                    return Response<Ciudadano>.BadResult(result?.vchMessage ?? "No se pudo completar el registro.", new Ciudadano());
                }

                var ciudadano = Map(result.iIdCiudadano, result.vchAlias, result.vchTelefono);
                ciudadano.Token = GenerateJwtToken(ciudadano);
                return new Response<Ciudadano>
                {
                    Data = ciudadano,
                    IsSuccess = true,
                    Message = result.vchMessage
                };
            }
            catch (Exception ex)
            {
                return Response<Ciudadano>.BadResult(ex.Message, new Ciudadano());
            }
        }

        public async Task<Response<Ciudadano>> Login(PostLoginCiudadano postModel)
        {
            try
            {
                var telefono = NormalizePhone(postModel.vchTelefono);
                var pin = postModel.vchPassword ?? string.Empty;

                var validationError = Validate(string.Empty, telefono, pin, requireAlias: false);
                if (validationError != null)
                {
                    return Response<Ciudadano>.BadResult(validationError, new Ciudadano());
                }

                var result = await _repository.spGetCiudadanoAuth(new spGetCiudadanoAuth.Request
                {
                    vchTelefono = telefono,
                    vchPassword = pin
                });

                if (result == null || !result.bResult || result.iIdCiudadano <= 0)
                {
                    return Response<Ciudadano>.BadResult(result?.vchMessage ?? "Teléfono o PIN incorrectos.", new Ciudadano());
                }

                var ciudadano = Map(result.iIdCiudadano, result.vchAlias, result.vchTelefono);
                ciudadano.Token = GenerateJwtToken(ciudadano);
                return new Response<Ciudadano>
                {
                    Data = ciudadano,
                    IsSuccess = true,
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                return Response<Ciudadano>.BadResult(ex.Message, new Ciudadano());
            }
        }

        private static string NormalizePhone(string? raw)
        {
            return DigitsOnly.Replace(raw ?? string.Empty, string.Empty);
        }

        private static string? Validate(string alias, string telefono, string pin, bool requireAlias)
        {
            if (requireAlias && string.IsNullOrWhiteSpace(alias))
            {
                return "El nombre o alias es obligatorio.";
            }

            if (telefono.Length != 10 || telefono.Any(c => c < '0' || c > '9'))
            {
                return "El teléfono debe tener exactamente 10 dígitos.";
            }

            if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
            {
                return "El PIN debe tener al menos 4 caracteres.";
            }

            return null;
        }

        private static Ciudadano Map(int id, string alias, string telefono) => new()
        {
            iIdCiudadano = id,
            vchAlias = alias ?? string.Empty,
            vchTelefono = telefono ?? string.Empty,
            vchRol = "Ciudadano"
        };

        private string GenerateJwtToken(Ciudadano user)
        {
            var keyStr = _configuration["JwtConfig:Secret"];
            if (string.IsNullOrEmpty(keyStr))
            {
                throw new Exception("JWT Secret not found in configuration.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.iIdCiudadano.ToString()),
                new Claim(ClaimTypes.Name, user.vchAlias),
                new Claim("telefono", user.vchTelefono),
                new Claim(ClaimTypes.Role, "Ciudadano")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtConfig:Issuer"] ?? "SantiagoConectaIA",
                audience: _configuration["JwtConfig:Audience"] ?? "SantiagoConectaIA",
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
