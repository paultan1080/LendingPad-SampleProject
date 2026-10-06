using System;
using BusinessEntities;
using Common;
using Common.Exceptions;
using Core.Factories;
using Data.Repositories;

namespace Core.Services.Users
{
    [AutoRegister]
    public class CreateUserService : ICreateUserService
    {
        private readonly IUpdateUserService _updateUserService;
        private readonly IIdObjectFactory<User> _userFactory;
        private readonly IUserRepository _userRepository;

        public CreateUserService(IIdObjectFactory<User> userFactory, IUserRepository userRepository, IUpdateUserService updateUserService)
        {
            _userFactory = userFactory;
            _userRepository = userRepository;
            _updateUserService = updateUserService;
        }

        public User Create(Guid id, UserDetails details)
        {
            if (id == Guid.Empty)
            {
                throw new DomainValidationException(nameof(id), "A non-empty user id is required.");
            }

            // Fails fast; a concurrent create racing past this check is still
            // rejected by optimistic concurrency when the UOW commits.
            if (_userRepository.Get(id) != null)
            {
                throw new ConflictException($"A user with id '{id}' already exists.");
            }

            var user = _userFactory.Create(id);
            _updateUserService.Update(user, details);
            _userRepository.Save(user);
            return user;
        }
    }
}