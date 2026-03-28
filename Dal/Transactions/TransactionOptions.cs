namespace Dal.Transactions;

public class TransactionOptions
{
    public Dictionary<TransactionTimeoutType, double> TransactionTimeoutType { get; set; } = null!;
}
