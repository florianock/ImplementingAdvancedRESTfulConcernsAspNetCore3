using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionConstraints
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string _requestHeaderToMatch;
        private readonly MediaTypeCollection _mediaTypes = new();

        public RequestHeaderMatchesMediaTypeAttribute(
            string requestHeaderToMatch,
            string mediaType, 
            params string[] otherMediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch
                                    ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));
            
            // check if the inputted media types are valid media types
            // and add them to the _mediaTypes collection.
            
            AddIfValid(mediaType, _mediaTypes, nameof(mediaType));

            foreach (var otherMediaType in otherMediaTypes)
            {
                AddIfValid(otherMediaType, _mediaTypes, nameof(otherMediaTypes));
            }
        }

        private static void AddIfValid(string mediaType, MediaTypeCollection mediaTypes, string argumentName)
        {
            if (MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType))
            {
                mediaTypes.Add(parsedMediaType);
            }
            else
            {
                throw new ArgumentException(null, argumentName);
            }
        }

        public bool Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            var parsedRequestMediaType = new MediaType(requestHeaders[_requestHeaderToMatch]);
            
            // if one of the media types matches, return true
            var mediaTypes = _mediaTypes.Select(mediaType => new MediaType(mediaType));
            var bools = mediaTypes.Select(parsedMediaType => parsedRequestMediaType.Equals(parsedMediaType));
            var result = bools.Any(b => b);

            return result;
        }

        public int Order => 0;
    }
}