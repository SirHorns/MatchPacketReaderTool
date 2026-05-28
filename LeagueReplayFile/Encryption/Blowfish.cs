//Blowfish encryption (ECB and CBC MODE) as defined by Bruce Schneier here: http://www.schneier.com/paper-blowfish-fse.html
//Complies with test vectors found here: http://www.schneier.com/code/vectors.txt
//non-standard mode profided to be usable with the javascript crypto library found here: http://etherhack.co.uk/symmetric/blowfish/blowfish.html
//By FireXware, 1/7/1010, Contact: firexware@hotmail.com
//Code is partly adopted from the javascript crypto library by Daniel Rench


//USAGE:
//BlowFish b = new BlowFish("04B915BA43FEB5B6");
//string plainText = "The quick brown fox jumped over the lazy dog.";
//string cipherText = b.Encrypt_CBC(plainText);
//MessageBox.Show(cipherText);
//plainText = b.Decrypt_CBC(cipherText);
//MessageBox.Show(plainText);

using System.Security.Cryptography;

namespace LeagueReplayFile.Encryption;

public partial class BlowFish
{
    private RNGCryptoServiceProvider _randomSource;

    //SBLOCKS
    private uint[] bf_s0;
    private uint[] bf_s1;
    private uint[] bf_s2;
    private uint[] bf_s3;

    private uint[] bf_P;

    //KEY
    private byte[] key;

    //HALF-BLOCKS
    private uint xl_par;
    private uint xr_par;

    // This is dumb as fuck
    private readonly object _lock;

    /// <summary>
    /// Constructor for byte key
    /// </summary>
    /// <param name="cipherKey">Cipher key as a byte array</param>
    public BlowFish(byte[] cipherKey)
    {
        _lock = new object();
        _randomSource = new RNGCryptoServiceProvider();
        SetupKey(cipherKey);
    }

    /// <summary>
    /// Encrypts a byte array in ECB mode
    /// </summary>
    /// <param name="pt">Plaintext data</param>
    /// <returns>Ciphertext bytes</returns>
    public byte[] Encrypt(byte[] pt)
    {
        lock (_lock)
        {
            return Crypt_ECB(pt, false);
        }
    }

    /// <summary>
    /// Decrypts a byte array (ECB)
    /// </summary>
    /// <param name="ct">Ciphertext byte array</param>
    /// <returns>Plaintext</returns>
    public byte[] Decrypt(byte[] ct)
    {
        lock (_lock)
        {
            return Crypt_ECB(ct, true);
        }
    }
}