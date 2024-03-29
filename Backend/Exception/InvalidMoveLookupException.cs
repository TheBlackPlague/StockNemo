﻿#nullable enable
using System;
using System.Runtime.Serialization;

namespace Backend.Exception;

[Serializable]
public class InvalidMoveLookupException : InvalidOperationException
{

    public static InvalidMoveLookupException FromBoard(Board board, string? message)
    {
        InvalidMoveLookupException e = new("\n" + board + message);
        return e;
    }

    public InvalidMoveLookupException(string? message) : base(message) {}

    public InvalidMoveLookupException(string? message, System.Exception? innerException) 
        : base(message, innerException) {}

    protected InvalidMoveLookupException(SerializationInfo info, StreamingContext context) : base(info, context) {}

}