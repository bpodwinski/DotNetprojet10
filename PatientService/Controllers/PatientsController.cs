using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;

namespace PatientService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController(LocalDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            return Ok("test");
        }
    }
}
