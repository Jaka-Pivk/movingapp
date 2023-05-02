using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly StoreContext _dbContext;

    public EventsController(UserManager<User> userManager, StoreContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpPost("createEvent")]
    public async Task<IActionResult> CreateEvent(CreateEventDto dto)
    {
        var newEvent = new Event
        {
            Name = dto.Title,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Description = dto.Description
        };

        _dbContext.Events.Add(newEvent);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Event created successfully" });
    }

    [HttpPost("joinEvent/{eventId}")]
    [Authorize]
    public async Task<IActionResult> AddUserToEvent(int eventId)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = userIdClaim.Value;
        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser == null)
        {
            return NotFound("User not found");
        }


        var eventEntity = await _dbContext.Events.FindAsync(eventId);
        if (eventEntity == null)
        {
            return NotFound("Event not found");
        }

        var userEvent = new UserEvent
        {
            UserId = currentUser.Id,
            EventId = eventId
        };

        _dbContext.UserEvents.Add(userEvent);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "User successfully added to the event" });
    }

    [HttpGet("allEvents")]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _dbContext.Events.ToListAsync();
        return Ok(events);
    }

    [HttpGet("eventAttendees/{eventId}")]
    public async Task<IActionResult> GetEventAttendees(int eventId)
    {
        var eventEntity = await _dbContext.Events
            .Include(e => e.UserEvents)
            .ThenInclude(ue => ue.User)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            return NotFound("Event not found");
        }

        var attendees = eventEntity.UserEvents.Select(ue => new AttendeeDto
        {
            Id = ue.User.Id,
            UserName = ue.User.UserName,
            Longitude = ue.User.Longitude,
            Latitude = ue.User.Latitude,
            Speed = ue.User.Speed // Use the moving average speed value
        }).ToList();

        var eventAttendeesDto = new EventAttendeesDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Latitude = eventEntity.Latitude,
            Longitude = eventEntity.Longitude,
            Description = eventEntity.Description,
            Attendees = attendees
        };

        return Ok(eventAttendeesDto);
    }

    [HttpPut("updateUserLocation")]
    [Authorize]
    public async Task<IActionResult> UpdateUserLocation(UpdateUserLocationDto dto)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = userIdClaim.Value;
        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser == null)
        {
            return NotFound("User not found");
        }

        // Save the previous location
        currentUser.PreviousLatitude = currentUser.Latitude;
        currentUser.PreviousLongitude = currentUser.Longitude;

        // Update the current location
        currentUser.Latitude = dto.Latitude;
        currentUser.Longitude = dto.Longitude;

        // Calculate the new speed
        var distance = GpsService.CalculateDistance(currentUser.PreviousLatitude, currentUser.PreviousLongitude, dto.Latitude, dto.Longitude);
        var timeElapsed = (DateTime.UtcNow - currentUser.LastLocationUpdate).TotalSeconds;
        var newSpeed = timeElapsed > 0 ? (distance / timeElapsed) * 3.6 : 0;

        // Update the moving average of the user's speed (assuming alpha = 0.1)
        var alpha = 0.1;
        currentUser.Speed = alpha * newSpeed + (1 - alpha) * currentUser.Speed;

        // Update the last location update timestamp
        currentUser.LastLocationUpdate = DateTime.UtcNow;

        // Update the user in the database
        var result = await _userManager.UpdateAsync(currentUser);

        if (result.Succeeded)
        {
            return Ok(new { message = "User location updated successfully" });
        }

        return BadRequest("Failed to update user location");
    }
}
