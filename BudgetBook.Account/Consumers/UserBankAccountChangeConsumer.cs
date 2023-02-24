using BudgetBook.Account.Contracts;
using BudgetBook.Account.Entities;
using BudgetBook.Account.Repositories;
using MassTransit;

namespace BudgetBook.Account.Consumers;

public class UserBankAccountChangeConsumer : IConsumer<UserBankAccountChange>
{
    private readonly IRepository<User> repository;

    public UserBankAccountChangeConsumer(IRepository<User> repository)
    {
        this.repository = repository;
    }

    public async Task Consume(ConsumeContext<UserBankAccountChange> context)
    {
        var message = context.Message;

        var item = await repository.GetAsync(message.UserId);

        if(item is null)
        {
            item = new User
            {
                Id = message.UserId,
                BankAccount = message.BankAccountChange
            };

            await repository.CreateAsync(item);
        }
        else
        {
            item.BankAccount = item.BankAccount + message.BankAccountChange;

            await repository.UpdateAsync(item);
        }
    }
}




