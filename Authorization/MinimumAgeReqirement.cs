using Microsoft.AspNetCore.Authorization;

namespace RestaurantAPI.Authorization
{
    public class MinimumAgeReqirement : IAuthorizationRequirement
    {
        public int MinimumAge { get; }

        public MinimumAgeReqirement(int minimumAge)
        {
            MinimumAge = minimumAge;
        }
    }
}
