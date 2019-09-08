using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Exceptions
{
    public class SaveCancelledException : Exception
    {
        public SaveCancelledException() { }
        public SaveCancelledException(string msg) : base(msg) { }
        public SaveCancelledException(string msg, Exception inner) : base(msg, inner) { }
    }
    public class NodeNameExistsException : Exception
    {
        public NodeNameExistsException() { }
        public NodeNameExistsException(string msg) : base(msg) { }
        public NodeNameExistsException(string msg, Exception inner) : base(msg, inner) { }
    }
    public class NoParentExistsException : Exception
    {
        public NoParentExistsException() { }
        public NoParentExistsException(string msg) : base(msg) { }
        public NoParentExistsException(string msg, Exception inner) : base(msg, inner) { }
    }
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException() { }
        public UserNotFoundException(string msg) : base(msg) { }
        public UserNotFoundException(string msg, Exception inner) : base(msg, inner) { }
    }
}
