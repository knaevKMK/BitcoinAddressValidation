using Bitcoin_Address_Validation.Models;
using Bitcoin_Address_Validation.Services;

while (true)
{

    Console.WriteLine("Enter BTC wallet address or  'N' to exit:");
    string? input = Console.ReadLine();
    if (input.Equals("N", StringComparison.OrdinalIgnoreCase)) { break; }
    try
    {
        AddressInfo addressInfo = Decoder.GetAddressInfo(input);
        Console.WriteLine($"Address info:\n\tAddress:{addressInfo.Address}\n\tType:{addressInfo.Type}\n\tNetwork:{addressInfo.Network}\n\tBench32: {addressInfo.Bech32}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"[ERROR] {e.Message}");
    }
}

