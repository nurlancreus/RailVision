using System.Net;

namespace RailVision.Domain.Exceptions
{
    public class SameCoordinateException : AppException
    {
        public override string Title => nameof(SameCoordinateException);
        public override string Description => "Route is not possible";

        public SameCoordinateException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
