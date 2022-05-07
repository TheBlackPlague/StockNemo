#nullable enable
using System;
using System.Runtime.Serialization;
using Backend.Board;
using Backend.Move;

namespace Backend.Exception
{

    public class InvalidMoveAttemptException : InvalidOperationException
    {

        public static InvalidMoveAttemptException FromBoard(BitDataBoard board, Log moveLog, string? message)
        {
            InvalidMoveAttemptException e = new("\n"+ board + moveLog + message);
            return e;
        }

        public InvalidMoveAttemptException(string? message) : base(message) {}

        public InvalidMoveAttemptException(string? message, System.Exception? innerException) 
            : base(message, innerException) {}

        protected InvalidMoveAttemptException(SerializationInfo info, StreamingContext context) : base(info, context) {}

    }

}