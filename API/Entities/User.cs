using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class User : IdentityUser<int>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double PreviousLatitude { get; set; } // Add this property
        public double PreviousLongitude { get; set; } // Add this property
        public DateTime LastLocationUpdate { get; set; } // Add this property
        public double Speed { get; set; } // Add this property
        public ICollection<UserEvent> UserEvents { get; set; }
    }
}