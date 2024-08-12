namespace EndpointToCSV.Models
{
    public class Customer
    {
        public string? Id { get; set; }
        public string? FirstName {  get; set; }
        public string? LastName { get; set; }
        public string? Company {  get; set; }
        public string? City {  get; set; }
        public string? Country {  get; set; }
        public string? Phone1 {  get; set; }
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public DateTime? SubscriptionDate { get; set; }
        public string? Website { get; set; }

        public static string CSVHeader
        {
            get
            {
                return $"{nameof(Id)},{nameof(FirstName)},{nameof(LastName)},{nameof(Company)},{nameof(City)},{nameof(Country)},{nameof(Phone1)},{nameof(Phone2)},{nameof(Email)},{nameof(SubscriptionDate)},{nameof(Website)}";
            }
        }

        public string CSVData
        {
            get
            {
                return $"{Id},{FirstName},{LastName},{Company},{City},{Country},{Phone1},{Phone2},{Email},{SubscriptionDate},{Website}";
            }
        }
    }
}
