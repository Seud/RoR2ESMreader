namespace ESMReader
{
    enum ReaderState
    {
        MagicBytesCheck,
        AssetID,
        ESMNameL,
        ESMName,
        ESCount,
        ESNameL,
        ESName,
        ESACount,
        ESANameL,
        ESAName,
        ESAType,
        ESAValue
    }
}
