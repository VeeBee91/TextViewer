using Microsoft.AspNetCore.DataProtection;

namespace TextViewer.Classes
{
    public class Encryptor
    {
        private readonly IDataProtector _protector;
        public Encryptor(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("TextViewer.Encryptor.v1");
        }
        public string Encrypt(string input)
        {
            return _protector.Protect(input);
        }

        public string Decrypt(string cipherText)
        {
            return _protector.Unprotect(cipherText);
        }
    }
}
