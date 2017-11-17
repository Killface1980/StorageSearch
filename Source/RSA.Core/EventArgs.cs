using System;

namespace RSA.Core {
    public class EventArgs<T> : EventArgs {
        public EventArgs(T value) {
            this.Value = value;
        }

        public T Value { get; set; }
    }
}
