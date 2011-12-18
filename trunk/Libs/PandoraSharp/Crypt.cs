//Blowfish encryption (ECB, CBC and CTR mode) as defined by Bruce Schneier here: http://www.schneier.com/paper-blowfish-fse.html
//Complies with test vectors found here: http://www.schneier.com/code/vectors.txt
//non-standard mode provided to be usable with the javascript crypto library found here: http://etherhack.co.uk/symmetric/blowfish/blowfish.html
//By FireXware, 1/7/1010, Contact: firexware@hotmail.com
//Code is partly adopted from the javascript crypto library by Daniel Rench

/*USAGE:

Provide the key when creating the object. The key can be any size up to 448 bits.
The key can be given as a hex string or an array of bytes.
  BlowFish b = new BlowFish("04B915BA43FEB5B6");

The plainText can be either a string or byte array.
  string plainText = "The quick brown fox jumped over the lazy dog.";

Use the Encypt_* methods to encrypt the plaintext in the mode that you want.
To Encrypt or decrypt a byte array using CBC or CTR mode, an array of bytes, you must provide an initialization vector.
A random IV can be created by calling SetRandomIV, then accessed with the IV property as it will be required to decrypt the data.
It is safe for the IV to be known by an attacker, as long as it is NEVER reused. IVs are handled automatically when encrypting and decrypting strings.
  string cipherText = b.Encrypt_CBC(plainText);
  MessageBox.Show(cipherText);

Use the same mode of operation for decryption.
  plainText = b.Decrypt_CBC(cipherText);
  MessageBox.Show(plainText);
*/

/*Which mode should I use?
 *---ECB---
 *  ECB mode encrypts each block of data with the same key, so patterns in a large set of data will be visible. 
 *  Encrypting the same data with the same key will result in the same ciphertext. This mode should NOT be used unless neccessary.
 *  
 *---CBC---
 *  CBC mode encrypts each block of data in succession so that any changes in the data will result in a completly different ciphertext.
 *  Also, an IV is used so that encrypting the same data with the same key will result in a different ciphertext. 
 *  CBC mode is the most popular mode of operation.
 *  
 *---CTR---
 *  CTR mode uses an IV and a counter to encrypt each block individually. 
 *  Like ECB mode, with the added protection of an IV to make sure the same plaintext encrypted with the same key yeilds a different result.
 *  The counter ensures that no patterns will be visible. CTR mode is secure and can be optimized for multi-threaded applications.
 * 
 * For more information on cipher modes of operation, see http://en.wikipedia.org/wiki/Block_cipher_modes_of_operation
 */

/*Things to remember
 * -Always use unique initialization vectors when using CBC and CTR mode. SetRandomIV will do the job for you.
 * -Blowfish is only as secure as the key you provide. When derriving a key from a password, use a secure hash function such as SHA-256 to create the key.
 * -Read "Which mode should I use?" and choose the best mode for your application
 * -Use a MAC to ensure that the ciphertext and IV have not been modified.
 * -Do not use the compatibility mode unless neccessary.
 */

/*
 * Modified by Adam Haile to work easier with PandoraSharp
 */
using System;
using System.Security.Cryptography;
using System.Text;

namespace PandoraSharp
{
    public class PandoraCrypt
    {
        private static readonly BlowFish bf_enc = new BlowFish(CryptType.Encrypt);
        private static readonly BlowFish bf_dec = new BlowFish(CryptType.Decrypt);

        public static string Encrypt(string s)
        {
            return bf_enc.Encrypt_ECB(s);
        }

        public static string Decrypt(string s)
        {
            return bf_dec.Decrypt_ECB(s).TrimEnd('\b');
        }
    }

    public enum CryptType
    {
        Encrypt,
        Decrypt
    }

    internal class BlowFish
    {
        #region "Global variables and constants"

        private const int ROUNDS = 16;
                          //standard is 16, to increase the number of rounds, bf_P needs to be equal to the number of rouds. Use digits of PI.

        //Random number generator for creating IVs
        private readonly RNGCryptoServiceProvider randomSource;
        private byte[] InitVector;
        private uint[] bf_P;

        //SBLOCKS
        private uint[] bf_s0;
        private uint[] bf_s1;
        private uint[] bf_s2;
        private uint[] bf_s3;
        private bool nonStandardMethod;

        //HALF-BLOCKS
        private uint xl_par;
        private uint xr_par;

        //Initialization Vector for CBC and CTR mode
        private bool IVSet;

        #endregion

        #region "Constructors"

