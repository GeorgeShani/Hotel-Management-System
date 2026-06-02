using BackEnd.DTOs.Manager;
using BackEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ManagersController : ControllerBase
    {
        private readonly IManagerService _managerService;

        public ManagersController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        // GET: api/managers
        [HttpGet]
        public async Task<IActionResult> GetAllManagers()
        {
            var managers = await _managerService.GetAllManagersAsync();
            return Ok(managers);
        }

        // GET: api/managers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetManager(int id)
        {
            var manager = await _managerService.GetManagerByIdAsync(id);
            if (manager == null) return NotFound("Manager not found.");

            return Ok(manager);
        }

        // POST: api/managers
        [HttpPost]
        public async Task<IActionResult> CreateManager([FromBody] CreateManagerDto createDto)
        {
            // Uniqueness (Email, PersonalNumber) and hotel existence are validated here
            var manager = await _managerService.CreateManagerAsync(createDto);
            return CreatedAtAction(nameof(GetManager), new { id = manager.Id }, manager);
        }

        // PUT: api/managers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateManager(int id, [FromBody] UpdateManagerDto updateDto)
        {
            await _managerService.UpdateManagerAsync(id, updateDto);
            return NoContent();
        }

        // DELETE: api/managers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManager(int id)
        {
            // Validates whether the hotel has another manager before deleting
            await _managerService.DeleteManagerAsync(id);
            return NoContent();
        }
    }
}
