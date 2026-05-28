namespace LeagueReplayFile.Encryption;

public partial class BlowFish
{
   
    //gets the first byte in a uint
    private byte wordByte0(uint w)
    {
        return (byte)(w / 256 / 256 / 256 % 256);
    }

    //gets the second byte in a uint
    private byte wordByte1(uint w)
    {
        return (byte)(w / 256 / 256 % 256);
    }

    //gets the third byte in a uint
    private byte wordByte2(uint w)
    {
        return (byte)(w / 256 % 256);
    }

    //gets the fourth byte in a uint
    private byte wordByte3(uint w)
    {
        return (byte)(w % 256);
    } 
}