using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace DotNetOpenId.Session
{
    public class SystemHttpSessionState : ISessionState
    {
        private HttpSessionState sessionstate;

        public SystemHttpSessionState(HttpSessionState value)
        {
            sessionstate = value;
        }

        public object this[string index] { get { return sessionstate[index]; } set { sessionstate[index] = value; } }
        public void Add(string name, object value) { sessionstate.Add(name, value); }
        public void Remove(string name) { sessionstate.Remove(name); }
    }
}
