using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController(LocalDbContext context) : ControllerBase
    {
        private readonly LocalDbContext _context = context;

        // GET: api/patients
        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Patients.ToListAsync();
            return Ok(patients);
        }

        // GET: api/patients/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                return NotFound(new { Message = $"Patient with ID {id} not found." });
            }

            return Ok(patient);
        }

        // POST: api/patients
        [HttpPost]
        public async Task<IActionResult> AddPatient([FromBody] Patient newPatient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatientById), new { id = newPatient.Id }, newPatient);
        }

        // PUT: api/patients/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] Patient updatedPatient)
        {
            if (id != updatedPatient.Id)
            {
                return BadRequest(new { Message = "Patient ID mismatch." });
            }

            var existingPatient = await _context.Patients.FindAsync(id);
            if (existingPatient == null)
            {
                return NotFound(new { Message = $"Patient with ID {id} not found." });
            }

            // Update properties
            existingPatient.FirstName = updatedPatient.FirstName;
            existingPatient.LastName = updatedPatient.LastName;
            existingPatient.DateOfBirth = updatedPatient.DateOfBirth;
            existingPatient.Gender = updatedPatient.Gender;
            existingPatient.Address = updatedPatient.Address;
            existingPatient.PhoneNumber = updatedPatient.PhoneNumber;

            _context.Entry(existingPatient).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/patients/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                return NotFound(new { Message = $"Patient with ID {id} not found." });
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
