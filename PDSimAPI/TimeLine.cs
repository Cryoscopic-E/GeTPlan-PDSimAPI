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
        private GeTReal lastTime;

        // Events for timeline notifications
        public event EventHandler<ActionEventArgs> ActionStarted;
        public event EventHandler<ActionEventArgs> ActionCompleted;
        public event EventHandler<EffectEventArgs> EffectOccurred;
        public event EventHandler<TimelineEventArgs> TimelineAdvanced;

        public TimeLine(List<GeTActionInstance> plan, bool timedPlan, Dictionary<string, List<GeTEffect>> actionEffects = null)
        {
            Plan = plan;
            currentTime = RealModelFactory.Create(-1.0d); // Start at time -1 so it takes the first action
            InitializeTimeline(timedPlan, actionEffects);
        }

        public GeTReal CurrentTime => currentTime;

        public int GetTotalTimeSteps()
        {
            return timelineEvents.Count;
        }

        private void InitializeTimeline(bool timedPlan, Dictionary<string, List<GeTEffect>> actionEffects)
        {
            timelineEvents = new SortedDictionary<GeTReal, List<TimelineEvent>>(new GeTRealComparer());

            if (!timedPlan)
            {
                var time = RealModelFactory.Create(0.0d);
                // Add all actions to the timeline
                foreach (var action in Plan)
                {
                    AddEvent(time, new TimelineEvent
                    {
                        EventType = TimelineEventType.InstantaneousAction,
                        Action = action
                    });

                    // Add effects for instantaneous actions if we have action effects
                    if (actionEffects != null && actionEffects.TryGetValue(action.ActionName, out var effects))
                    {
                        foreach (var effect in effects)
                        {
                            AddEvent(time, new TimelineEvent
                            {
                                EventType = TimelineEventType.Effect,
                                Action = action,
                                Effect = effect
                            });
                        }
                    }

                    // Fake current time advancement as the action is instantaneous
                    time = RealModelFactory.Create(time.ToDouble() + 1.0d);
                }
                lastTime = time;
                return;
            }
            // Plan is timed
            // Add all start and end events to the timeline
            foreach (var action in Plan)
            {
                // If StartTime is specified, add start event
                if (action.StartTime != null)
                {
                    AddEvent(action.StartTime, new TimelineEvent
                    {
                        EventType = TimelineEventType.ActionStart,
                        Action = action
                    });

                    // Add effects based on the action's start time and the effect timing
                    if (actionEffects != null && actionEffects.TryGetValue(action.ActionName, out var effects))
                    {
                        foreach (var effect in effects)
                        {
                            // Calculate when this effect occurs based on the action start time and the effect timing
                            //var effectTime = CalculateEffectTime(action.StartTime, action.EndTime, effect.OccurrenceTime);
                            if(effect.OccurrenceTime.timePoint.type == TimepointKind.START)
                                AddEvent(action.StartTime, new TimelineEvent
                                    {
                                        EventType = TimelineEventType.Effect,
                                        Action = action,
                                        Effect = effect
                                    });
                        }
                    }
                }

                // If EndTime is specified, add end event
                if (action.EndTime != null)
                {
                    AddEvent(action.EndTime, new TimelineEvent
                    {
                        EventType = TimelineEventType.ActionEnd,
                        Action = action
                    });

                    // Add effects based on the action's end time and the effect timing
                    if (actionEffects != null && actionEffects.TryGetValue(action.ActionName, out var effects))
                    {
                        foreach (var effect in effects)
                        {
                            // Calculate when this effect occurs based on the action end time and the effect timing
                            //var effectTime = CalculateEffectTime(action.EndTime, null, effect.OccurrenceTime);
                            if (effect.OccurrenceTime.timePoint.type == TimepointKind.END)
                                AddEvent(action.EndTime, new TimelineEvent
                                {
                                    EventType = TimelineEventType.Effect,
                                    Action = action,
                                    Effect = effect
                                });
                        }
                    }
                }
            }
            lastTime = timelineEvents.Keys.Last();
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
                OnTimelineAdvanced(new TimelineEventArgs { Time = nextTime, Progress = nextTime.ToDouble()/lastTime.ToDouble()});

                // First process all start events
                foreach (var evt in events.Where(e => e.EventType == TimelineEventType.ActionStart))
                {
                    OnActionStarted(new ActionEventArgs { Action = evt.Action });
                }

                // Then process instantaneous actions
                foreach (var evt in events.Where(e => e.EventType == TimelineEventType.InstantaneousAction))
                {
                    OnActionStarted(new ActionEventArgs { Action = evt.Action });
                    OnActionCompleted(new ActionEventArgs { Action = evt.Action });
                }

                // Then process effects
                foreach (var evt in events.Where(e => e.EventType == TimelineEventType.Effect))
                {
                    OnEffectOccurred(new EffectEventArgs { Action = evt.Action, Effect = evt.Effect });
                }

                // Finally process end events
                foreach (var evt in events.Where(e => e.EventType == TimelineEventType.ActionEnd))
                {
                    OnActionCompleted(new ActionEventArgs { Action = evt.Action });
                }
            }

            return true;
        }

        public void Reset()
        {
            currentTime = RealModelFactory.Create(-1.0d);
        }

        protected virtual void OnActionStarted(ActionEventArgs e)
        {
            ActionStarted?.Invoke(this, e);
        }

        protected virtual void OnActionCompleted(ActionEventArgs e)
        {
            ActionCompleted?.Invoke(this, e);
        }

        protected virtual void OnEffectOccurred(EffectEventArgs e)
        {
            EffectOccurred?.Invoke(this, e);
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
            InstantaneousAction,
            Effect
        }

        private class TimelineEvent
        {
            public TimelineEventType EventType { get; set; }
            public GeTActionInstance Action { get; set; }
            public GeTEffect Effect { get; set; }
        }
    }

    // Event argument classes
    public class  ActionEventArgs : EventArgs
    {
        public GeTActionInstance Action { get; set; }
    }

    public class EffectEventArgs : EventArgs
    {
        public GeTActionInstance Action { get; set; }
        public GeTEffect Effect { get; set; }
    }

    public class TimelineEventArgs : EventArgs
    {
        public GeTReal Time { get; set; }
        public double Progress { get; set; }
    }
}
