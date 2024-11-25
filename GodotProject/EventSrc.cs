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
        public class EventSrcAwaiter : INotifyCompletion{
            EventSrc<T> dsp;
            public bool IsCompleted => false;
            T result;
            public void OnCompleted(Action continuation){
                Action<T> theAction = x => {
                    result = x;
                    continuation();
                };
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
    }

}