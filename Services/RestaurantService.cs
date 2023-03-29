using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using System.Linq.Expressions;
using System.Security.Claims;

namespace RestaurantAPI.Services
{
    public interface IRestaurantService
    {
        RestaurantDto GetById(int id);
        PagedResult<RestaurantDto> GetAll(RestaurantQuery query);
        int Create(CreateRestaurantDto dto);
        void Delete(int id);
        void Update(int id, UpdateRestaurantDto dto);
    }
    public class RestaurantService : IRestaurantService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RestaurantService> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextServices _userContextServices;

        public RestaurantService(RestaurantDbContext dbContext, IMapper mapper, ILogger<RestaurantService> logger
            , IAuthorizationService authorizationService, IUserContextServices userContextServices)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextServices = userContextServices;
        }
        public void Update(int id, UpdateRestaurantDto dto)
        {
            var restaurnat = _dbContext
                 .Restaurants
                 .FirstOrDefault(r => r.Id == id);

            if (restaurnat is null) 
                throw new NotFoundException("Restaurnat not found");

            var authorizationResult = 
                _authorizationService
                .AuthorizeAsync(_userContextServices.User, restaurnat, new ResourceOperationRequirement(ResourceOperation
                .Update))
                .Result;

            if (!authorizationResult.Succeeded) 
            {
                throw new ForbidException();
            }

            restaurnat.Name = dto.Name;
            restaurnat.Description = dto.Description;
            restaurnat.HasDelivery = dto.HasDelivery;
            _dbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            _logger.LogError($"Restaurant with id: {id} DELETE action invoked");

            var restaurnat = _dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id);

            if (restaurnat is null)
                throw new NotFoundException("Restaurnat not found");

            var authorizationResult =
                _authorizationService
                .AuthorizeAsync(_userContextServices.User, restaurnat, new ResourceOperationRequirement(ResourceOperation
                .Delete))
                .Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }

            _dbContext.Restaurants.Remove(restaurnat);
            _dbContext.SaveChanges();
        }
        public RestaurantDto GetById(int id)
        {
            var restaurnat = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == id);


            if (restaurnat == null)
                throw new NotFoundException("Restaurnat not found"); 
            
            var result = _mapper.Map<RestaurantDto>(restaurnat);
            return result;
        }

        public PagedResult<RestaurantDto> GetAll(RestaurantQuery query)
        {
            var baseQuery = _dbContext
                .Restaurants.Include(r => r.Address)
                .Include(r => r.Dishes)
                .Where(r => query.SearchPhrase == null || (r.Name.ToLower().Contains(query.SearchPhrase.ToLower())
                                                    || r.Description.ToLower().Contains(query.SearchPhrase.ToLower())));

            if (string.IsNullOrEmpty(query.SortBy))
            {
                var columsSelectors = new Dictionary<string, Expression<Func<Restaurant, object>>>()
                {
                    {nameof(Restaurant.Name), r => r.Name },
                    {nameof(Restaurant.Description), r => r.Description },
                    {nameof(Restaurant.Category), r => r.Category },
                };

                var selectedColumn = columsSelectors[query.SortBy];

                baseQuery = query.SortDirection == SortDirection.ASC ?
                    baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }

            var restaurants = baseQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToList();

            var totalItemsCount = baseQuery.Count();

            var restaurantsDtos = _mapper.Map<List<RestaurantDto>>(restaurants);

            var result = new PagedResult<RestaurantDto>(restaurantsDtos, totalItemsCount, query.PageSize, query.PageNumber); 

            return result;
        }

        public int Create(CreateRestaurantDto dto)
        {
            var restaurant = _mapper.Map<Restaurant>(dto);
            restaurant.CreatedById = _userContextServices.GetUserId;
            _dbContext.Restaurants.Add(restaurant);
            _dbContext.SaveChanges();

            return restaurant.Id;
        }

        
    }
}
