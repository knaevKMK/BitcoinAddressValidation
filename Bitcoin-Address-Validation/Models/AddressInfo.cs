namespace Bitcoin_Address_Validation.Models
{
    using Bitcoin_Address_Validation.Enums;
    public class AddressInfo
    {
        public bool Bech32 { get; set; }
        public Network Network { get; set; }
        public string Address { get; set; } = string.Empty;
        public AddressType Type { get; set; }
    }
}
