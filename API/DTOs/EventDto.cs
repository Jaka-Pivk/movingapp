public class EventAttendeesDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; }
    public List<AttendeeDto> Attendees { get; set; }
}
