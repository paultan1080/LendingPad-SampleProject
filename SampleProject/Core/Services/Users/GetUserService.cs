using System;
using System.Collections.Generic;
using BusinessEntities;
using Common;
using Common.Exceptions;
using Data.Repositories;

namespace Core.Services.Users
{
    [AutoRegister]
    public class GetUserService : IGetUserService
    {
        private readonly IUserRepository _userRepository;

        public GetUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User GetUser(Guid id)
        {
            return _userRepository.Get(id);
        }

        public IEnumerable<User> GetUsers(UserTypes? userType = null, string name = null, string email = null)
        {
            return _userRepository.Get(userType, name, email);
        }

        public IEnumerable<User> GetUsersByTag(string tag, int skip, int take)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new DomainValidationException(nameof(tag), "A tag is required.");
            }
            if (skip < 0)
            {
                throw new DomainValidationException(nameof(skip), "Skip cannot be negative.");
            }
            if (take < 1 || take > Paging.MaxPageSize)
            {
                throw new DomainValidationException(nameof(take), $"Take must be between 1 and {Paging.MaxPageSize}.");
            }

            return _userRepository.GetByTag(tag.Trim(), skip, take);
        }
    }
}