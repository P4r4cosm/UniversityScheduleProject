using Microsoft.EntityFrameworkCore;
using University_Schedule_Gateway.Models;

namespace University_Schedule_Gateway.Repositories;

public class UserRepository
{
    private readonly AuthenticationContext _contextOld;

    public UserRepository(AuthenticationContext dbContextOld)
        => _contextOld = dbContextOld;

    public async Task Create(User user)
    {
        await _contextOld.Users.AddAsync(user);
        await _contextOld.SaveChangesAsync();
    }

    public async Task<User> GetByName(string name)
    {
        return await _contextOld.Users.FirstOrDefaultAsync(u => u.Name == name);
    }
}