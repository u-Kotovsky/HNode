public static class Util
{
    /// <summary>
    /// Helper function for block alpha, to support various automatic masking features
    /// check out <see cref="ShowConfiguration.autoMaskOnZero"/>
    /// </summary>
    /// <param name="channelValue"></param>
    /// <returns></returns>
    internal static byte GetBlockAlpha(byte channelValue)
    {
        if (Loader.showconf.autoMaskOnZero && channelValue == 0)
        {
            return 0;
        }

        return 255;
    }
}
