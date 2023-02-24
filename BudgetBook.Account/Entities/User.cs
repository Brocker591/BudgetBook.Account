namespace BudgetBook.Account.Entities;

public class User : IEntity
{
    public Guid Id { get; set; }
    public decimal BankAccount { get; set; }
}