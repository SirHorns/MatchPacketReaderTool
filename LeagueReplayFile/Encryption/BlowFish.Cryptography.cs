namespace LeagueReplayFile.Encryption;

public partial class BlowFish
{
    
    /// <summary>
    /// Sets up the S-blocks and the key
    /// </summary>
    /// <param name="cipherKey">Block cipher key (1-448 bits)</param>
    private void SetupKey(byte[] cipherKey)
    {
        bf_P = SetupP();
        //set up the S blocks
        bf_s0 = SetupS0();
        bf_s1 = SetupS1();
        bf_s2 = SetupS2();
        bf_s3 = SetupS3();

        key = new byte[cipherKey.Length]; // 448 bits
        if (cipherKey.Length > 56)
        {
            throw new Exception("Key too long. 56 bytes required.");
        }

        Buffer.BlockCopy(cipherKey, 0, key, 0, cipherKey.Length);
        var j = 0;
        for (var i = 0; i < 18; i++)
        {
            var d = (uint)(((key[j % cipherKey.Length] * 256 + key[(j + 1) % cipherKey.Length]) * 256 + key[(j + 2) % cipherKey.Length]) * 256 + key[(j + 3) % cipherKey.Length]);
            bf_P[i] ^= d;
            j = (j + 4) % cipherKey.Length;
        }

        xl_par = 0;
        xr_par = 0;
        for (var i = 0; i < 18; i += 2)
        {
            encipher();
            bf_P[i] = xl_par;
            bf_P[i + 1] = xr_par;
        }

        for (var i = 0; i < 256; i += 2)
        {
            encipher();
            bf_s0[i] = xl_par;
            bf_s0[i + 1] = xr_par;
        }
        for (var i = 0; i < 256; i += 2)
        {
            encipher();
            bf_s1[i] = xl_par;
            bf_s1[i + 1] = xr_par;
        }
        for (var i = 0; i < 256; i += 2)
        {
            encipher();
            bf_s2[i] = xl_par;
            bf_s2[i + 1] = xr_par;
        }
        for (var i = 0; i < 256; i += 2)
        {
            encipher();
            bf_s3[i] = xl_par;
            bf_s3[i + 1] = xr_par;
        }
    }

    /// <summary>
    /// Encrypts or decrypts data in ECB mode
    /// </summary>
    /// <param name="text">plain/ciphertext</param>
    /// <param name="decrypt">true to decrypt, false to encrypt</param>
    /// <returns>(En/De)crypted data</returns>
    private byte[] Crypt_ECB(byte[] text, bool decrypt)
    {
        var block = new byte[8];
        var plainText = new byte[text.Length];

        Buffer.BlockCopy(text, 0, plainText, 0, text.Length);

        var n = plainText.Length - plainText.Length % 8;
        for (var i = 0; i < n; i += 8)
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

    // a little hack for keycheck
    public long Decrypt(ulong key)
    {
        lock (_lock)
        {
            var bytes = BitConverter.GetBytes(key);
            BlockDecrypt(ref bytes);
            return BitConverter.ToInt64(bytes, 0);
        }
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
        //ToUInt32 requires the bytes in reverse order
        Array.Reverse(block1);
        Array.Reverse(block2);

        xl_par = BitConverter.ToUInt32(block1, 0);
        xr_par = BitConverter.ToUInt32(block2, 0);
    }

    /// <summary>
    /// Converts the two uint values into a 64 bit block
    /// </summary>
    /// <param name="block">64 bit buffer to receive the block</param>
    private void GetBlock(ref byte[] block)
    {
        var block1 = BitConverter.GetBytes(xl_par);
        var block2 = BitConverter.GetBytes(xr_par);

        //GetBytes returns the bytes in reverse order
        Array.Reverse(block1);
        Array.Reverse(block2);

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
        for (uint i = 0; i < 16; i += 2)
        {
            xr_par = round(xr_par, xl_par, i + 1);
            xl_par = round(xl_par, xr_par, i + 2);
        }
        xr_par = xr_par ^ bf_P[17];

        //swap the blocks
        var swap = xl_par;
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
        var swap = xl_par;
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
        var x1 = bf_s0[wordByte0(b)] + bf_s1[wordByte1(b)] ^ bf_s2[wordByte2(b)];
        var x2 = x1 + bf_s3[wordByte3(b)];
        var x3 = x2 ^ bf_P[n];
        return x3 ^ a;
    }
}