        public BlowFish(CryptType type)
        {
            randomSource = new RNGCryptoServiceProvider();
            SetupKey(type);
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Initialization vector for CBC mode.
        /// </summary>
        public byte[] IV
        {
            get { return InitVector; }
            set
            {
                if (value.Length == 8)
                {
                    InitVector = value;
                    IVSet = true;
                }
                else
                {
                    throw new Exception("Invalid IV size.");
                }
            }
        }

        public bool NonStandard
        {
            get { return nonStandardMethod; }
            set { nonStandardMethod = value; }
        }

        /// <summary>
        /// Encrypt a string in ECB mode
        /// </summary>
        /// <param name="pt">Plaintext to encrypt as ascii string</param>
        /// <returns>hex value of encrypted data</returns>
        public string Encrypt_ECB(string pt)
        {
            return ByteToHex(Encrypt_ECB(Encoding.ASCII.GetBytes(pt)));
        }

        /// <summary>
        /// Decrypts a string (ECB)
        /// </summary>
        /// <param name="ct">hHex string of the ciphertext</param>
        /// <returns>Plaintext ascii string</returns>
        public string Decrypt_ECB(string ct)
        {
            return Encoding.ASCII.GetString(Decrypt_ECB(HexToByte(ct))).Replace("\0", "");
        }

        /// <summary>
        /// Encrypts a byte array in ECB mode
        /// </summary>
        /// <param name="pt">Plaintext data</param>
        /// <returns>Ciphertext bytes</returns>
        public byte[] Encrypt_ECB(byte[] pt)
        {
            return Crypt_ECB(pt, false);
        }

        /// <summary>
        /// Decrypts a byte array (ECB)
        /// </summary>
        /// <param name="ct">Ciphertext byte array</param>
        /// <returns>Plaintext</returns>
        public byte[] Decrypt_ECB(byte[] ct)
        {
            return Crypt_ECB(ct, true);
        }

        /// <summary>
        /// Creates and sets a random initialization vector.
        /// </summary>
        /// <returns>The random IV</returns>
        public byte[] SetRandomIV()
        {
            InitVector = new byte[8];
            randomSource.GetBytes(InitVector);
            IVSet = true;
            return InitVector;
        }

        #endregion

        #region Cryptography

        private void SetupKey(CryptType type)
        {
            if (type == CryptType.Encrypt)
            {
                bf_P = CipherKeys.out_key_p;
                bf_s0 = CipherKeys.out_key_s[0];
                bf_s1 = CipherKeys.out_key_s[1];
                bf_s2 = CipherKeys.out_key_s[2];
                bf_s3 = CipherKeys.out_key_s[3];
            }
            else
            {
                bf_P = CipherKeys.in_key_p;
                bf_s0 = CipherKeys.in_key_s[0];
                bf_s1 = CipherKeys.in_key_s[1];
                bf_s2 = CipherKeys.in_key_s[2];
                bf_s3 = CipherKeys.in_key_s[3];
            }

            return;
        }

        /// <summary>
        /// Encrypts or decrypts data in ECB mode
        /// </summary>
        /// <param name="text">plain/ciphertext</param>
        /// <param name="decrypt">true to decrypt, false to encrypt</param>
        /// <returns>(En/De)crypted data</returns>
        private byte[] Crypt_ECB(byte[] text, bool decrypt)
        {
            int paddedLen = (text.Length%8 == 0 ? text.Length : text.Length + 8 - (text.Length%8));
            var plainText = new byte[paddedLen];
            Buffer.BlockCopy(text, 0, plainText, 0, text.Length);
            var block = new byte[8];
            for (int i = 0; i < plainText.Length; i += 8)
            {
                Buffer.BlockCopy(plainText, i, block, 0, 8);
                if (decrypt)
                {
                    BlockDecrypt(ref block);
                }
                else
                {
                    BlockEncrypt(ref block);
                }
                Buffer.BlockCopy(block, 0, plainText, i, 8);
            }
            return plainText;
        }

        /// <summary>
        /// Encrypts a 64 bit block
        /// </summary>
        /// <param name="block">The 64 bit block to encrypt</param>
        private void BlockEncrypt(ref byte[] block)
        {
            SetBlock(block);
            encipher();
            GetBlock(ref block);
        }

        /// <summary>
        /// Decrypts a 64 bit block
        /// </summary>
        /// <param name="block">The 64 bit block to decrypt</param>
        private void BlockDecrypt(ref byte[] block)
        {
            SetBlock(block);
            decipher();
            GetBlock(ref block);
        }

        /// <summary>
        /// Splits the block into the two uint values
        /// </summary>
        /// <param name="block">the 64 bit block to setup</param>
        private void SetBlock(byte[] block)
        {
            var block1 = new byte[4];
            var block2 = new byte[4];
            Buffer.BlockCopy(block, 0, block1, 0, 4);
            Buffer.BlockCopy(block, 4, block2, 0, 4);
            //split the block
            if (nonStandardMethod)
            {
                xr_par = BitConverter.ToUInt32(block1, 0);
                xl_par = BitConverter.ToUInt32(block2, 0);
            }
            else
            {
                //ToUInt32 requires the bytes in reverse order
                Array.Reverse(block1);
                Array.Reverse(block2);
                xl_par = BitConverter.ToUInt32(block1, 0);
                xr_par = BitConverter.ToUInt32(block2, 0);
            }
        }

        /// <summary>
        /// Converts the two uint values into a 64 bit block
        /// </summary>
        /// <param name="block">64 bit buffer to receive the block</param>
        private void GetBlock(ref byte[] block)
        {
            var block1 = new byte[4];
            var block2 = new byte[4];
            if (nonStandardMethod)
            {
                block1 = BitConverter.GetBytes(xr_par);
                block2 = BitConverter.GetBytes(xl_par);
            }
            else
            {
                block1 = BitConverter.GetBytes(xl_par);
                block2 = BitConverter.GetBytes(xr_par);

                //GetBytes returns the bytes in reverse order
                Array.Reverse(block1);
                Array.Reverse(block2);
            }
            //join the block
            Buffer.BlockCopy(block1, 0, block, 0, 4);
            Buffer.BlockCopy(block2, 0, block, 4, 4);
        }

        /// <summary>
        /// Runs the blowfish algorithm (standard 16 rounds)
        /// </summary>
        private void encipher()
        {
            xl_par ^= bf_P[0];
            for (uint i = 0; i < ROUNDS; i += 2)
            {
                xr_par = round(xr_par, xl_par, i + 1);
                xl_par = round(xl_par, xr_par, i + 2);
            }
            xr_par = xr_par ^ bf_P[17];

            //swap the blocks
            uint swap = xl_par;
            xl_par = xr_par;
            xr_par = swap;
        }

        /// <summary>
        /// Runs the blowfish algorithm in reverse (standard 16 rounds)
        /// </summary>
        private void decipher()
        {
            xl_par ^= bf_P[17];
            for (uint i = 16; i > 0; i -= 2)
            {
                xr_par = round(xr_par, xl_par, i);
                xl_par = round(xl_par, xr_par, i - 1);
            }
            xr_par = xr_par ^ bf_P[0];

            //swap the blocks
            uint swap = xl_par;
            xl_par = xr_par;
            xr_par = swap;
        }

        /// <summary>
        /// one round of the blowfish algorithm
        /// </summary>
        /// <param name="a">See spec</param>
        /// <param name="b">See spec</param>
        /// <param name="n">See spec</param>
        /// <returns></returns>
        private uint round(uint a, uint b, uint n)
        {
            uint x1 = (bf_s0[wordByte0(b)] + bf_s1[wordByte1(b)]) ^ bf_s2[wordByte2(b)];
            uint x2 = x1 + bf_s3[wordByte3(b)];
            uint x3 = x2 ^ bf_P[n];
            return x3 ^ a;
        }

        #endregion

        #region Conversions

        //gets the first byte in a uint
        private byte wordByte0(uint w)
        {
            return (byte) (w/256/256/256%256);
        }

        //gets the second byte in a uint
        private byte wordByte1(uint w)
        {
            return (byte) (w/256/256%256);
        }

        //gets the third byte in a uint
        private byte wordByte2(uint w)
        {
            return (byte) (w/256%256);
        }

        //gets the fourth byte in a uint
        private byte wordByte3(uint w)
        {
            return (byte) (w%256);
        }

        //converts a byte array to a hex string
        private string ByteToHex(byte[] bytes)
        {
            var s = new StringBuilder();
            foreach (byte b in bytes)
                s.Append(b.ToString("x2"));
            return s.ToString();
        }

        //converts a hex string to a byte array
        private byte[] HexToByte(string hex)
        {
            var r = new byte[hex.Length/2];
            for (int i = 0; i < hex.Length - 1; i += 2)
            {
                byte a = GetHex(hex[i]);
                byte b = GetHex(hex[i + 1]);
                r[i/2] = (byte) (a*16 + b);
            }
            return r;
        }

        //converts a single hex character to it's decimal value
        private byte GetHex(char x)
        {
            if (x <= '9' && x >= '0')
            {
                return (byte) (x - '0');
            }
            else if (x <= 'z' && x >= 'a')
            {
                return (byte) (x - 'a' + 10);
            }
            else if (x <= 'Z' && x >= 'A')
            {
                return (byte) (x - 'A' + 10);
            }
            return 0;
        }

        #endregion
    }
}