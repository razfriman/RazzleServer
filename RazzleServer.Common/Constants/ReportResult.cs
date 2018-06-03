namespace RazzleServer.Common.Constants
{
    public enum ReportResult : byte
    {
        Success,
        UnableToLocate,
        Max10TimesADay,
        YouAreReportedByUser,
        UnknownError
    }
}
