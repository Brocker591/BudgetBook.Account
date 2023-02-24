using BudgetBook.Account.Entities;

namespace BudgetBook.Account;

public static class Extensions
{
    public static UserDto AsDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.BankAccount
        );
    }
}