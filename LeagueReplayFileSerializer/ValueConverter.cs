namespace LeaguePacketsSerializer;

public static class ValueConverter
{
    public static float ConvertFloat(byte[] bytes, ref int readIndex)
    {
        float value = 0;
        if (bytes[readIndex] == 0xFF)
        {
            readIndex++;
        }
        else
        {
            int startIndex = readIndex;
            if (bytes[readIndex] == 0xFE)
            {
                startIndex++;
            }

            value = BitConverter.ToSingle(bytes, startIndex);
            readIndex = startIndex + 4;
        }
        return value;
    }
}