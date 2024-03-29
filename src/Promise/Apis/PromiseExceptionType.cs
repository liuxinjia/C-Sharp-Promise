namespace Cr7Sund.Promises
{
    public enum PromiseExceptionType
    {
        /// <summary>
        /// Attempt to resolve a promise that is already in resolved_state or rejected_state
        /// a promise can only be resolved when it is still in pending state
        /// </summary>
        Valid_RESOLVED_STATE,
        /// <summary>
        /// Attempt to rejected a promise that is already in rejected_state or resolved state
        /// a promise can only be rejected when it is still in pending state
        /// </summary>
        Valid_REJECTED_STATE,
        /// <summary>
        /// Attempt to report progress that is already in rejected_state or resolved state
        /// a promise can only be rejected when it is still in pending state
        /// </summary>
        Valid_PROGRESS_STATE,
        // No promise for race been provided , the result is undefined will be fulfilled or rejected
        EMPTY_PROMISE_RACE,
        // No promise for any been provided , the result is undefined will be fulfilled or rejected
        EMPTY_PROMISE_ANY,
        // there is an exception happen in OnExecuteAsync
        EXCEPTION_ON_ExecuteAsync,
        // there is no promise command to bind
        EMPTY_PROMISE_TOREACT,
        // This version of the function must supply an onResolved.
        // Otherwise there is now way to get the converted value to pass to the resulting promise.
        NO_UNRESOLVED,
        // forbid conversion in fist chain
        CONVERT_FIRST,
        // can not react an released binding , try to do not using at once
        CAN_NOT_REACT_RELEASED,
        // can not react an running binding since using at once, try to use ForceStop
        CAN_NOT_REACT_RUNNING,
    }
}
