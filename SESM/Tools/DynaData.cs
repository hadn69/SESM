using System.Collections.Generic;
using System.Dynamic;

namespace SESM.Tools
{
    public class DynaData : DynamicObject
    {
        private Dictionary<string, DynaData> _children = new Dictionary<string, DynaData>();

        public DynaData()
        {

        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.IgnoreCase? binder.Name.ToLower(): binder.Name;

            if (_children.ContainsKey(name))
            {
                result = _children[name];
                return true;
            }
            else
            {
                DynaData newObject = new DynaData();
                _children.Add(name, newObject);
                result = newObject;
                return true;
            }
        }
        public override bool TrySetMember(SetMemberBinder binder, object result)
        {
            string name = binder.IgnoreCase ? binder.Name.ToLower() : binder.Name;

            if (_children.ContainsKey(name))
            {
                result = _children[name];
                return true;
            }
            else
            {
                DynaData newObject = new DynaData();
                _children.Add(name, newObject);
                result = newObject;
                return true;
            }
        }

    }
}