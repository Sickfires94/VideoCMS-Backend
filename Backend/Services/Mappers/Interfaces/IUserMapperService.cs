using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;

namespace Backend.Services.Mappers.Interfaces
{
    public interface IUserMapperService
    {
        public User toEntity(UserRequestDto request);
        public UserResponseDto toResponse(User entity);
        public User toEntity(UserLoginRequestDto login);
    }
}
