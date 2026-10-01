using EngramaCoreStandar.Mapper;
using EngramaCoreStandar.Results;

using SantiagoConectaIA.API.EngramaLevels.Domain.Interfaces.AuthModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Entity.AuthModule;
using SantiagoConectaIA.API.EngramaLevels.Infrastructure.Interfaces.AuthModule;
using SantiagoConectaIA.API.Services;
using SantiagoConectaIA.Share.Objects.AuthModulo;
using SantiagoConectaIA.Share.PostModels.AuthModulo;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SantiagoConectaIA.API.EngramaLevels.Domain.Core.AuthModule
{
    public class AuthDomain : IAuthDomain
    {
        private readonly IAuthRepository authRepository;
        private readonly MapperHelper mapperHelper;
        private readonly IResponseHelper responseHelper;
        private readonly IParametrosService parametros;

        public AuthDomain(IAuthRepository authRepository, MapperHelper mapperHelper, IResponseHelper responseHelper, IParametrosService parametros)
        {
            this.authRepository = authRepository;
            this.mapperHelper = mapperHelper;
            this.responseHelper = responseHelper;
            this.parametros = parametros;
        }

        public async Task<Response<UsuarioAuth>> Login(PostLoginUsuario daoModel)
        {
            try
            {
                var model = mapperHelper.Get<PostLoginUsuario, spGetUsuarioAuth.Request>(daoModel);
                var result = await authRepository.spGetUsuarioAuth(model);
                var validation = responseHelper.Validacion<spGetUsuarioAuth.Result, UsuarioAuth>(result);

                if (validation.IsSuccess)
                {
                    // Generar JWT
                    validation.Data.Token = await GenerateJwtToken(validation.Data);
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<UsuarioAuth>.BadResult(ex.Message, new UsuarioAuth());
            }
        }

        public async Task<Response<UsuarioAuth>> SaveUsuario(PostSaveUsuario daoModel)
        {
            try
            {
                var model = mapperHelper.Get<PostSaveUsuario, spSaveUsuario.Request>(daoModel);
                var result = await authRepository.spSaveUsuario(model);
                
                // Mapeamos el resultado de Save a una entidad vacia o a lo que devuelva
                var validation = responseHelper.Validacion<spSaveUsuario.Result, UsuarioAuth>(result);

                if (validation.IsSuccess)
                {
                    validation.Data.iIdUsuario = result.iIdUsuario;
                    validation.Data.vchNombre = daoModel.vchNombre;
                    validation.Data.vchEmail = daoModel.vchEmail;
                    // No generamos token de inmediato, se pide que hagan login
                }

                return validation;
            }
            catch (Exception ex)
            {
                return Response<UsuarioAuth>.BadResult(ex.Message, new UsuarioAuth());
            }
        }

        private async Task<string> GenerateJwtToken(UsuarioAuth user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.iIdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.vchNombre),
                new Claim(ClaimTypes.Email, user.vchEmail),
                new Claim(ClaimTypes.Role, user.vchRol ?? "User")
            };

            return JwtTokenFactory.Crear(await parametros.GetJwtAsync(), claims);
        }
    }
}
