using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Middlewares
{
    public static class LimitarPeticionesMiddlewareExtensions 
    {
        public static IApplicationBuilder UseLimitarPeticiones(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LimitarPeticionesMiddleware>();
        }
    }

    public class LimitarPeticionesMiddleware
    {
        private readonly RequestDelegate siguiente;
        private readonly IConfiguration configuration;

        public LimitarPeticionesMiddleware(RequestDelegate siguiente, IConfiguration configuration)
        {
            this.siguiente = siguiente;
            this.configuration = configuration;
        }


        public async Task InvokeAsync(HttpContext httpcontext, ApplicationDbContext context)
        {
            var limitarPeticionesConfiguracion = new LimitarPeticionesConfiguracion();
            configuration.GetRequiredSection("limitarPeticiones").Bind(limitarPeticionesConfiguracion);

            var ruta = httpcontext.Request.Path.ToString();
            var estaLaRutaEnListaBlanca = limitarPeticionesConfiguracion.ListaBlancaRutas.Any(x => ruta.Contains(x));

            if (estaLaRutaEnListaBlanca)
            {
                await siguiente(httpcontext);
                return;
            }

            var llaveStringValues = httpcontext.Request.Headers["X-Api-Key"];

            if (llaveStringValues.Count == 0)

            {
                httpcontext.Response.StatusCode = 400;
                await httpcontext.Response.WriteAsync("Debe proveer la llave en al cabecera X-Api-Key");
                return;
            }

            if (llaveStringValues.Count > 1)
            {
                httpcontext.Response.StatusCode = 400;
                await httpcontext.Response.WriteAsync("Solo una llave debe de estar presente");
                return;
            }


            var llave = llaveStringValues[0];
            var llaveDB = await context.LlavesAPI
                .Include(x => x.RestriccionesDominio)
                .Include(x => x.RestriccionesIP)
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Llave == llave);

            if (llaveDB == null)
            {
                httpcontext.Response.StatusCode = 400;
                await httpcontext.Response.WriteAsync("La llave no Existe");
                return;
            }

            if (!llaveDB.Activa)
            {
                httpcontext.Response.StatusCode = 400;
                await httpcontext.Response.WriteAsync("La llave se encuentra inactiva");
                return;
            }

            if (llaveDB.TipoLlave == Entidades.TipoLlave.Gratuita)
            {
                var hoy = DateTime.Today;
                var mañana = hoy.AddDays(1);

                var cantidadPeticionesRealizadasHoy = await context.Peticiones.CountAsync(x =>
                x.LlaveId == llaveDB.Id && x.FechaPeticion >= hoy && x.FechaPeticion < mañana);

                if (cantidadPeticionesRealizadasHoy >= limitarPeticionesConfiguracion.PeticionesPorDiaGratuito)
                {
                    httpcontext.Response.StatusCode = 429;
                    await httpcontext.Response.WriteAsync("Ha excedido el número de peticiones por día");
                    return;
                }
            }
            else if (llaveDB.Usuario.MalaPaga)
            {
                httpcontext.Response.StatusCode = 400;
                await httpcontext.Response.WriteAsync("El usuario es un Mala Paga");
                return;

            }
            


            var superaRestricciones = PeticionSuperaAlgunaDeLasRestricciones(llaveDB, httpcontext);

            if (!superaRestricciones)
            {
                httpcontext.Response.StatusCode = 403;
                return;
            }

            var peticion = new Peticion() { LlaveId = llaveDB.Id, FechaPeticion = DateTime.Now, };
            context.Add(peticion);

            await context.SaveChangesAsync();


            await siguiente(httpcontext);

        }


        private bool PeticionSuperaAlgunaDeLasRestricciones(LlaveAPI llaveAPI, HttpContext httpContext)
        {
            var hayRestricciones = llaveAPI.RestriccionesDominio.Any() || llaveAPI.RestriccionesIP.Any();

            if (!hayRestricciones)
            {
                return true;
            }

            var peticionSuperaLasRestriccionesDeDominio = PeticionSuperaLasRestriccionesDeDominio(llaveAPI.RestriccionesDominio, httpContext);

            var peticionSuperaLasRestriccionesPorIP = PeticionSuperaLasRestriccionesPorIP(llaveAPI.RestriccionesIP, httpContext);

            return peticionSuperaLasRestriccionesDeDominio || peticionSuperaLasRestriccionesPorIP;

        }

        private bool PeticionSuperaLasRestriccionesDeDominio(List<RestriccionDominio> restricciones, HttpContext httpContext)
        {
            if (restricciones == null || restricciones.Count == 0)
            {
                return false;
            }

            var referer = httpContext.Request.Headers["Referer"].ToString();
            if (referer == string.Empty)
            {
                return false;
            }

            Uri myUri = new Uri(referer);
            string host = myUri.Host;

            var superaRestriccion = restricciones.Any(x => x.Dominio == host);
            return superaRestriccion;
        }

        private bool PeticionSuperaLasRestriccionesPorIP(List<RestriccionIP> restricciones, HttpContext httpContext)
        {
            if (restricciones == null || restricciones.Count == 0)
            {
                return false;
            }

            var IP = httpContext.Connection.RemoteIpAddress.ToString();

            if (IP == string.Empty)
            {
                return false;
            }

            var superaRestriccion = restricciones.Any(x => x.IP == IP);
            return superaRestriccion;

        }





    }
}
