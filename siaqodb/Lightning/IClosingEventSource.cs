using System;

namespace LightningDB
{
    /// <summary>
    /// Contains Closing event.
    /// </summary>
    internal interface IClosingEventSource
    {
        /// <summary>
        /// Triggers when hen closable object is closing.
        /// </summary>
        event EventHandler<LightningClosingEventArgs> Closing;
    }
}
