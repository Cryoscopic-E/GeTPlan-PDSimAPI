using GeTPlanModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDSimAPI
{
    public class TimeLine
    {
        public List<GeTActionInstance> Plan { get; set; }
        private SortedDictionary<GeTReal, List<TimelineEvent>> timelineEvents;
        private GeTReal currentTime;

        // Events for timeline notifications
        public event EventHandler<ActionEventArgs> ActionStarted;
        public event EventHandler<ActionEventArgs> ActionCompleted;
        public event EventHandler<TimelineEventArgs> TimelineAdvanced;

        public TimeLine(List<GeTActionInstance> plan, bool timedPlan)
        {
            Plan = plan;
            currentTime = RealModelFactory.Create(0.0d); // Start at time 0
            InitializeTimeline(timedPlan);
        }

        public GeTReal CurrentTime => currentTime;

        private void InitializeTimeline(bool timedPlan)
        {
            timelineEvents = new SortedDictionary<GeTReal, List<TimelineEvent>>(new GeTRealComparer());
            
            if (!timedPlan)
            {
                // Add all actions to the timeline
                foreach (var action in Plan)
                {
                    AddEvent(currentTime, new TimelineEvent
                    {
                        EventType = TimelineEventType.InstantaneousAction,
                        Action = action
                    });
                }
                return;
            }


            // Add all start and end events to the timeline
            foreach (var action in Plan)
            {
                // If EndTime is specified, add end event
                if (action.EndTime != null)
                {
                    AddEvent(action.EndTime, new TimelineEvent
                    {
                        EventType = TimelineEventType.ActionEnd,
                        Action = action
                    });
                }
                // If StartTime is specified, add start event
                if (action.StartTime != null)
                {
                    AddEvent(action.StartTime, new TimelineEvent
                    {
                        EventType = TimelineEventType.ActionStart,
                        Action = action
                    });
                }
            }
        }

        private void AddEvent(GeTReal time, TimelineEvent timelineEvent)
        {
            if (!timelineEvents.ContainsKey(time))
            {
                timelineEvents[time] = new List<TimelineEvent>();
            }
            timelineEvents[time].Add(timelineEvent);
        }

        public bool MoveNext()
        {
            // Find next time point after current time
            var nextTimePoints = timelineEvents.Keys.Where(time => time.ToDouble() > currentTime.ToDouble()).ToList();
            if (nextTimePoints.Count == 0)
                return false;

            var nextTime = nextTimePoints.First();
            currentTime = nextTime;

            // Process all events at this time point
            if (timelineEvents.TryGetValue(nextTime, out var events))
            {
                // Raise the TimelineAdvanced event
                OnTimelineAdvanced(new TimelineEventArgs { Time = nextTime });

                // Process all events at this time point
                foreach (var evt in events)
                {
                    switch (evt.EventType)
                    {
                        case TimelineEventType.ActionStart:
                            OnActionStarted(new ActionEventArgs { Action = evt.Action });
                            break;

                        case TimelineEventType.ActionEnd:
                            OnActionCompleted(new ActionEventArgs { Action = evt.Action });
                            break;

                        case TimelineEventType.InstantaneousAction:
                            OnActionStarted(new ActionEventArgs { Action = evt.Action });
                            OnActionCompleted(new ActionEventArgs { Action = evt.Action });
                            break;
                    }
                }
            }

            return true;
        }

        public void Reset()
        {
            currentTime = RealModelFactory.Create(0.0d);
        }

        public IEnumerable<GeTActionInstance> GetActiveActionsAt(GeTReal time)
        {
            return Plan.Where(action =>
                (action.StartTime == null || action.StartTime.ToDouble() <= time.ToDouble()) &&
                (action.EndTime == null || action.EndTime.ToDouble() > time.ToDouble()));
        }

        public IEnumerable<GeTActionInstance> GetActiveActions()
        {
            return GetActiveActionsAt(currentTime);
        }

        protected virtual void OnActionStarted(ActionEventArgs e)
        {
            ActionStarted?.Invoke(this, e);
        }

        protected virtual void OnActionCompleted(ActionEventArgs e)
        {
            ActionCompleted?.Invoke(this, e);
        }

        protected virtual void OnTimelineAdvanced(TimelineEventArgs e)
        {
            TimelineAdvanced?.Invoke(this, e);
        }

        // Helper classes for the timeline implementation

        private enum TimelineEventType
        {
            ActionStart,
            ActionEnd,
            InstantaneousAction
        }

        private class TimelineEvent
        {
            public TimelineEventType EventType { get; set; }
            public GeTActionInstance Action { get; set; }
        }
    }

    // Event argument classes

    public class ActionEventArgs : EventArgs
    {
        public GeTActionInstance Action { get; set; }
    }

    public class TimelineEventArgs : EventArgs
    {
        public GeTReal Time { get; set; }
    }
}
