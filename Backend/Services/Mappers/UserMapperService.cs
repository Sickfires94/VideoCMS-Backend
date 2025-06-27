using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;
using Backend.Services.Mappers.Interfaces;

namespace Backend.Services.Mappers
{
    public class UserMapperService : IUserMapperService
    {
        public User toEntity(UserRequestDto request)
        {
            return new User
            {
                userName = request.userName,
                userPassword = request.userPassword,
                userEmail = request.userEmail,
                role = request.role
            };
        }


        public UserResponseDto toResponse(User entity)
        {
            return new UserResponseDto 
            { 
                userId = entity.userId,
                userName = entity.userName,
                userEmail = entity.userEmail,
                role = entity.role,
                userCreatedDate = entity.userCreatedDate
            };
        }


        public User toEntity(UserLoginRequestDto request)
        {
            return new User {
                userEmail = request.userEmail,
                userPassword = request.userPassword
            };
        }
    }
}
