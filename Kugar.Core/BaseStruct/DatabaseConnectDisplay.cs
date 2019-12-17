using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.BaseStruct
{
    [Serializable]
    public class DatabaseConnectDisplay : IEquatable<DatabaseConnectDisplay>
    {
        private string _connectString = string.Empty;

        [NonSerialized]
        private object _lockerObj=new object();
        [NonSerialized]
        private string _displayConnectString = null;

        
        public string ConnectString
        {
            get { return _connectString; }
            set
            {
                lock (_lockerObj)
                {
                    _connectString = value;
                    _displayConnectString = GetConnectString(value);
                }
                
            }
        }

        public string DisplayConnectString
        {
            get
            {
                return _displayConnectString;
            }
        }

        public override string ToString()
        {
            return _connectString;
        }

        private string GetConnectString(string connectStr)
        {
            var t = new System.Data.Common.DbConnectionStringBuilder();

            t.ConnectionString = connectStr;
            t["Password"] = "******";

            var ret = t.ConnectionString;

            t.Clear();
            

            return ret;

        }

        public bool Equals(DatabaseConnectDisplay other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._connectString, _connectString);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (DatabaseConnectDisplay)) return false;
            return Equals((DatabaseConnectDisplay) obj);
        }

        public override int GetHashCode()
        {
            return (_connectString != null ? _connectString.GetHashCode() : 0);
        }

        public static bool operator ==(DatabaseConnectDisplay left, DatabaseConnectDisplay right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DatabaseConnectDisplay left, DatabaseConnectDisplay right)
        {
            return !Equals(left, right);
        }
    }
}
