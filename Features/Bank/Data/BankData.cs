namespace ProjectSMP.Features.Bank.Data
{
    public class PlayerBankAccount
    {
        public int Id { get; set; }
        public string CitizenId { get; set; } = "";
        public string AccountNumber { get; set; } = "";
        public string AccountName { get; set; } = "";
        public string CreationDate { get; set; } = "";
        public string LastTransaction { get; set; } = "";
        public int Balance { get; set; }
        public bool IsActive { get; set; } = true;
    }
}