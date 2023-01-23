using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// An event-based class for managing state
/// </summary>
public class StateManager<T>
{
    public class StateChangedEventArgs : EventArgs
    {
        public T newState;
        public T previousState;
        public StateChangedEventArgs(T _newState, T _previousState)
        {
            newState = _newState;
            previousState = _previousState;
        }
    }

    public class StateChangedFromEventArgs : EventArgs
    {
        public T newState;
        public StateChangedFromEventArgs(T _newState)
        {
            newState = _newState;
        }
    }

    public class StateChangedToEventArgs : EventArgs
    {
        public T previousState;
        public StateChangedToEventArgs(T _previousState)
        {
            previousState = _previousState;
        }
    }

    public class StateEvents
    {
        EventHandler<StateChangedToEventArgs> _changedTo;
        public EventHandler<StateChangedToEventArgs> changedTo { get { return _changedTo; } set { _changedTo = value ?? delegate (object sender, StateChangedToEventArgs args) { }; } }
        EventHandler<StateChangedFromEventArgs> _changedFrom;
        public EventHandler<StateChangedFromEventArgs> changedFrom { get { return _changedFrom; } set { _changedFrom = value ?? delegate (object sender, StateChangedFromEventArgs args) { }; } }

        public StateEvents(EventHandler<StateChangedToEventArgs> changedToHandler = null, EventHandler<StateChangedFromEventArgs> changedFromHandler = null)
        {
            changedTo = changedToHandler;
            changedFrom = changedFromHandler;
        }
    }

    T _currentState;
    public T currentState { get { return _currentState; } }

    Dictionary<T, StateEvents> _states;
    public Dictionary<T, StateEvents> states { get { return _states; } }

    EventHandler<StateChangedEventArgs> _stateChanged;

    public StateManager(IEnumerable<KeyValuePair<T, StateEvents>> initialStates, T state, EventHandler<StateChangedEventArgs> handler = null)
    {
        Init(initialStates, state, handler);
    }

    public StateManager(IEnumerable<T> initialStates, T state, EventHandler<StateChangedEventArgs> handler = null)
    {
        Init(initialStates, state, handler);
    }

    void Init<S>(IEnumerable<S> initialStates, T state, EventHandler<StateChangedEventArgs> handler)
    {
        if (initialStates == null)
        {
            throw new ArgumentNullException("initialStates", "Parameter `initialStates` cannot be null.");
        }

        if (initialStates.Count() == 0)
        {
            throw new ArgumentException("Parameter `initialStates` cannot be empty.", "initialStates");
        }

        if (typeof(S) == typeof(T))
        {
            _states = (initialStates as IEnumerable<T>).ToDictionary(s => s, e => new StateEvents());
        }
        else if (typeof(S) == typeof(KeyValuePair<T, StateEvents>))
        {
            _states = (initialStates as IEnumerable<KeyValuePair<T, StateEvents>>).ToDictionary(s => s.Key, e => e.Value ?? new StateEvents());
        }
        else
        {
            throw new ArgumentException("Parameter `initialStates` must be of type IEnumerable<T> or IEnumerable<KeyValuePair<T, StateEvents>>.", "initialStates");
        }

        if (!_states.ContainsKey(state))
        {
            throw new ArgumentOutOfRangeException("state", "Parameter `state` must be an element of `initialStates`.");
        }

        _currentState = state;
        _stateChanged = handler ?? delegate (object sender, StateChangedEventArgs args) { };
    }

    /// <summary>
    /// Binds the given handler to the "State Changed" event
    /// </summary>
    public void Bind(EventHandler<StateChangedEventArgs> handler)
    {
        _stateChanged += handler;
    }

    /// <summary>
    /// Binds the given handler to the "State Changed From" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Bind(T state, EventHandler<StateChangedFromEventArgs> handler)
    {
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        _states[state].changedFrom += handler;
        return true;
    }

    /// <summary>
    /// Binds the given handler to the "State Changed To" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Bind(T state, EventHandler<StateChangedToEventArgs> handler)
    {
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        _states[state].changedTo += handler;
        return true;
    }

    /// <summary>
    /// Unbinds the given handler from the "State Changed" event
    /// </summary>
    public void Unbind(EventHandler<StateChangedEventArgs> handler)
    {
        _stateChanged -= handler;
    }

    /// <summary>
    /// Unbinds the given handler from the "State Changed From" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Unbind(T state, EventHandler<StateChangedFromEventArgs> handler)
    {
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        _states[state].changedFrom -= handler;
        return true;
    }

    /// <summary>
    /// Unbinds the given handler from the "State Changed To" event of the given state
    /// </summary>
    /// <returns>
    /// false if the given state does not exist, otherwise true
    /// </returns>
    public bool Unbind(T state, EventHandler<StateChangedToEventArgs> handler)
    {
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        _states[state].changedTo -= handler;
        return true;
    }

    /// <returns>
    /// false if the given state does not exits, otherwise true
    /// </returns>
    public bool SetState(object sender, T state)
    {
        if (!_states.ContainsKey(state))
        {
            return false;
        }

        T previousState = _currentState;
        _currentState = state;
        _stateChanged.Invoke(sender, new StateChangedEventArgs(state, previousState));
        _states[previousState].changedFrom.Invoke(sender, new StateChangedFromEventArgs(state));
        _states[state].changedTo.Invoke(sender, new StateChangedToEventArgs(previousState));

        return true;
    }

    /// <returns>
    /// false if the given state already exits, otherwise true
    /// </returns>
    public bool AddState(T state, StateEvents events = null)
    {
        if (_states.ContainsKey(state))
        {
            return false;
        }

        _states.Add(state, events ?? new StateEvents());

        return true;
    }

    /// <returns>
    /// false if the given state is the current state, otherwise true
    /// </returns>
    public bool RemoveState(T state)
    {
        if (state.Equals(_currentState))
        {
            return false;
        }

        return _states.Remove(state);
    }
}
