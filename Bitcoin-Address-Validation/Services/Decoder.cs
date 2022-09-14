using Bitcoin_Address_Validation.Enums;
using Bitcoin_Address_Validation.Models;
using Nano.Bech32;
using System.Security.Cryptography;

namespace Bitcoin_Address_Validation.Services
{
    public class Decoder
    {

        private static AddressInfo ParseBech32(string address)
        {
            Nano.Bech32.Bech32Encoder.Decode(address, out string? decodeString, out byte[]? decodeByteArr);

            if (string.IsNullOrEmpty(decodeString) && (decodeByteArr is null || decodeByteArr.Length == 0))
            {
                throw new Exception("Invalid address");
            }

            var mapPrefixToNetwork = new Dictionary<string, Network>{
                                                                        { "bc", Network.mainnet },
                                                                        { "tb", Network.testnet },
                                                                        { "bcrt", Network.regtest }
                                                                      };
            Network network;
            try
            {
                network = mapPrefixToNetwork.FirstOrDefault(d => decodeString.StartsWith(d.Key)).Value;


                var witnessVersion = Convert.ToInt32(decodeString[..1]);

                if (witnessVersion < 0 || witnessVersion > 16)
                {
                    throw new Exception("Invalid address");
                }
                Bech32Encoder.Decode(decodeString[..1], out string? _decodeStr, out byte[]? data);

                AddressType type;

                if (data.Count() == 20)
                {
                    type = AddressType.p2wpkh;
                }
                else if (witnessVersion == 1)
                {
                    type = AddressType.p2tr;
                }
                else
                {
                    type = AddressType.p2wsh;
                }

                return new AddressInfo
                {
                    Bech32 = true,
                    Network = network,
                    Address = address,
                    Type = type
                };
            }
            catch
            {
                throw new Exception("Invalid address");
            }
        }

        public static AddressInfo GetAddressInfo(string address)
        {
            byte[] decoded;
            string prefix = address.Substring(0, 2).ToLower();

            if (prefix.Equals("bc") || prefix.Equals("tb"))
            {
                return ParseBech32(address);
            }

            try
            {
                decoded = Base58.Decode(address);
            }
            catch (Exception)
            {
                throw new Exception("Invalid address");
            }

            var length = decoded.Length;

            if (length != 25)
            {
                throw new Exception("Invalid address");
            }

            var version = decoded[0];

            var checksum = decoded.Skip(length - 4).ToArray();
            var body = decoded.SkipLast(length - 4).ToArray();
            var expectedChecksum = SHA256.HashData(body.Take(4).ToArray());

            if (checksum.Where((value, index) => value != expectedChecksum[index]).Any())
            {
                throw new Exception("Invalid address");
            }

            var versionHex = Convert.ToInt32(version);

            Dictionary<int, AddressInfo> AddressTypes = new Dictionary<int, AddressInfo> {
                                                                    {0x00, new AddressInfo {Type= AddressType.p2pkh, Network= Network.mainnet } },
                                                                    {0x6f, new AddressInfo {Type= AddressType.p2pkh, Network= Network.testnet} },
                                                                    {0x05, new AddressInfo {Type= AddressType.p2sh, Network= Network.mainnet } },
                                                                    {0xc4, new AddressInfo {Type= AddressType.p2sh, Network= Network.testnet } }
                                                                };
            bool validVersions = AddressTypes.ContainsKey(versionHex);

            if (!validVersions)
            {
                throw new Exception("Invalid address");
            }

            var addressType = AddressTypes[version];

            return new AddressInfo
            {
                Address = address,
                Bech32 = false
            };
        }

        public static bool Validate(string address, Network? network)
        {
            try
            {
                var addressInfo = GetAddressInfo(address);

                if (network is not null)
                {
                    return network.Equals(addressInfo.Network);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
