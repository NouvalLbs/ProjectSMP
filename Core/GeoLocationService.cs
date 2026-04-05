using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using System;
using System.IO;

namespace ProjectSMP.Core
{
    public static class GeoLocationService
    {
        private static DatabaseReader _reader;
        private const string DatabasePath = "scriptfiles/GeoLite2-City.mmdb";

        public static void Initialize()
        {
            try
            {
                if (!File.Exists(DatabasePath))
                {
                    Console.WriteLine($"[+] GeoLocation - Database not found: {DatabasePath}");
                    return;
                }

                _reader = new DatabaseReader(DatabasePath);
                Console.WriteLine("[+] GeoLocation - Database loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] GeoLocation - Failed to load database: {ex.Message}");
            }
        }

        public static (string City, string Country) GetLocation(string ipAddress)
        {
            if (_reader == null)
                return ("Unknown", "Unknown");

            try
            {
                var response = _reader.City(ipAddress);
                var city = response.City?.Name ?? "Unknown";
                var country = response.Country?.Name ?? "Unknown";
                return (city, country);
            }
            catch (AddressNotFoundException)
            {
                return ("Unknown", "Unknown");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] GeoLocation - Error getting location for {ipAddress}: {ex.Message}");
                return ("Unknown", "Unknown");
            }
        }

        public static void Dispose()
        {
            _reader?.Dispose();
            Console.WriteLine("[+] GeoLocation - Service disposed");
        }
    }
}