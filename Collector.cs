using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace XADev.DataCollector
{
    public class Collector : Dictionary<string, object>
    {
        #region Private Elements
        private object myObject;
        private void _add(string key, IEnumerable<Collector> value, string include)
        {
            if (include != "")
            {
                foreach (var item in value)
                {
                    item.Include(include);
                }
            }
            this.Add(key, value);
        }
        private void _add(string key, Collector value)
        {
            this.Add(key, value);
        }
        private void _append(string key, object data)
        {
            var item = FromObject(data);
            this._add(key, item);
        }

        private void _include(string key)
        {
            string[] includes = key.Split('.');
            string now = includes[0];
            string next = "";
            for (var i = 1; i < includes.Length; i++) { next = $"{(next == "" ? "" : ".")}{includes[i]}"; }

            var prop = this.myObject.GetType().GetProperty(now).GetValue(this.myObject);
            if (prop is IEnumerable)
            {
                IEnumerable<Collector> item = FromArray((IEnumerable)prop);
                if (next != "")
                { foreach (var r in item) { r.Include(next); } }
                this.Add(now, item);
            }
            else
            {
                Collector item = FromObject(prop);
                if (next != "")
                    item.Include(next);
                this.Add(now, item);
            }
        }
        #endregion 


        /// <summary>
        /// Builds a collector with data from an array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<Collector> FromArray(IEnumerable data)
        {
            List<Collector> output = new List<Collector>();

            foreach (var item in data)
            {
                output.Add(FromObject(item));
            }

            return output;
        }
        /// <summary>
        /// Builds a collector from an object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Collector FromObject(object data)
        {
            var props = data.GetType().GetProperties();
            Collector output = new Collector();
            output.myObject = data;
            foreach (var prop in props)
            {
                try
                {
                    var value = prop.GetValue(data);
                    if (value is String |
                         value is Decimal |
                         value is Int16 |
                         value is Int32 |
                         value is Int64 |
                         value is Double)
                    {
                        output.Add(prop.Name, value);
                    }
                    else if (value is DateTime)
                    {
                        DateTime? date = (DateTime?)value;
                        if (date != null)
                        {
                            DateTime t = (DateTime)date;
                            output.Add(prop.Name, $"{t.Year}-{t.Month}-{t.Day} {t.Hour}:{t.Minute}:{t.Second}.{t.Millisecond} ({TimeZoneInfo.Local.DaylightName})");
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
            return output;
        }
        /// <summary>
        /// Appends a data array with specified key
        /// </summary>
        /// <param name="key">Output property of array to be appended</param>
        /// <param name="data">An array of data to be dictionaried.</param>
        /// <returns></returns>
        public Collector AppendArray(string key, IEnumerable data)
        {
            List<Collector> list = FromArray(data);
            this.AppendArray(key, list);
            return this;
        }
        /// <summary>
        /// Appends a data array with specified key
        /// </summary>
        /// <param name="key">Output property of array to be appended</param>
        /// <param name="data">An array of data to be dictionaried.</param>
        /// <returns></returns>
        public Collector AppendArray(string key, IEnumerable<Collector> data)
        {
            this._add(key, data, "");
            return this;
        }

        /// <summary>
        /// Appends a property/field to this collector.
        /// </summary>
        /// <param name="key">Name of containing property</param>
        /// <param name="data">Data to be appended</param>
        /// <returns></returns>
        public Collector Append(string key, object data)
        {
            this._append(key, data);
            return this;
        }
        /// <summary>
        /// Include special property. All nested objects are  ignored by default. Use this to include them.
        /// </summary>
        /// <param name="key">Name of special property to be included. Can import nested records. Example: x.Include("Contact.Address")</param>
        /// <returns></returns>
        public Collector Include(string key)
        {
            this._include(key);
            return this;
        }


        /// <summary>
        /// Appends a property/field to this collector.
        /// </summary>
        /// <param name="key">Name of containing property</param>
        /// <param name="data">Data to be appended</param>
        /// <returns></returns>
        public Collector Append(string key, Collector value)
        {
            this.Add(key, value);
            return this;
        }

        /// <summary>
        /// Appends a property/field to this collector.
        /// </summary>
        /// <param name="key">Name of containing property</param>
        /// <param name="data">Data to be appended</param>
        /// <returns></returns>
        public Collector Append(Collector data)
        {
            foreach (var item in data)
            {
                this.Add(item.Key, item.Value);
            }
            return this;
        }

        /// <summary>
        /// Appends a property/field to this collector.
        /// </summary>
        /// <param name="key">Name of containing property</param>
        /// <param name="data">Data to be appended</param>
        /// <returns></returns>
        public Collector Append(object data)
        {
            var d = FromObject(data);
            this.Append(d);
            return this;
        }

        /// <summary>
        /// Seralizes and outputs data as JSON string.
        /// </summary>
        /// <returns></returns>
        public string ToJSON()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}
