using System;
using System.Runtime.CompilerServices;
using Godot;
namespace Fcc{
    public class EventSrc<T>{
        Action<T> tasks;
        public EventSrc(){
            tasks = null;
        }
        public void Emit(T param){
            if(tasks == null)return;
            Action<T> temp = tasks;
            tasks = null;
            temp(param);
        }
        public EventSrcAwaiter Wait(){
            return new EventSrcAwaiter(this);
        }
        public EventSrcCounterAwaiter Count(int count){
            return new EventSrcCounterAwaiter(this, count);
        }
        public class EventSrcAwaiter : INotifyCompletion{
            EventSrc<T> dsp;
            public bool IsCompleted => false;
            T result;
            public void OnCompleted(Action continuation){
                Action<T> theAction = x => {
                    result = x;
                    continuation();
                };
                if(dsp.tasks == null){
                    dsp.tasks = theAction;
                    return;
                }
                dsp.tasks += theAction;
            }
            public T GetResult(){return result;}
            internal EventSrcAwaiter(EventSrc<T> ds){
                dsp = ds;
            }
            public EventSrcAwaiter GetAwaiter(){
                return this;
            }
        }

        public class EventSrcCounterAwaiter : INotifyCompletion{
            EventSrc<T> dsp;
            int count;
            public bool IsCompleted => count <= 0;
            Action<T> theAction;
            public void OnCompleted(Action continuation){
                theAction = x => {
                    if(IsCompleted){
                        continuation();
                        return;
                    }
                    --count;
                    if(dsp.tasks == null){
                        dsp.tasks = theAction;
                        return;
                    }
                    dsp.tasks += theAction;
                };
                if(dsp.tasks == null){
                    dsp.tasks = theAction;
                    return;
                }
                dsp.tasks += theAction;
            }
            public void GetResult(){}
            internal EventSrcCounterAwaiter(EventSrc<T> ds, int c){
                dsp = ds;
                count = c;
            }
            public EventSrcCounterAwaiter GetAwaiter(){
                return this;
            }
            public void Discard(){
                dsp.tasks -= theAction;
            }
        }
    }
    public class EventCounter<T>{
        public bool elapsed{get; private set;}
        EventSrc<T> ev;
        EventSrc<T>.EventSrcCounterAwaiter evCounterAw;
        public void Rewind(int count){
            elapsed = false;
            evCounterAw?.Discard();
            evCounterAw = ev.Count(count);
            new Action(async () => {
                await evCounterAw;
                elapsed = true;
            })();
        }
        public EventCounter(EventSrc<T> eventSrc){
            ev = eventSrc;
            evCounterAw = null;
            elapsed = true;
        }
    }


}