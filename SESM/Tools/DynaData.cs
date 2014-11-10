using System.Collections.Generic;
using System.Dynamic;

namespace SESM.Tools
{
    public class DynaData : DynamicObject
    {
        private Dictionary<string, object> _children = new Dictionary<string, object>();

        public DynaData()
        {

        }

        public override bool TryGetMember(GetMemberBinder binder, out object member)
        {
            string name = binder.IgnoreCase ? binder.Name.ToLower() : binder.Name;

            member = _children.ContainsKey(name) ? _children[name] : null;

            return true;
        }
        public override bool TrySetMember(SetMemberBinder binder, object member)
        {
            string name = binder.IgnoreCase ? binder.Name.ToLower() : binder.Name;

            if(_children.ContainsKey(name))
            {
                _children[name] = member;
                return true;
            }
            else
            {
                DynaData newObject = new DynaData();
                _children.Add(name, newObject);
                member = newObject;
                return true;
            }
        }

    }
}