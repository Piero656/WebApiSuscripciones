using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace WebAPIAutores.Controllers
{
    public class CustomBaseController : ControllerBase
    {

        protected string ObtenerUsuairoId()
        {
            var usuarioClaim = HttpContext.User.Claims.Where(x => x.Type == "id").FirstOrDefault();
            var usuarioId = usuarioClaim.Value;
            return usuarioId;
        }

    }
}
