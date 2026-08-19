using EngramaCoreStandar.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.CiudadanoModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.Objects.CiudadanoModule;
using SantiagoConectaIA.Share.PostModels.CiudadanoModule;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly IWhatsAppService _whatsAppService;

        public CiudadanoDomain(ICiudadanoRepository repository, IConfiguration configuration, IWhatsAppService whatsAppService)
        {
            _repository = repository;
            _configuration = configuration;
            _whatsAppService = whatsAppService;
        }

        public async Task<Response<Ciudadano>> Registrar(PostSaveCiudadano postModel)
        {
            try
            {
                var alias = (postModel.vchAlias ?? string.Empty).Trim();
                var telefono = NormalizePhone(postModel.vchTelefono);
                var pin = postModel.vchPassword ?? string.Empty;
                var codigo = NormalizeCode(postModel.vchCodigo);

                var validationError = Validate(alias, telefono, pin, requireAlias: true);
                if (validationError != null)
                {
                    return Response<Ciudadano>.BadResult(validationError, new Ciudadano());
                }

                if (codigo.Length != 6 || codigo.Any(c => c < '0' || c > '9'))
                {
                    return Response<Ciudadano>.BadResult("Debes capturar el código de 6 dígitos.", new Ciudadano());
                }

                var otpValidation = await _repository.spValidarCiudadanoCodigo(new spValidarCiudadanoCodigo.Request
                {
                    vchTelefono = telefono,
                    vchCodigo = codigo
                });
                if (otpValidation == null || !otpValidation.bResult)
                {
                    return Response<Ciudadano>.BadResult(otpValidation?.vchMessage ?? "Código inválido o vencido.", new Ciudadano());
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

        public async Task<Response<string>> EnviarCodigoWhatsApp(PostSendCodigoCiudadano postModel)
        {
            try
            {
                var alias = (postModel.vchAlias ?? string.Empty).Trim();
                var telefono = NormalizePhone(postModel.vchTelefono);
                var pin = postModel.vchPassword ?? string.Empty;

                var validationError = Validate(alias, telefono, pin, requireAlias: true);
                if (validationError != null)
                {
                    return Response<string>.BadResult(validationError, string.Empty);
                }

                var code = GenerateVerificationCode();
                var saveCodeResult = await _repository.spSaveCiudadanoCodigo(new spSaveCiudadanoCodigo.Request
                {
                    vchTelefono = telefono,
                    vchCodigo = code
                });
                if (saveCodeResult == null || !saveCodeResult.bResult)
                {
                    return Response<string>.BadResult(saveCodeResult?.vchMessage ?? "No se pudo generar el código.", string.Empty);
                }

                var message = $"Hola, tu codigo de acceso expres para Santiago Conecta es: {code}";
                var destinationPhone = $"52{telefono}";
                var sendResult = await _whatsAppService.SendTextMessageDetailedAsync(destinationPhone, message);

                if (!sendResult.IsSuccess)
                {
                    var sendError = string.IsNullOrWhiteSpace(sendResult.ErrorMessage)
                        ? "No se pudo enviar el código por WhatsApp."
                        : sendResult.ErrorMessage;

                    if (IsDevelopment())
                    {
                        return new Response<string>
                        {
                            IsSuccess = true,
                            Data = destinationPhone,
                            Message = $"{sendError} Codigo de prueba: {code}"
                        };
                    }

                    return Response<string>.BadResult(sendError, string.Empty);
                }

                var deliveryHint = string.IsNullOrWhiteSpace(sendResult.MessageId)
                    ? "Meta aceptó la solicitud, pero no regresó message_id."
                    : $"Meta message_id: {sendResult.MessageId}";
                var statusHint = sendResult.StatusCode > 0 ? $" HTTP: {sendResult.StatusCode}." : string.Empty;
                var waIdHint = string.IsNullOrWhiteSpace(sendResult.WaId) ? string.Empty : $" wa_id: {sendResult.WaId}.";

                return new Response<string>
                {
                    IsSuccess = true,
                    Data = destinationPhone,
                    Message = IsDevelopment()
                        ? $"Código enviado por WhatsApp. {deliveryHint}{statusHint}{waIdHint} Codigo debug: {code}"
                        : $"Código enviado por WhatsApp. {deliveryHint}{statusHint}{waIdHint}"
                };
            }
            catch (Exception ex)
            {
                return Response<string>.BadResult(ex.Message, string.Empty);
            }
        }

        private static string NormalizePhone(string? raw)
        {
            return DigitsOnly.Replace(raw ?? string.Empty, string.Empty);
        }

        private static string NormalizeCode(string? raw)
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

            if (telefono[0] < '2' || telefono[0] > '9')
            {
                return "El teléfono debe iniciar con un dígito entre 2 y 9.";
            }

            if (telefono.All(c => c == telefono[0]))
            {
                return "El teléfono no puede tener todos los dígitos iguales.";
            }

            if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
            {
                return "El PIN debe tener al menos 4 caracteres.";
            }

            return null;
        }

        private static string GenerateVerificationCode()
        {
            return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        }

        private bool IsDevelopment()
        {
            var env = _configuration["ASPNETCORE_ENVIRONMENT"]
                ?? _configuration["DOTNET_ENVIRONMENT"]
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? string.Empty;
            return env.Equals("Development", StringComparison.OrdinalIgnoreCase);
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
