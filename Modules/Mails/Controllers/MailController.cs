using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Joblify.Modules.Mails.DTOs;
using Joblify.Modules.Mails.Interfaces;

namespace Joblify.Modules.Mails.Controllers;

[ApiController]
[Route("api/mails")]
[Authorize]
public class MailController : ControllerBase
{
    private readonly IMailService _service;

    public MailController(IMailService service)
    {
        _service = service;
    }


}
