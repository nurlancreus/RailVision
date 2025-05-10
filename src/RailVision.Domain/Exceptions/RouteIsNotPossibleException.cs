using System.Net;

namespace RailVision.Domain.Exceptions
{
    public class RouteIsNotPossibleException : AppException
    {
        public override string Title => nameof(RouteIsNotPossibleException);
        public override string Description => "Route is not possible";

        public RouteIsNotPossibleException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
