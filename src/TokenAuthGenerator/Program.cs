using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace com.onapp.cdn
{
    class Program
    {
        private const string PARAM_EXPIRE = "expire";
        private const string PARAM_REF_ALLOW = "ref_allow";
        private const string PARAM_REF_DENY = "ref_deny";
        private static readonly List<string> SUPPORTED_PARAM = new List<string>() { PARAM_EXPIRE, PARAM_REF_ALLOW, PARAM_REF_DENY };
        
        private static Encoding encoding = Encoding.UTF8;
        private static IBlockCipher cipherEngine = new BlowfishEngine();
        private static IBlockCipherPadding padding = new Pkcs7Padding();

        private static string Cipher(bool encrypt, byte[] key, byte[] data)
        {
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(cipherEngine, padding);
            cipher.Init(encrypt, new KeyParameter(key));

            int size = cipher.GetOutputSize(data.Length);
            byte[] result = new byte[size];
            int position = cipher.ProcessBytes(data, 0, data.Length, result, 0);
            cipher.DoFinal(result, position);

            return encrypt ? BitConverter.ToString(result).Replace("-", String.Empty).ToLower() : encoding.GetString(result);
        }

        private static string Encrypt(string key, string parameters)
        {
            ParseSecurityParameters(parameters, true);
            return Cipher(true, encoding.GetBytes(key), encoding.GetBytes(parameters));
        }

        private static string Decrypt(string key, string encryptedStr)
        {
            return Cipher(false, encoding.GetBytes(key), StringToByteArray(encryptedStr));
        }

        private static byte[] StringToByteArray(string hexString)
        {
            int charCount = hexString.Length;
            byte[] buffer = new byte[charCount / 2];

            for (int index = 0; index < charCount; index += 2)
            {
                buffer[index / 2] = Convert.ToByte(hexString.Substring(index, 2), 16);
            }

            return buffer;
        }

        private static void ParseSecurityParameters(string parameters, bool isValidateSecurityParams)
        {
            if (String.IsNullOrEmpty(parameters))
                throw new ArgumentException("Parameters must not be empty");

            string[] tokens = parameters.Split(new char[] { '&' });
            List<string> param_keys = new List<string>();

            foreach (string token in tokens)
            {
                string[] strArray = token.Split(new char[] { '=' });
                if (strArray.Length != 2 || String.IsNullOrEmpty(strArray[0]) || String.IsNullOrEmpty(strArray[1]))
                    throw new ArgumentException("Malformed key/value pair");

                string paramKey = strArray[0];
                string paramValue = strArray[1];

                if (!param_keys.Contains(paramKey))
                {
                    Parse(paramKey, paramValue, isValidateSecurityParams);
                    param_keys.Add(paramKey);
                }
                else
                {
                    throw new ArgumentException(String.Format("Duplicate key '{0}' is not allowed", paramKey));
                }

                if(!SUPPORTED_PARAM.Contains(paramKey))
                    throw new ArgumentException(String.Format("Unsupported parameter '{0}'", paramKey));
            }
        }

        private static void Parse(string key, string value, bool isValidate)
        {
            if(key == PARAM_EXPIRE)
            {
                long expire = long.Parse(value);

                if(isValidate && expire <= ToUnixTime(System.DateTime.UtcNow))
                {
                    throw new ArgumentException("Parameter 'expire' should not be past date");
                }
            }
            else if(key == PARAM_REF_ALLOW || key == PARAM_REF_DENY)
            {
                // No need to validate further as C# version does not need to return Map object
            }
            else
            {
                throw new ArgumentException(String.Format("Unsupported parameter '{0}'", key));
            }
        }

        private static long ToUnixTime(DateTime dateTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((dateTime - epoch).TotalSeconds);
        }

        private static void ValidateReferrer(string referrer)
        {
            if(String.IsNullOrEmpty(referrer))
            {
                throw new ArgumentException("Referrer must not be blank");
            }

            if(referrer.StartsWith(" ") || referrer.EndsWith(" "))
            {
                throw new ArgumentException("Referrer must not start/end with space(s)");
            }

            if(referrer.Contains("*"))
            {
                if(!referrer.StartsWith("*.") || referrer.LastIndexOf("*") > 0)
                {
                    throw new ArgumentException("Wildcard usage(*.DOMAIN) for referrer must exist only at the beginning of a domain");
                }
            }

        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
                throw new ArgumentException("Expected 3 arguments. Refer to README for usage");

            if (args[0] != "encrypt" && args[0] != "decrypt")
                throw new ArgumentException("Invalid action. Refer to README for usage");

            if (args[0] == "encrypt")
            {
                string key = args[1];
                string parameters = args[2];
                string encryptedStr = Encrypt(key, parameters);
                Console.WriteLine("token=" + encryptedStr);
            }
            else if (args[0] == "decrypt")
            {
                string key = args[1];
                string encryptedStr = args[2];
                Console.WriteLine("security parameters=" + Decrypt(key, encryptedStr));
            }
        }
    }
}
