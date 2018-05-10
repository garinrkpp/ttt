using Quantum.Core;
using System;
using System.Collections.Generic;

public partial class QuantumGame {

  static class TriggeredSetPool {
    static Stack<HashSet<Int32>[]> _triggeredPool = new Stack<HashSet<Int32>[]>();

    static public void Init(Int32 size) {
      while (_triggeredPool.Count < size) {
        _triggeredPool.Push(CreateNew());
      }
    }

    static public HashSet<Int32>[] Alloc() {
      HashSet<Int32>[] set;

      if (_triggeredPool.Count > 0) {
        set = _triggeredPool.Pop();
      }
      else {
        set = CreateNew();
      }

      return set;
    }

    static public void Free(HashSet<Int32>[] set) {
      if (set != null) {
        for (Int32 i = 0; i < Quantum.Frame.FrameEvents.EVENT_TYPE_COUNT; ++i) {
          set[i].Clear();
        }

        // push on pool
        _triggeredPool.Push(set);
      }
    }

    static HashSet<Int32>[] CreateNew() {
      var set = new HashSet<Int32>[Quantum.Frame.FrameEvents.EVENT_TYPE_COUNT];

      for (Int32 i = 0; i < Quantum.Frame.FrameEvents.EVENT_TYPE_COUNT; ++i) {
        set[i] = new HashSet<Int32>();
      }

      return set;
    }
  }

  Dictionary<Int32, HashSet<Int32>[]> _eventsTriggered;

  void InitEventInvoker(Int32 size) {
    // allocate dictionary with pre-defined capacity
    _eventsTriggered = new Dictionary<Int32, HashSet<Int32>[]>(size);

    // init trigger set pool with empty hashsets
    TriggeredSetPool.Init(size);
  }

  void RaiseEvent(IEventBaseInternal evnt) {
    try {
      evnt.EventRaise();
    }
    catch (Exception exn) {
      Quantum.Log.Error("## Event Callback Threw Exception ##");
      Quantum.Log.Exception(exn);
    }
  }

  void InvokeEvents(Quantum.Frame f) {
    // store previous frame value so we can restore it
    var previousFrameValue = _frame;

    try {
      // set current frame we are invoking the events for
      _frame = f;

      HashSet<Int32>[] triggered;

      // grab or create new triggered set lookup
      if (_eventsTriggered.TryGetValue(f.Number, out triggered) == false) {
        _eventsTriggered.Add(f.Number, triggered = TriggeredSetPool.Alloc());
      }

      // grab event head
      var head = (IEventBaseInternal)(((IFrameInternal)f).EventHead);

      // step over each event
      while (head != null) {
        if (head.EventIsSynced) {
          if (f.IsVerified) {
            RaiseEvent(head);
          }
        }
        else {
          // calculate hash code
          var hash = head.GetHashCode();

          // if this was already raised, do nothing
          if (triggered[head.Id].Contains(hash) == false) {
            // dont trigger this again
            triggered[head.Id].Add(hash);

            // trigger event
            RaiseEvent(head);
          }
        }

        // next
        head = head.EventTail;
      }

      // frame is verified?
      if (f.IsVerified) {
        // remove triggered set
        _eventsTriggered.Remove(f.Number);

        // free it
        TriggeredSetPool.Free(triggered);
      }
    }
    finally {
      // restore frame value
      _frame = previousFrameValue;
    }
  }

}
