using Microsoft.AspNetCore.Authorization;
using RestaurantAPI.Entities;
using System.Security.Claims;

namespace RestaurantAPI.Authorization
{
    public class CreatedMultipleRestaurantsReqirementHandler : AuthorizationHandler<CreatedMultipleRestaurantsReqirement>
    {
        private readonly RestaurantDbContext _context;

        public CreatedMultipleRestaurantsReqirementHandler(RestaurantDbContext context)
        {
            _context = context;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CreatedMultipleRestaurantsReqirement requirement)
        {
            var userId = int.Parse(context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);
          
            var createdRestaurantsCount =  _context
                .Restaurants
                .Count(r => r.CreatedById == userId);

            if(createdRestaurantsCount >= requirement.MinimumRestaurantCreated)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
