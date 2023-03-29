using Microsoft.AspNetCore.Authorization;

namespace RestaurantAPI.Authorization
{
    public class CreatedMultipleRestaurantsReqirement : IAuthorizationRequirement
    {
        public CreatedMultipleRestaurantsReqirement(int minimumRestaurantCreated)
        {
            MinimumRestaurantCreated = minimumRestaurantCreated;
        }
        public int MinimumRestaurantCreated { get; set; }
    }
}
