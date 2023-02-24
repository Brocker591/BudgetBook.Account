using BudgetBook.Account.Entities;
using BudgetBook.Account.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetBook.Account.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IRepository<User> userRepository;

    public AccountController(IRepository<User> userRepository)
    {
        this.userRepository = userRepository;
    }


    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllAsync()
    {
        var items = await userRepository.GetAllAsync();
        var users = items.Select(item => item.AsDto()).ToList();

        return users;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> GetAsync()
    {
        var userIdAsString = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (userIdAsString == null)
            return BadRequest("Kein User vorhanden");

        Guid userId = new Guid(userIdAsString);

        var model = await userRepository.GetAsync(userId);
        if (model is null)
            return NotFound();

        return model.AsDto();
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateAsync(UserDto dto)
    {
        User user = new()
        {
            Id = dto.Id,
            BankAccount = dto.BankAccount
        };

        await userRepository.CreateAsync(user);

        return user.AsDto();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateAsync(UserDto dto)
    {
        var existingModel = await userRepository.GetAsync(dto.Id);

        if (existingModel is null)
            return NotFound();

        existingModel.BankAccount = dto.BankAccount;

        await userRepository.UpdateAsync(existingModel);

        return NoContent();

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        var existingModel = await userRepository.GetAsync(id);

        if (existingModel is null)
            return NotFound();

        await userRepository.RemoveAsync(id);

        return NoContent();

    }



}