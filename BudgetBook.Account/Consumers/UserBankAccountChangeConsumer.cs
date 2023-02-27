using BudgetBook.Account.Contracts;
using BudgetBook.Account.Entities;
using BudgetBook.Account.Repositories;
using MassTransit;

namespace BudgetBook.Account.Consumers;

public class UserBankAccountChangeConsumer : IConsumer<UserBankAccountChange>
{
    private readonly IRepository<User> repository;
    private readonly ILogger<UserBankAccountChangeConsumer> logger;

    public UserBankAccountChangeConsumer(IRepository<User> repository, ILogger<UserBankAccountChangeConsumer> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<UserBankAccountChange> context)
    {
        try
        {
            var message = context.Message;
            var item = await repository.GetAsync(message.UserId);

            if (item is null)
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
        catch (System.Exception ex)
        {
            logger.LogError(ex.Message);
        }

    }
}




