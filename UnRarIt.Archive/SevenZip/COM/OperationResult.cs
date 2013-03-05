
namespace UnRarIt.Archive
{
  internal enum OperationResult : int
  {
    OK = 0,
    UnSupportedMethod = 1,
    DataError = 2,
    CRCError = 3
  }
}
