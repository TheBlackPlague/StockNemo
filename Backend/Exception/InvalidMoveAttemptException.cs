#nullable enable
using System;
using System.Runtime.Serialization;

namespace Backend.Exception
{

    public class InvalidMoveAttemptException : InvalidOperationException
    {

        public static InvalidMoveAttemptException FromBoard(Board board, string? message)
        {
            InvalidMoveAttemptException e = new("\n"+ board + message);
            return e;
        }

        public InvalidMoveAttemptException(string? message) : base(message) {}

        public InvalidMoveAttemptException(string? message, System.Exception? innerException) 
            : base(message, innerException) {}

        protected InvalidMoveAttemptException(SerializationInfo info, StreamingContext context) : base(info, context) {}

    }

}