
using System;
namespace Cr7Sund.Promises
{
    public class PromiseGroupException : Exception
    {
        public readonly Exception[] Exceptions;

        public PromiseGroupException(int length)
        {
            Exceptions = new Exception[length];
        }
    }
}
